#!/usr/bin/env python3
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent.parent
ASM_PATH = ROOT / "reference" / "Tecmo_Super_Bowl_NES_Disassembly" / "Bank19_20_on_field_gameplay_loop.asm"
OUT_DIR = ROOT / "content" / "game-data" / "on-field" / "generated"
SUMMARY_OUT_DIR = ROOT / "content" / "game-data" / "bank19_20" / "generated"

START_RE = re.compile(r"^_F\{_([A-Z0-9_]+)")
END_RE = re.compile(r"^_F\}_?([A-Z0-9_]+)?")
GLOBAL_LABEL_RE = re.compile(r"^([A-Z0-9_]+):")
CONST_RE = re.compile(r"^([A-Z0-9_]+(?:\[\])?)\s*=\s*([^;]+?)(?:\s*;.*)?$")

SECTION_NOTES = {
    "GAME_PLAY_START_CHECK_FOR_KICK_TEAM": "Top-level on-field entry routing that decides which kickoff-side phase starts the live play host.",
    "LOAD_UPDATE_PLAY_CODE_FUNCTIONS": "Bulk script-assignment and reassignment helpers that copy Bank5_6 reaction pointers into player RAM and seed the per-player command runner.",
    "DEFENDER_CHANGE_BEFORE_HIKE": "Pre-snap defender-selection and snap-gating logic that also primes the active player to re-enter Bank21_22 command execution when the ball is snapped.",
    "CHECK_SNAP_PUNT": "Punt snap-gating logic that shares the same pre-snap/control-handoff boundary as the broader defender-change flow.",
    "SET_PLAYERS_CLOSE_TO_PASS": "Pass-target and nearby-defender prioritization plus one-shot command priming for the jump/dive pass-contest handlers in Bank21_22.",
    "UPDATE_STATS": "Post-play stat-accounting family that should stay represented in the bank conversion but move into a dedicated accounting service in modern code.",
    "CUTSCENE_SEQUENCE_PTRS_AND_SEQUENCES": "Large cutscene-sequence table block that is semantically part of on-field outcome presentation, not controller flow.",
}

SECTION_CARRY_FORWARD = {
    "LOAD_UPDATE_PLAY_CODE_FUNCTIONS": {
        "reason": "Seeds COMMAND_COUNTER and command-return addresses so players resume through Bank21_22's DO_NEXT_PLAYER_COMMAND entrypoint.",
        "symbols": ["JUMP_DO_NEXT_PLAYER_COMMAND"],
    },
    "DEFENDER_CHANGE_BEFORE_HIKE": {
        "reason": "Pre-snap snap handlers set PLAY_CODE_ADDR and command-return state for the active player, creating a hard boundary with Bank21_22 command semantics.",
        "symbols": ["JUMP_DO_NEXT_PLAYER_COMMAND"],
    },
    "CHECK_SNAP_PUNT": {
        "reason": "Snap gating remains Bank19_20 host logic, but the moment of handoff to active command execution must stay visible in the Bank21_22 notes.",
        "symbols": ["JUMP_DO_NEXT_PLAYER_COMMAND"],
    },
    "SET_PLAYERS_CLOSE_TO_PASS": {
        "reason": "Pass-target ranking directly primes Bank21_22 jump/dive pass-contest handlers and should be revisited when that bank's interaction edge cases are converted.",
        "symbols": ["JUMP_WR_JUMP_DIVE_CHECK_PASS", "JUMP_DEF_JUMP_DIVE_CHECK_PASS"],
    },
}

