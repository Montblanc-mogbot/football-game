#!/usr/bin/env python3
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
ASM_PATH = ROOT / "reference" / "Tecmo_Super_Bowl_NES_Disassembly" / "Bank1_2_team_data.asm"
OUT_DIR = ROOT / "content" / "reference" / "bank12" / "generated"

ROSTER_SLOTS = [
    "QB1", "QB2", "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4",
    "TE1", "TE2", "C", "LG", "RG", "LT", "RT", "RE", "NT", "LE",
    "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS", "SS", "K", "P",
]

ATTRIBUTE_VALUES = {
    "ATTRIBUTE_6": 6,
    "ATTRIBUTE_13": 13,
    "ATTRIBUTE_19": 19,
    "ATTRIBUTE_25": 25,
    "ATTRIBUTE_31": 31,
    "ATTRIBUTE_38": 38,
    "ATTRIBUTE_44": 44,
    "ATTRIBUTE_50": 50,
    "ATTRIBUTE_56": 56,
    "ATTRIBUTE_63": 63,
    "ATTRIBUTE_69": 69,
    "ATTRIBUTE_75": 75,
    "ATTRIBUTE_81": 81,
    "ATTRIBUTE_88": 88,
    "ATTRIBUTE_94": 94,
    "ATTRIBUTE_100": 100,
}

COMMON_FIELDS = ["RushingPower", "RunningSpeed", "MaximumSpeed", "HittingPower", "FaceIdentifier"]
ROLE_FIELDS = {
    "QB": ["PassingSpeed", "PassControl", "AccuracyOfPassing", "AvoidPassBlock"],
    "SKILL": ["BallControl", "Receptions"],
    "OL": [],
    "DEF": ["PassInterceptions", "Quickness"],
    "KP": ["KickOrPuntAbility", "AvoidKickBlock"],
}


def read_lines() -> list[str]:
    return ASM_PATH.read_text().splitlines()


def find_section(lines: list[str], start_marker: str, end_marker: str) -> list[tuple[int, str]]:
    start = next(i for i, line in enumerate(lines) if start_marker in line)
    end = next(i for i, line in enumerate(lines[start + 1 :], start + 1) if end_marker in line)
    return list(enumerate(lines[start:end], start + 1))


def parse_team_order(lines: list[str]) -> list[str]:
    section = find_section(lines, "_F{TEAM_PLAYER_NAMES_TEAM_PTR_TABLE", "_F}_PLAYER_NAMES_TEAM_PTR_TABLE")
    team_labels: list[str] = []
    for _, line in section:
        if ".WORD" not in line:
            continue
        payload = line.split(".WORD", 1)[1]
        team_labels.extend([part.strip() for part in payload.split(",") if part.strip()])
    return team_labels


def parse_team_lists(lines: list[str], team_labels: list[str]) -> dict[str, list[str]]:
    section = find_section(lines, "_F{_PLAYER_NAME_POINTERS", "_F}_PLAYER_NAME_POINTERS")
    result: dict[str, list[str]] = {}
    current: str | None = None
    for _, line in section:
        label_match = re.match(r"^([A-Z0-9_]+):", line.strip())
        if label_match:
            label = label_match.group(1)
            if label in team_labels:
                current = label
                result[current] = []
                continue
            current = None
        if current and ".WORD" in line:
            payload = line.split(".WORD", 1)[1]
            entries = [part.strip() for part in payload.split(",") if part.strip()]
            result[current].extend([entry for entry in entries if entry != "PLAYER_LIST_END"])
    return result


def parse_identity_records(lines: list[str]) -> dict[str, dict[str, object]]:
    section = find_section(lines, "_F{_PLAYER_NUMBERS_AND_NAMES", "_F{_PLAYER_ABILITIES")
    result: dict[str, dict[str, object]] = {}
    pattern = re.compile(r'^([A-Z0-9_]+):\s*\.DB\s*\$([0-9A-F]{1,2}),\s*"([^"]*)"')
    for line_no, line in section:
        match = pattern.match(line.strip())
        if not match:
            continue
        label, jersey_hex, name = match.groups()
        result[label] = {
            "sourceLabel": label,
            "line": line_no,
            "jerseyNumberHex": f"0x{jersey_hex.upper().zfill(2)}",
            "jerseyNumber": int(jersey_hex, 16),
            "sourceNamePayload": name,
        }
    return result


def role_for_slot(slot: str) -> str:
    if slot.startswith("QB"):
        return "QB"
    if slot in {"C", "LG", "RG", "LT", "RT"}:
        return "OL"
    if slot in {"K", "P"}:
        return "KP"
    if slot.startswith(("RB", "WR", "TE")):
        return "SKILL"
    return "DEF"


