#!/usr/bin/env python3
from __future__ import annotations

import json
import re
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent.parent
ASM_PATH = ROOT / "reference" / "Tecmo_Super_Bowl_NES_Disassembly" / "Bank5_6_off_def_play_data.asm"
MACROS_PATH = ROOT / "reference" / "Tecmo_Super_Bowl_NES_Disassembly" / "macros" / "play_data_macros.asm"
OUT_PATH = ROOT / "content" / "game-data" / "play-scripts" / "generated" / "bank5_6-play-scripts.json"
SUMMARY_PATH = ROOT / "content" / "game-data" / "bank5_6" / "generated" / "summary.json"

ENTRY_LABEL_RE = re.compile(r"^(OFFENSE|DEFENSE)_PLAYER_REACTION_(\d{3}):$")
SCOPED_LABEL_RE = re.compile(r"^(OFFENSE|DEFENSE)_[A-Z0-9_]+:$")
INSTRUCTION_RE = re.compile(r"^PlayerCommandData\.([A-Za-z0-9_]+)(?:\s+(.*))?$")
VOCAB_RE = re.compile(r"^(PLAYER_COMMAND_DATA_[A-Z0-9]+)\s*=\s*(.+)$")
HEX_RE = re.compile(r"^\$([0-9A-F]{1,2})$", re.IGNORECASE)
IMMEDIATE_HEX_RE = re.compile(r"^#\$([0-9A-F]{1,2})$", re.IGNORECASE)
BINARY_RE = re.compile(r"^%([01]{8})$")

GROUP_MACRO_SPECS = {
    "manCoverageTight": {"opcodeBase": 0x00, "family": "group", "controlFlow": None},
    "manCoverageLoose": {"opcodeBase": 0x10, "family": "group", "controlFlow": None},
    "randomJumpTo": {"opcodeBase": 0x20, "family": "group", "controlFlow": "randomJump"},
    "blockPlayer": {"opcodeBase": 0x30, "family": "group", "controlFlow": None},
    "chopBlockPlayer": {"opcodeBase": 0x40, "family": "group", "controlFlow": None},
    "handoffToPlayer": {"opcodeBase": 0x50, "family": "group", "controlFlow": None},
    "fakeHandoffToPlayer": {"opcodeBase": 0x60, "family": "group", "controlFlow": None},
    "pitchToPlayer": {"opcodeBase": 0x70, "family": "group", "controlFlow": None},
    "motionFollowingPlayer": {"opcodeBase": 0x80, "family": "group", "controlFlow": None},
    "setRouteNumber": {"opcodeBase": 0xA0, "family": "group", "controlFlow": None},
}