EXTERNAL_DEPENDENCIES = [
    {
        "symbol": "JUMP_DO_NEXT_PLAYER_COMMAND",
        "sourceBank": "Bank21_22_play_commands_on_field_logic.asm",
        "dependencyKind": "runtime-entrypoint",
        "notes": "Per-player script execution re-entrypoint used after Bank19_20 assigns or retargets play-script addresses.",
    },
    {
        "symbol": "JUMP_WR_JUMP_DIVE_CHECK_PASS",
        "sourceBank": "Bank21_22_play_commands_on_field_logic.asm",
        "dependencyKind": "runtime-entrypoint",
        "notes": "Receiver-side jump/dive pass-contest handler primed by Bank19_20 pass-target logic.",
    },
    {
        "symbol": "JUMP_DEF_JUMP_DIVE_CHECK_PASS",
        "sourceBank": "Bank21_22_play_commands_on_field_logic.asm",
        "dependencyKind": "runtime-entrypoint",
        "notes": "Defender-side jump/dive pass-contest handler primed by Bank19_20 pass-target logic.",
    },
    {
        "symbol": "JUMP_DRAW_GAMEFIELD",
        "sourceBank": "Bank23_draw_field_ball_ani_coll_check.asm",
        "dependencyKind": "presentation-entrypoint",
        "notes": "Field-draw task entrypoint invoked by Bank19_20 after updating scroll/window state.",
    },
    {
        "symbol": "JUMP_START_BANNER_TASK",
        "sourceBank": "Bank23_draw_field_ball_ani_coll_check.asm",
        "dependencyKind": "presentation-entrypoint",
        "notes": "Banner/status-bar task entrypoint used by Bank19_20 presentation helpers.",
    },
    {
        "symbol": "JUMP_START_COLL_CHECK_TASK",
        "sourceBank": "Bank23_draw_field_ball_ani_coll_check.asm",
        "dependencyKind": "simulation-entrypoint",
        "notes": "Collision-check task entrypoint used during cutscene/presentation setup.",
    },
    {
        "symbol": "LOAD_ALL_SKILLS_ROUTINE",
        "sourceBank": "Bank27_misc.asm",
        "dependencyKind": "data-hydration-entrypoint",
        "notes": "Bulk player-skill load invoked during on-field setup.",
    },
    {
        "symbol": "LOAD_SINGLE_PLAYER_SKILLS_ROUTINE",
        "sourceBank": "Bank27_misc.asm",
        "dependencyKind": "data-hydration-entrypoint",
        "notes": "Single-player skill load used for kickers, returners, and similar setup adjustments.",
    },
    {
        "symbol": "JUMP_PLAYER_INJURED",
        "sourceBank": "Bank17_18_main_game_loop.asm",
        "dependencyKind": "orchestration-entrypoint",
        "notes": "External jump used by the injury flow to hand control back to the broader game-loop bank.",
    },
    {
        "symbol": "JUMP_CHANGE_PLAYERS",
        "sourceBank": "Bank17_18_main_game_loop.asm",
        "dependencyKind": "orchestration-entrypoint",
        "notes": "External jump used by roster/injury replacement flow.",
    },
    {
        "symbol": "JUMP_UPDATE_LARGE_SCOREBOARD",
        "sourceBank": "Bank17_18_main_game_loop.asm",
        "dependencyKind": "presentation-entrypoint",
        "notes": "Scoreboard update jump used by Bank19_20 scoring/outcome helpers.",
    },
]

SCRIPT_POINTER_FAMILY_METADATA = {
    "OFF_PLAYERS_CHEER_PLAY_PTRS[]": ("offense", "cheer/celebration"),
    "INT_RETURN_DEFENSE_PLAY_PTRS[]": ("offense", "interception-return defense"),
    "OFF_RECOVER_ONSIDE_PLAY_PTRS[]": ("offense", "recover-onside"),
    "OFF_RECOVER_BALL_PLAY_PTRS[]": ("offense", "ball-recovery"),
    "PUNT_COVERAGE_PLAY_PTRS[]": ("offense", "punt coverage"),
    "OFF_ONSIDE_KICK_RET_PLAY_PTRS[]": ("offense", "onside return"),
    "OFF_RECOVERS_OWN_FUM_PLAY_PTRS[]": ("offense", "own-fumble recovery"),
    "OFF_DEFENDS_LOST_FUM_PLAY_PTRS[]": ("offense", "defend lost fumble"),
    "OFF_PLAYERS_CRY_PLAY_PTRS[]": ("offense", "cry/post-failure"),
    "DEF_PLAYERS_CRY_PLAY_PTRS[]": ("defense", "cry/post-failure"),
    "INT_RETURN_PLAY_PTRS[]": ("defense", "interception return"),
    "DEF_RECOVER_ONSIDE_PLAY_PTRS[]": ("defense", "recover-onside"),
    "DEF_RECOVER_BALL_PLAY_PTRS[]": ("defense", "ball-recovery"),
    "PUNT_RETURN_PLAY_PTRS[]": ("defense", "punt return"),
    "DEF_ONSIDE_KICK_RET_PLAY_PTRS[]": ("defense", "onside return"),
    "FUM_RET_DEF_PLAY_PTRS[]": ("defense", "fumble return defense"),
    "DEF_RET_LOST_FUM_PLAY_PTRS[]": ("defense", "return after lost fumble"),
    "DEF_PLAYERS_CHEER_PLAY_PTRS[]": ("defense", "cheer/celebration"),
    "CHASE_BALL_CARRIER_PLAY_PTRS[]": ("defense", "chase ball carrier"),
}