def expected_tokens_for_slot(slot: str) -> int:
    role = role_for_slot(slot)
    return 5 if role == "QB" else 3 if role == "OL" else 4


def parse_ability_teams(lines: list[str]) -> list[tuple[str, int, list[str]]]:
    section = find_section(lines, "_F{_PLAYER_ABILITIES", "_F}_PLAYER_ABILITIES")
    result: list[tuple[str, int, list[str]]] = []
    current_label: str | None = None
    current_line = 0
    current_tokens: list[str] = []
    team_label_re = re.compile(r"^([A-Z0-9_]+_ABILITIES):")
    for line_no, line in section:
        stripped = line.strip()
        match = team_label_re.match(stripped)
        if match:
            label = match.group(1)
            if label.startswith(("QB_", "RB_", "WR_", "TE_", "CENTER_", "LEFT_", "RIGHT_", "RE_", "NT_", "LE_", "ROLB_", "RILB_", "LILB_", "LOLB_", "RCB_", "LCB_", "FS_", "SS_", "K_", "P_")):
                continue
            if current_label is not None:
                result.append((current_label, current_line, current_tokens))
            current_label = label
            current_line = line_no
            current_tokens = []
            continue
        if current_label is None:
            continue
        if stripped.startswith("ADD_NIBBLES_AS_BYTE"):
            args = stripped.split("]", 1)[1]
            left, right = [part.strip().split()[0] for part in args.split(";")[0].split(",")]
            current_tokens.append(f"PAIR:{left}:{right}")
        elif stripped.startswith("ADD_FACE_IDENTIFIER"):
            face = stripped.split("]", 1)[1].split(";")[0].strip()
            current_tokens.append(f"FACE:{face}")
    if current_label is not None:
        result.append((current_label, current_line, current_tokens))
    return result


def decode_ability_slot(slot: str, tokens: list[str]) -> dict[str, object]:
    role = role_for_slot(slot)
    expected = expected_tokens_for_slot(slot)
    if len(tokens) != expected:
        raise ValueError(f"slot {slot} expected {expected} tokens, got {len(tokens)}")

    result: dict[str, object] = {"slot": slot, "role": role}

    pair1 = tokens[0].split(":")
    pair2 = tokens[1].split(":")
    face = tokens[2].split(":", 1)[1]

    result["rawTokens"] = tokens
    result["RushingPower"] = {"sourceLabel": pair1[1], "value": ATTRIBUTE_VALUES[pair1[1]]}
    result["RunningSpeed"] = {"sourceLabel": pair1[2], "value": ATTRIBUTE_VALUES[pair1[2]]}
    result["MaximumSpeed"] = {"sourceLabel": pair2[1], "value": ATTRIBUTE_VALUES[pair2[1]]}
    result["HittingPower"] = {"sourceLabel": pair2[2], "value": ATTRIBUTE_VALUES[pair2[2]]}
    result["FaceIdentifier"] = {"hex": face, "value": int(face.replace("$", "0x"), 16)}

    if role == "QB":
        pair3 = tokens[3].split(":")
        pair4 = tokens[4].split(":")
        result["PassingSpeed"] = {"sourceLabel": pair3[1], "value": ATTRIBUTE_VALUES[pair3[1]]}
        result["PassControl"] = {"sourceLabel": pair3[2], "value": ATTRIBUTE_VALUES[pair3[2]]}
        result["AccuracyOfPassing"] = {"sourceLabel": pair4[1], "value": ATTRIBUTE_VALUES[pair4[1]]}
        result["AvoidPassBlock"] = {"sourceLabel": pair4[2], "value": ATTRIBUTE_VALUES[pair4[2]]}
    elif role == "SKILL":
        pair3 = tokens[3].split(":")
        result["BallControl"] = {"sourceLabel": pair3[1], "value": ATTRIBUTE_VALUES[pair3[1]]}
        result["Receptions"] = {"sourceLabel": pair3[2], "value": ATTRIBUTE_VALUES[pair3[2]]}
    elif role == "DEF":
        pair3 = tokens[3].split(":")
        result["PassInterceptions"] = {"sourceLabel": pair3[1], "value": ATTRIBUTE_VALUES[pair3[1]]}
        result["Quickness"] = {"sourceLabel": pair3[2], "value": ATTRIBUTE_VALUES[pair3[2]]}
    elif role == "KP":
        pair3 = tokens[3].split(":")
        result["KickOrPuntAbility"] = {"sourceLabel": pair3[1], "value": ATTRIBUTE_VALUES[pair3[1]]}
        result["AvoidKickBlock"] = {"sourceLabel": pair3[2], "value": ATTRIBUTE_VALUES[pair3[2]]}

    return result