FIXED_MACRO_SPECS = {
    "passChance2ReceiversAndPostCatch": {"opcode": 0x91, "family": "group", "controlFlow": None},
    "passChance3ReceiversAndPostCatch": {"opcode": 0x92, "family": "group", "controlFlow": None},
    "passChance4ReceiversAndPostCatch": {"opcode": 0x93, "family": "group", "controlFlow": None},
    "passChance5ReceiversAndPostCatch": {"opcode": 0x94, "family": "group", "controlFlow": None},
    "setPositionFromKickoffB0": {"opcode": 0xB0, "family": "group", "controlFlow": None},
    "setPositionFromKickoffB1": {"opcode": 0xB1, "family": "group", "controlFlow": None},
    "moveDuringKickoff": {"opcode": 0xB4, "family": "group", "controlFlow": None},
    "dropback": {"opcode": 0xC0, "family": "single", "controlFlow": None},
    "COACOMPassTiming": {"opcode": 0xC1, "family": "single", "controlFlow": None},
    "celebrate": {"opcode": 0xC4, "family": "single", "controlFlow": None},
    "cry": {"opcode": 0xC5, "family": "single", "controlFlow": None},
    "COMJumpTo": {"opcode": 0xC7, "family": "single", "controlFlow": "conditionalJumpCom"},
    "basedOnJuiceCOMJumpTo": {"opcode": 0xC8, "family": "single", "controlFlow": "conditionalJumpCpuBoost"},
    "COACOMJumpTo": {"opcode": 0xCA, "family": "single", "controlFlow": "conditionalJumpCoachOrCpu"},
    "block": {"opcode": 0xCC, "family": "single", "controlFlow": None},
    "pullRelative": {"opcode": 0xCD, "family": "single", "controlFlow": None},
    "pullBallPlacement": {"opcode": 0xCE, "family": "single", "controlFlow": None},
    "pullMiddleOfField": {"opcode": 0xCF, "family": "single", "controlFlow": None},
    "setPositionBallPlacement": {"opcode": 0xD0, "family": "single", "controlFlow": None},
    "setPositionMiddleOfField": {"opcode": 0xD1, "family": "single", "controlFlow": None},
    "hikeUnderCenter": {"opcode": 0xD2, "family": "single", "controlFlow": None},
    "hikeFromShotgun": {"opcode": 0xD3, "family": "single", "controlFlow": None},
    "takeSnapUnderCenter": {"opcode": 0xD4, "family": "single", "controlFlow": None},
    "takeSnapFromShotgun": {"opcode": 0xD5, "family": "single", "controlFlow": None},
    "takeSnapForFGXP": {"opcode": 0xD6, "family": "single", "controlFlow": None},
    "moveRelative": {"opcode": 0xD7, "family": "single", "controlFlow": None},
    "moveBallPlacement": {"opcode": 0xD8, "family": "single", "controlFlow": None},
    "moveMiddleOfField": {"opcode": 0xD9, "family": "single", "controlFlow": None},
    "runRush": {"opcode": 0xDA, "family": "single", "controlFlow": None},
    "verticallyMirrorBallCarrier": {"opcode": 0xDB, "family": "single", "controlFlow": None},
    "passRush": {"opcode": 0xDD, "family": "single", "controlFlow": None},
    "computerTakesControl": {"opcode": 0xDF, "family": "single", "controlFlow": None},
    "setRS": {"opcode": 0xE0, "family": "single", "controlFlow": None},
    "setMS": {"opcode": 0xE1, "family": "single", "controlFlow": None},
    "boostRP": {"opcode": 0xE2, "family": "single", "controlFlow": None},
    "boostRS": {"opcode": 0xE3, "family": "single", "controlFlow": None},
    "playerTakesControl": {"opcode": 0xE4, "family": "single", "controlFlow": None},
    "kickoff": {"opcode": 0xE5, "family": "single", "controlFlow": None},
    "punt": {"opcode": 0xE6, "family": "single", "controlFlow": None},
    "fieldGoal": {"opcode": 0xE7, "family": "single", "controlFlow": None},
    "extraPoint": {"opcode": 0xE8, "family": "single", "controlFlow": None},
    "waitForSnap3PointStance": {"opcode": 0xEA, "family": "single", "controlFlow": None},
    "shift": {"opcode": 0xEB, "family": "single", "controlFlow": None},
    "waitForSnap2PointStance": {"opcode": 0xEC, "family": "single", "controlFlow": None},
    "motion": {"opcode": 0xED, "family": "single", "controlFlow": None},
    "qbStance": {"opcode": 0xEE, "family": "single", "controlFlow": None},
    "changePlayerIconToReturner": {"opcode": 0xEF, "family": "single", "controlFlow": None},
    "faceDirection": {"opcode": 0xF0, "family": "single", "controlFlow": None},
    "stand": {"opcode": 0xF3, "family": "single", "controlFlow": None},
    "turn": {"opcode": 0xF4, "family": "single", "controlFlow": None},
    "wait": {"opcode": 0xF5, "family": "single", "controlFlow": None},
    "setHP": {"opcode": 0xF6, "family": "single", "controlFlow": None},
    "boostHP": {"opcode": 0xF7, "family": "single", "controlFlow": None},
    "infiniteLoop": {"opcode": 0xF8, "family": "single", "controlFlow": None},
    "recoverBall": {"opcode": 0xFA, "family": "single", "controlFlow": None},
    "setToGrapple": {"opcode": 0xFC, "family": "single", "controlFlow": None},
    "setToBlock": {"opcode": 0xFD, "family": "single", "controlFlow": None},
    "loopTo": {"opcode": 0xFE, "family": "single", "controlFlow": "loop"},
    "jumpTo": {"opcode": 0xFF, "family": "single", "controlFlow": "jump"},
}