def read_lines() -> list[str]:
    return ASM_PATH.read_text(encoding="latin-1").splitlines()


def classify_section(section_name: str) -> tuple[str, str]:
    play_phase_sections = {
        "GAME_PLAY_START_CHECK_FOR_KICK_TEAM",
        "P2_KICKOFF",
        "P1_PLAY_SELECT_AND_PLAY_LOAD",
        "P1_RUN_PLAY",
        "P1_PLAY_OVER_NORMAL",
        "P1_PASS_PLAY",
        "P1_SACK_OR_SCRAMBLE",
        "P1_SPECIAL_TEAMS_PLAY_TYPE_CHECK",
        "P1_PUNT_PLAY",
        "P1_FG_PLAY",
        "P1_ONSIDES_RETURN",
        "P1_PASS_TIPPED_RESULT",
        "P1_SAFETIED",
        "P1_TD",
        "P1_INTERCEPTED",
        "P1_TO_P2_POSSESSION_CHANGE",
        "P1_KICKOFF",
        "P2_PLAY_SELECT_AND_PLAY_LOAD",
        "P2_RUN_PLAY",
        "P2_PLAY_OVER_NORMAL",
        "P2_PASS_PLAY",
        "P2_SACK_OR_SCRAMBLE",
        "P2_SPECIAL_TEAMS_PLAY_TYPE_CHECK",
        "P2_PUNT_PLAY",
        "P2_FG_PLAY",
        "P2_ONSIDES_RETURN",
        "P2_PASS_TIPPED_RESULT",
        "P2_SAFETIED",
        "P2_TD",
        "P2_INTERCEPTED",
        "P2_TO_P1_POSSESSION_CHANGE",
    }

    play_outcome_sections = {
        "CHECK_FOR_FIRST_DOWN_OR_TOD",
        "UPDATE_HASHMARK_FOR_NEXT_SNAP",
        "CHECK_FOR_TD",
        "CHECK_FOR_TOUCHBACK",
        "CHECK_FOR_SAFETY",
        "CHECK_FOR_PLAY_OVER",
        "CHECK_FOR_FUMBLES_TOSS_AND_NORMAL",
        "ONSIDE_AND_FUMBLE_RECOVERY_LOGIC",
        "P1_RECOVERS_FUMBLE",
        "P2_RECOVERS_FUMBLE",
        "MISC_FUMBLE_FUNCTIONS",
        "CHECK_FOR_QTR_OVER",
        "CLEAR_VARIABLES_FOR_XP_KICKOFF",
    }

    service_groups = {
        "END_SPECIFIC_TASKS": "task-coordination",
        "SET_GAME_STATUS_ON_FIELD_START_PLAYER_TASK": "task-coordination",
        "DEFENDER_CHANGE_BEFORE_HIKE": "pre-snap-control",
        "CHECK_SNAP_PUNT": "pre-snap-control",
        "SET_ONFIELD_SONG": "presentation",
        "LOAD_P1_OR_P2_OFF_PLAY_INFO": "play-assignment",
        "LOAD_OFF_FORMATIONS": "play-assignment",
        "LOAD_DEF_PLAY_INFO": "play-assignment",
        "LOAD_UPDATE_PLAY_CODE_FUNCTIONS": "play-assignment",
        "LOAD_SKILLS": "roster-skill-hydration",
        "STOP_CURRENT_SONG": "presentation",
        "MAN_CONTROLLED_PLAYER_FUNCTIONS": "pre-snap-control",
        "CPU_PLAY_LOGIC": "cpu-decision-support",
        "SIDE_CHANGE_BANNER_AND_SONG": "presentation",
        "SET_PLAYERS_CLOSE_TO_PASS": "pass-targeting",
        "UPDATE_SCROLL_LIMITS": "presentation",
        "START_DRAW_GAME_FIELD": "presentation",
        "UPDATE_STATS": "stats-and-distance",
        "CALCULATE_PLAY_DISTANCE": "stats-and-distance",
        "INJURY_CHECK_NORMAL_AND_SKIP": "injury-and-cutscene",
        "CHECK_IF_PLAYER_CAN_BE_INJURED": "injury-and-cutscene",
        "PLAYER_CHANGE_INJURY": "injury-and-cutscene",
        "CUTSCENE": "injury-and-cutscene",
        "GENERATE_CUTSCENE_RANDOM": "injury-and-cutscene",
        "UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS": "pass-targeting",
        "INJURY_ANIMATION": "injury-and-cutscene",
        "UPDATE_LOS_MARKERS": "presentation",
        "CUTSCENE_SEQUENCE_PTRS_AND_SEQUENCES": "injury-and-cutscene",
        "CHECK_FOR_UPDATE_BANNER": "presentation",
        "UPDATE_SCORE_FUNCTIONS": "presentation",
        "DRAW_RECOVER": "presentation",
    }

    if section_name in play_phase_sections:
        return "controller", "play-phase-routing"

    if section_name in play_outcome_sections:
        return "controller", "play-outcome"

    if section_name in service_groups:
        return "supporting-service", service_groups[section_name]

    raise KeyError(f"Unhandled section classification: {section_name}")