def build_identities(lines: list[str]) -> dict[str, object]:
    team_labels = parse_team_order(lines)
    team_lists = parse_team_lists(lines, team_labels)
    identity_records = parse_identity_records(lines)

    teams = []
    for team_index, team_label in enumerate(team_labels, start=1):
        players = team_lists[team_label]
        if len(players) != len(ROSTER_SLOTS):
            raise ValueError(f"{team_label} expected {len(ROSTER_SLOTS)} players, got {len(players)}")
        team_entries = []
        for slot, player_label in zip(ROSTER_SLOTS, players):
            record = identity_records[player_label]
            team_entries.append({
                "slot": slot,
                "playerLabel": player_label,
                **record,
            })
        teams.append({
            "order": team_index,
            "teamListLabel": team_label,
            "players": team_entries,
        })

    return {
        "sourceFile": str(ASM_PATH.relative_to(ROOT)),
        "teamPointerTableLabel": "STARTING_ADDR_FOR_TEAM_PLAYER_NAMES_PTR_TABLE",
        "teamCount": len(teams),
        "rosterSlotCount": len(ROSTER_SLOTS),
        "rosterSlots": ROSTER_SLOTS,
        "teams": teams,
    }


def build_abilities(lines: list[str], identities: dict[str, object]) -> dict[str, object]:
    ability_teams = parse_ability_teams(lines)
    identity_teams = identities["teams"]
    if len(ability_teams) != len(identity_teams):
        raise ValueError(f"ability team count mismatch: {len(ability_teams)} vs {len(identity_teams)}")

    teams = []
    for identity_team, (ability_label, line_no, tokens) in zip(identity_teams, ability_teams):
        expected_tokens = sum(expected_tokens_for_slot(slot) for slot in ROSTER_SLOTS)
        if len(tokens) != expected_tokens:
            raise ValueError(f"{ability_label} expected {expected_tokens} tokens, got {len(tokens)}")
        slot_entries = []
        cursor = 0
        for slot in ROSTER_SLOTS:
            width = expected_tokens_for_slot(slot)
            slot_entries.append(decode_ability_slot(slot, tokens[cursor:cursor + width]))
            cursor += width
        teams.append({
            "order": identity_team["order"],
            "teamListLabel": identity_team["teamListLabel"],
            "abilityLabel": ability_label,
            "line": line_no,
            "slots": slot_entries,
        })

    return {
        "sourceFile": str(ASM_PATH.relative_to(ROOT)),
        "sectionLabel": "_F{_PLAYER_ABILITIES",
        "attributeScale": [
            {"sourceLabel": label, "nibble": int(value_name.split('_')[1]) if False else nibble, "value": value}
            for nibble, (label, value) in enumerate(ATTRIBUTE_VALUES.items())
        ],
        "teams": teams,
    }


def build_abilities_metadata() -> dict[str, object]:
    return {
        "sourceFile": str(ASM_PATH.relative_to(ROOT)),
        "sectionLabel": "_F{_PLAYER_ABILITIES",
        "attributeScale": [
            {"nibble": index, "sourceLabel": label, "value": value}
            for index, (label, value) in enumerate(ATTRIBUTE_VALUES.items())
        ],
        "commonFields": COMMON_FIELDS,
        "roleFields": ROLE_FIELDS,
        "rosterSlots": [{"slot": slot, "role": role_for_slot(slot), "tokenWidth": expected_tokens_for_slot(slot)} for slot in ROSTER_SLOTS],
    }


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    lines = read_lines()

    identities = build_identities(lines)
    abilities = build_abilities(lines, identities)
    metadata = build_abilities_metadata()
    summary = {
        "sourceFile": str(ASM_PATH.relative_to(ROOT)),
        "teamCount": identities["teamCount"],
        "rosterSlotCount": identities["rosterSlotCount"],
        "identityRecordCount": sum(len(team["players"]) for team in identities["teams"]),
        "abilityTeamCount": len(abilities["teams"]),
        "abilitySlotCount": sum(len(team["slots"]) for team in abilities["teams"]),
    }

    (OUT_DIR / "team-identities.json").write_text(json.dumps(identities, indent=2) + "\n")
    (OUT_DIR / "team-abilities.json").write_text(json.dumps(abilities, indent=2) + "\n")
    (OUT_DIR / "ability-metadata.json").write_text(json.dumps(metadata, indent=2) + "\n")
    (OUT_DIR / "summary.json").write_text(json.dumps(summary, indent=2) + "\n")

    print(json.dumps(summary, indent=2))


if __name__ == "__main__":
    main()