CROSS_PLAYER_MACROS = {
    "manCoverageTight",
    "manCoverageLoose",
    "blockPlayer",
    "chopBlockPlayer",
    "handoffToPlayer",
    "fakeHandoffToPlayer",
    "pitchToPlayer",
    "motionFollowingPlayer",
    "passChance2ReceiversAndPostCatch",
    "passChance3ReceiversAndPostCatch",
    "passChance4ReceiversAndPostCatch",
    "passChance5ReceiversAndPostCatch",
    "setToGrapple",
    "setToBlock",
}

CPU_CONDITIONAL_MACROS = {
    "COMJumpTo",
    "basedOnJuiceCOMJumpTo",
    "COACOMJumpTo",
}

SPECIAL_TEAMS_MACROS = {
    "setPositionFromKickoffB0",
    "setPositionFromKickoffB1",
    "moveDuringKickoff",
    "takeSnapForFGXP",
    "kickoff",
    "punt",
    "fieldGoal",
    "extraPoint",
    "changePlayerIconToReturner",
}

TARGET_ARG_MACROS = {
    "randomJumpTo",
    "COMJumpTo",
    "basedOnJuiceCOMJumpTo",
    "COACOMJumpTo",
    "loopTo",
    "jumpTo",
}

POST_CATCH_MACROS = {
    "passChance2ReceiversAndPostCatch",
    "passChance3ReceiversAndPostCatch",
    "passChance4ReceiversAndPostCatch",
    "passChance5ReceiversAndPostCatch",
}


def read_lines(path: Path) -> list[str]:
    return path.read_text().splitlines()


def parse_args(arg_text: str | None) -> list[str]:
    if arg_text is None:
        return []
    return [part.strip() for part in arg_text.split(",") if part.strip()]


def parse_numeric_token(token: str) -> int | None:
    hex_match = HEX_RE.match(token)
    if hex_match:
        return int(hex_match.group(1), 16)

    immediate_hex_match = IMMEDIATE_HEX_RE.match(token)
    if immediate_hex_match:
        return int(immediate_hex_match.group(1), 16)

    binary_match = BINARY_RE.match(token)
    if binary_match:
        return int(binary_match.group(1), 2)

    return None