def parse_sections(lines: list[str]) -> list[dict[str, object]]:
    sections: list[dict[str, object]] = []
    stack: list[dict[str, object]] = []

    for line_no, line in enumerate(lines, start=1):
        stripped = line.strip()
        start_match = START_RE.match(stripped)
        if start_match:
            section_name = start_match.group(1)
            modern_owner, responsibility_group = classify_section(section_name)
            stack.append(
                {
                    "sectionName": section_name,
                    "sourceStartLine": line_no,
                    "sourceStartMarker": stripped,
                    "sourceLines": [],
                    "labels": [],
                    "modernOwner": modern_owner,
                    "responsibilityGroup": responsibility_group,
                    "notes": SECTION_NOTES.get(section_name, ""),
                    "carryForwardToBank21_22": section_name in SECTION_CARRY_FORWARD,
                    "bank21_22CarryForwardReason": SECTION_CARRY_FORWARD.get(section_name, {}).get("reason"),
                    "bank21_22BridgeSymbols": SECTION_CARRY_FORWARD.get(section_name, {}).get("symbols", []),
                    "depth": len(stack),
                    "parentSectionName": stack[-1]["sectionName"] if stack else None,
                }
            )
            continue

        if not stack:
            continue

        end_match = END_RE.match(stripped)
        if end_match:
            current = stack.pop()
            current["sourceEndLine"] = line_no
            current["sourceEndMarker"] = stripped
            current["lineCount"] = line_no - int(current["sourceStartLine"]) + 1
            current["globalLabelCount"] = len(current["labels"])
            sections.append(finalize_section(current))
            continue

        for current in stack:
            current["sourceLines"].append((line_no, line))

        label_match = GLOBAL_LABEL_RE.match(stripped)
        if label_match:
            label_record = {
                "label": label_match.group(1),
                "line": line_no,
            }
            for current in stack:
                current["labels"].append(label_record)

    if stack:
        raise ValueError(f"Unterminated section: {stack[-1]['sectionName']}")

    sections.sort(key=lambda section: (int(section["sourceStartLine"]), int(section["depth"])))
    return sections


def finalize_section(section: dict[str, object]) -> dict[str, object]:
    labels = section.pop("labels")
    source_lines: list[tuple[int, str]] = section.pop("sourceLines")

    dependency_hits: list[str] = []
    bank21_symbols = set(SECTION_CARRY_FORWARD.get(section["sectionName"], {}).get("symbols", []))
    for dependency in EXTERNAL_DEPENDENCIES:
        if any(dependency["symbol"] in line for _, line in source_lines):
            dependency_hits.append(dependency["symbol"])
            if dependency["sourceBank"].startswith("Bank21_22"):
                bank21_symbols.add(dependency["symbol"])

    section["labels"] = labels
    section["primaryEntryLabels"] = [label["label"] for label in labels[: min(6, len(labels))]]
    section["externalDependencySymbols"] = dependency_hits
    section["bank21_22BridgeSymbols"] = sorted(bank21_symbols)
    return section


def parse_constants(lines: list[str]) -> tuple[list[dict[str, object]], list[dict[str, object]], list[dict[str, object]]]:
    entry_points: list[dict[str, object]] = []
    script_pointer_families: list[dict[str, object]] = []
    external_jump_constants: list[dict[str, object]] = []

    bank_jump_start = next(i for i, line in enumerate(lines) if line.strip().startswith("BANK_JUMP_ON_FIELD_GAMEPLAY_START:"))
    preamble = list(enumerate(lines[:bank_jump_start], start=1))

    for line_no, line in preamble:
        stripped = line.strip()
        const_match = CONST_RE.match(stripped)
        if not const_match:
            continue

        name = const_match.group(1)
        value = const_match.group(2).strip()
        if name in SCRIPT_POINTER_FAMILY_METADATA:
            side, purpose = SCRIPT_POINTER_FAMILY_METADATA[name]
            script_pointer_families.append(
                {
                    "sourceLabel": name,
                    "address": value,
                    "teamSide": side,
                    "purpose": purpose,
                    "line": line_no,
                }
            )
            continue

        if name in {
            "JUMP_DO_NEXT_PLAYER_COMMAND",
            "JUMP_WR_JUMP_DIVE_CHECK_PASS",
            "JUMP_DEF_JUMP_DIVE_CHECK_PASS",
            "JUMP_DRAW_GAMEFIELD",
            "JUMP_START_BANNER_TASK",
            "JUMP_START_COLL_CHECK_TASK",
            "LOAD_ALL_SKILLS_ROUTINE",
            "LOAD_SINGLE_PLAYER_SKILLS_ROUTINE",
        }:
            external_jump_constants.append(
                {
                    "symbol": name,
                    "value": value,
                    "line": line_no,
                }
            )

    for line_no, line in enumerate(lines, start=1):
        stripped = line.strip()
        if stripped == "BANK_JUMP_ON_FIELD_GAMEPLAY_START:":
            entry_points.append(
                {
                    "sourceLabel": "BANK_JUMP_ON_FIELD_GAMEPLAY_START",
                    "targetLabel": "ON_FIELD_GAMEPLAY_START",
                    "line": line_no,
                    "notes": "Primary bank entrypoint for the on-field gameplay host.",
                }
            )
        elif stripped == "BANK_JUMP_SKP_VS_SKP_INJURY_START:":
            entry_points.append(
                {
                    "sourceLabel": "BANK_JUMP_SKP_VS_SKP_INJURY_START",
                    "targetLabel": "SKP_VS_SKP_INJURY_START",
                    "line": line_no,
                    "notes": "Secondary bank entrypoint for skip-vs-skip injury handling.",
                }
            )

    return entry_points, script_pointer_families, external_jump_constants


def build_inventory(lines: list[str]) -> dict[str, object]:
    sections = parse_sections(lines)
    entry_points, script_pointer_families, external_jump_constants = parse_constants(lines)

    return {
        "sourceFile": str(ASM_PATH.relative_to(ROOT)),
        "entryPoints": entry_points,
        "scriptPointerFamilies": script_pointer_families,
        "externalJumpConstants": external_jump_constants,
        "externalDependencies": EXTERNAL_DEPENDENCIES,
        "sections": sections,
    }


def build_summary(inventory: dict[str, object]) -> dict[str, object]:
    sections = inventory["sections"]
    controller_sections = [section for section in sections if section["modernOwner"] == "controller"]
    service_sections = [section for section in sections if section["modernOwner"] == "supporting-service"]
    carry_forward_sections = [section for section in sections if section["carryForwardToBank21_22"]]

    dependency_counts: dict[str, int] = {}
    for dependency in inventory["externalDependencies"]:
        dependency_counts[dependency["sourceBank"]] = dependency_counts.get(dependency["sourceBank"], 0) + 1

    return {
        "sourceFile": inventory["sourceFile"],
        "sectionCount": len(sections),
        "controllerSectionCount": len(controller_sections),
        "supportingServiceSectionCount": len(service_sections),
        "entryPointCount": len(inventory["entryPoints"]),
        "scriptPointerFamilyCount": len(inventory["scriptPointerFamilies"]),
        "carryForwardToBank21_22SectionCount": len(carry_forward_sections),
        "carryForwardToBank21_22Sections": [section["sectionName"] for section in carry_forward_sections],
        "externalDependencyCountsByBank": dependency_counts,
        "responsibilityGroups": sorted({section["responsibilityGroup"] for section in sections}),
    }


def write_json(path: Path, payload: dict[str, object]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2) + "\n")


def main() -> None:
    lines = read_lines()
    inventory = build_inventory(lines)
    summary = build_summary(inventory)

    write_json(OUT_DIR / "bank19_20-section-map.json", inventory)
    write_json(SUMMARY_OUT_DIR / "summary.json", summary)


if __name__ == "__main__":
    main()