def parse_player_slot_vocabulary() -> dict[str, object]:
    resolved_values: dict[str, int] = {}
    aliases: dict[str, str] = {}

    for line in read_lines(MACROS_PATH):
        stripped = line.strip()
        match = VOCAB_RE.match(stripped)
        if match is None:
            continue

        name, raw_value = match.groups()
        raw_value = raw_value.strip()
        numeric_value = parse_numeric_token(raw_value)
        if numeric_value is not None:
            resolved_values[name] = numeric_value
            continue

        aliases[name] = raw_value

    unresolved = True
    while unresolved:
        unresolved = False
        for name, target in list(aliases.items()):
            if target in resolved_values:
                resolved_values[name] = resolved_values[target]
                del aliases[name]
                unresolved = True

    if aliases:
        raise ValueError(f"Unresolved player-slot aliases: {aliases}")

    canonical_order = [
        "PLAYER_COMMAND_DATA_QB1",
        "PLAYER_COMMAND_DATA_RB1",
        "PLAYER_COMMAND_DATA_RB2",
        "PLAYER_COMMAND_DATA_WR1",
        "PLAYER_COMMAND_DATA_WR2",
        "PLAYER_COMMAND_DATA_TE1",
        "PLAYER_COMMAND_DATA_C",
        "PLAYER_COMMAND_DATA_LG",
        "PLAYER_COMMAND_DATA_RG",
        "PLAYER_COMMAND_DATA_LT",
        "PLAYER_COMMAND_DATA_RT",
    ]

    offense_names = [name.removeprefix("PLAYER_COMMAND_DATA_") for name in canonical_order]
    defense_aliases_by_value = {
        resolved_values[name]: name.removeprefix("PLAYER_COMMAND_DATA_")
        for name in (
            "PLAYER_COMMAND_DATA_RE",
            "PLAYER_COMMAND_DATA_NT",
            "PLAYER_COMMAND_DATA_LE",
            "PLAYER_COMMAND_DATA_ROLB",
            "PLAYER_COMMAND_DATA_RILB",
            "PLAYER_COMMAND_DATA_LILB",
            "PLAYER_COMMAND_DATA_LOLB",
            "PLAYER_COMMAND_DATA_RCB",
            "PLAYER_COMMAND_DATA_LCB",
            "PLAYER_COMMAND_DATA_FS",
            "PLAYER_COMMAND_DATA_SS",
        )
    }

    slots: list[dict[str, object]] = []
    for index, constant_name in enumerate(canonical_order):
        value = resolved_values[constant_name]
        slots.append(
            {
                "value": value,
                "hexValue": f"0x{value:02X}",
                "constant": constant_name,
                "offenseName": offense_names[index],
                "defenseAlias": defense_aliases_by_value[value],
            }
        )

    return {
        "slots": slots,
        "constants": {name: resolved_values[name] for name in sorted(resolved_values)},
    }


def get_command_spec(macro_name: str) -> dict[str, object]:
    if macro_name in GROUP_MACRO_SPECS:
        return GROUP_MACRO_SPECS[macro_name]
    if macro_name in FIXED_MACRO_SPECS:
        return FIXED_MACRO_SPECS[macro_name]
    raise ValueError(f"Unknown PlayerCommandData macro: {macro_name}")


def resolve_slot_arg(token: str, vocabulary_constants: dict[str, int]) -> dict[str, object] | None:
    if token not in vocabulary_constants:
        return None

    value = vocabulary_constants[token]
    return {
        "kind": "playerSlot",
        "token": token,
        "value": value,
        "hexValue": f"0x{value:02X}",
    }


def resolve_arg(token: str, vocabulary_constants: dict[str, int]) -> dict[str, object]:
    slot = resolve_slot_arg(token, vocabulary_constants)
    if slot is not None:
        return slot

    numeric_value = parse_numeric_token(token)
    if numeric_value is not None:
        return {
            "kind": "number",
            "token": token,
            "value": numeric_value,
            "hexValue": f"0x{numeric_value:02X}",
        }

    if token.startswith("OFFENSE_PLAYER_REACTION_") or token.startswith("DEFENSE_PLAYER_REACTION_"):
        return {
            "kind": "label",
            "token": token,
        }

    return {
        "kind": "token",
        "token": token,
    }


def resolve_opcode(macro_name: str, args: list[str], vocabulary_constants: dict[str, int]) -> int:
    spec = get_command_spec(macro_name)
    opcode = spec.get("opcode")
    if opcode is not None:
        return int(opcode)

    opcode_base = int(spec["opcodeBase"])
    if not args:
        raise ValueError(f"Grouped macro {macro_name} missing argument for opcode resolution")

    first_arg = args[0]
    slot = resolve_slot_arg(first_arg, vocabulary_constants)
    if slot is not None:
        return opcode_base + int(slot["value"])

    numeric_value = parse_numeric_token(first_arg)
    if numeric_value is None:
        raise ValueError(f"Unable to resolve opcode nibble for {macro_name}: {first_arg}")
    return opcode_base + numeric_value


def parse_bank(vocabulary: dict[str, object]) -> dict[str, object]:
    lines = read_lines(ASM_PATH)
    vocabulary_constants = vocabulary["constants"]  # type: ignore[index]

    sections: dict[str, list[dict[str, object]]] = {
        "OFFENSE": [],
        "DEFENSE": [],
    }
    all_labels: dict[str, dict[str, object]] = {}
    current_section: str | None = None
    current_reaction: dict[str, object] | None = None

    for line_no, line in enumerate(lines, start=1):
        stripped = line.strip()
        if not stripped:
            continue

        if stripped == "OFFENSE_PLAY_DATA:":
            current_section = "OFFENSE"
            current_reaction = None
            continue

        if stripped == "DEFENSE_PLAY_DATA:":
            current_section = "DEFENSE"
            current_reaction = None
            continue

        entry_match = ENTRY_LABEL_RE.match(stripped)
        if entry_match is not None:
            section_name = entry_match.group(1)
            label = stripped[:-1]
            reaction_number = int(entry_match.group(2))
            current_section = section_name
            current_reaction = {
                "reactionId": label,
                "kind": section_name.lower(),
                "sourceOrder": len(sections[section_name]),
                "reactionNumber": reaction_number,
                "entryLabel": label,
                "line": line_no,
                "labels": [
                    {
                        "label": label,
                        "kind": "entry",
                        "line": line_no,
                        "instructionIndex": 0,
                    }
                ],
                "instructions": [],
                "controlFlowEdges": [],
            }
            sections[section_name].append(current_reaction)
            all_labels[label] = {
                "reactionId": label,
                "kind": "entry",
                "line": line_no,
            }
            continue

        scoped_label_match = SCOPED_LABEL_RE.match(stripped)
        if scoped_label_match is not None:
            if current_reaction is None:
                raise ValueError(f"Scoped label outside reaction at line {line_no}: {stripped}")

            label = stripped[:-1]
            if label != current_reaction["reactionId"]:
                instruction_index = len(current_reaction["instructions"])
                if "_LOOP_" in label:
                    label_kind = "loop"
                elif "_JUMP_" in label:
                    label_kind = "jump"
                else:
                    label_kind = "local"
                current_reaction["labels"].append(
                    {
                        "label": label,
                        "kind": label_kind,
                        "line": line_no,
                        "instructionIndex": instruction_index,
                    }
                )
                all_labels[label] = {
                    "reactionId": current_reaction["reactionId"],
                    "kind": label_kind,
                    "line": line_no,
                }
            continue

        instruction_match = INSTRUCTION_RE.match(stripped.split(";", 1)[0].strip())
        if instruction_match is None:
            continue

        if current_reaction is None:
            raise ValueError(f"Instruction outside reaction at line {line_no}: {stripped}")

        macro_name = instruction_match.group(1)
        args = parse_args(instruction_match.group(2))
        spec = get_command_spec(macro_name)
        opcode = resolve_opcode(macro_name, args, vocabulary_constants)
        instruction_index = len(current_reaction["instructions"])

        resolved_args = [resolve_arg(arg, vocabulary_constants) for arg in args]
        instruction: dict[str, object] = {
            "index": instruction_index,
            "line": line_no,
            "sourceMacro": macro_name,
            "family": spec["family"],
            "opcode": f"0x{opcode:02X}",
            "rawArgs": args,
            "resolvedArgs": resolved_args,
        }

        if macro_name in TARGET_ARG_MACROS:
            target_label = args[-1]
            instruction["targetLabel"] = target_label
            current_reaction["controlFlowEdges"].append(
                {
                    "fromInstructionIndex": instruction_index,
                    "kind": spec["controlFlow"],
                    "targetLabel": target_label,
                }
            )

        if macro_name in POST_CATCH_MACROS:
            instruction["postCatchTargetLabel"] = args[0]

        current_reaction["instructions"].append(instruction)

    for section_reactions in sections.values():
        for reaction in section_reactions:
            labels_by_name = {label["label"]: label for label in reaction["labels"]}
            for edge in reaction["controlFlowEdges"]:
                target_label = edge["targetLabel"]
                if target_label not in all_labels:
                    raise ValueError(
                        f"Unresolved control-flow target {target_label} from {reaction['reactionId']} "
                        f"instruction {edge['fromInstructionIndex']}"
                    )
                target_owner = all_labels[target_label]
                edge["targetReactionId"] = target_owner["reactionId"]
                local_label = labels_by_name.get(target_label)
                if local_label is not None:
                    edge["targetInstructionIndex"] = local_label["instructionIndex"]

    return {
        "sourceFile": str(ASM_PATH.relative_to(ROOT)),
        "sharedPlayerSlotVocabulary": vocabulary["slots"],
        "offenseReactions": sections["OFFENSE"],
        "defenseReactions": sections["DEFENSE"],
    }


def build_summary(bank: dict[str, object]) -> dict[str, object]:
    offense_reactions = bank["offenseReactions"]  # type: ignore[index]
    defense_reactions = bank["defenseReactions"]  # type: ignore[index]

    reactions = [*offense_reactions, *defense_reactions]
    macro_counter: Counter[str] = Counter()
    family_counter: Counter[str] = Counter()
    label_counter: Counter[str] = Counter()

    total_instructions = 0
    total_edges = 0
    cross_player_instruction_count = 0
    cpu_conditional_instruction_count = 0
    special_teams_instruction_count = 0

    for reaction in reactions:
        labels = reaction["labels"]
        instructions = reaction["instructions"]
        edges = reaction["controlFlowEdges"]

        total_instructions += len(instructions)
        total_edges += len(edges)

        for label in labels:
            label_counter[str(label["kind"])] += 1

        for instruction in instructions:
            macro_name = str(instruction["sourceMacro"])
            macro_counter[macro_name] += 1
            family_counter[str(instruction["family"])] += 1

            if macro_name in CROSS_PLAYER_MACROS:
                cross_player_instruction_count += 1
            if macro_name in CPU_CONDITIONAL_MACROS:
                cpu_conditional_instruction_count += 1
            if macro_name in SPECIAL_TEAMS_MACROS:
                special_teams_instruction_count += 1

    return {
        "sourceFile": bank["sourceFile"],
        "offenseReactionCount": len(offense_reactions),
        "defenseReactionCount": len(defense_reactions),
        "totalReactionCount": len(reactions),
        "offenseInstructionCount": sum(len(reaction["instructions"]) for reaction in offense_reactions),
        "defenseInstructionCount": sum(len(reaction["instructions"]) for reaction in defense_reactions),
        "totalInstructionCount": total_instructions,
        "offenseLocalLabelCount": sum(len(reaction["labels"]) - 1 for reaction in offense_reactions),
        "defenseLocalLabelCount": sum(len(reaction["labels"]) - 1 for reaction in defense_reactions),
        "totalControlFlowEdgeCount": total_edges,
        "crossPlayerInstructionCount": cross_player_instruction_count,
        "cpuConditionalInstructionCount": cpu_conditional_instruction_count,
        "specialTeamsInstructionCount": special_teams_instruction_count,
        "instructionFamilyCounts": dict(sorted(family_counter.items())),
        "labelKindCounts": dict(sorted(label_counter.items())),
        "commandCounts": dict(sorted(macro_counter.items())),
    }


def write_json(path: Path, payload: dict[str, object]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2) + "\n")


def main() -> None:
    vocabulary = parse_player_slot_vocabulary()
    bank = parse_bank(vocabulary)
    summary = build_summary(bank)

    write_json(OUT_PATH, bank)
    write_json(SUMMARY_PATH, summary)


if __name__ == "__main__":
    main()
