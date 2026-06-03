from __future__ import annotations

import json
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

REPO_ROOT = Path(__file__).resolve().parents[2]
SOURCE_PATH = REPO_ROOT / "reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm"
SECTION_MAP_PATH = REPO_ROOT / "content/game-data/bank21_22/generated/section-map.json"
SUMMARY_PATH = REPO_ROOT / "content/game-data/bank21_22/generated/summary.json"

LABEL_PATTERN = re.compile(r"^([A-Z0-9_@]+):")
CONSTANT_PATTERN = re.compile(r"^([A-Z0-9_]+)\s*=\s*([^;]+?)(?:\s*;\s*(.*))?$")


@dataclass(frozen=True)
class Section:
    section_name: str
    source_start_line: int
    source_end_line: int
    source_start_marker: str
    source_end_marker: str
    line_count: int
    primary_entry_labels: list[str]
    labels: list[dict[str, int | str]]
    category: str
    notes: str


def main() -> None:
    lines = SOURCE_PATH.read_text(encoding="latin-1").splitlines()
    sections = extract_sections(lines)
    constants = extract_constants(lines)
    summary = build_summary(lines, sections, constants)

    write_json(
        SECTION_MAP_PATH,
        {
            "sourceFile": SOURCE_PATH.name,
            "sectionCount": len(sections),
            "sections": [section.__dict__ for section in sections],
        },
    )
    write_json(SUMMARY_PATH, summary)


def extract_sections(lines: list[str]) -> list[Section]:
    sections: list[Section] = []
    current_name: str | None = None
    current_start = 0
    current_marker = ""

    for line_number, line in enumerate(lines, start=1):
        stripped = line.strip()
        if stripped.startswith("_F{"):
            current_name = stripped[3:].split(";")[0].strip()
            current_start = line_number
            current_marker = stripped
            continue

        if stripped.startswith("_F}"):
            if current_name is None:
                raise ValueError(f"Found section end at line {line_number} without a start marker.")

            section_lines = lines[current_start - 1 : line_number]
            labels = extract_labels(section_lines, current_start)
            sections.append(
                Section(
                    section_name=current_name,
                    source_start_line=current_start,
                    source_end_line=line_number,
                    source_start_marker=current_marker,
                    source_end_marker=stripped,
                    line_count=line_number - current_start + 1,
                    primary_entry_labels=select_primary_entry_labels(labels),
                    labels=labels,
                    category=classify_section(current_name),
                    notes=describe_section(current_name),
                )
            )
            current_name = None
            current_start = 0
            current_marker = ""

    if current_name is not None:
        raise ValueError(f"Section {current_name} was not closed.")

    return sections


def extract_labels(section_lines: Iterable[str], source_start_line: int) -> list[dict[str, int | str]]:
    labels: list[dict[str, int | str]] = []
    for offset, line in enumerate(section_lines):
        match = LABEL_PATTERN.match(line.strip())
        if match:
            labels.append({"label": match.group(1), "line": source_start_line + offset})

    return labels


def select_primary_entry_labels(labels: list[dict[str, int | str]]) -> list[str]:
    if not labels:
        return []

    primary = labels[0]["label"]
    return [str(primary)]


def extract_constants(lines: list[str]) -> list[dict[str, str | int]]:
    in_constants = False
    constants: list[dict[str, str | int]] = []
    for line_number, line in enumerate(lines, start=1):
        stripped = line.strip()
        if stripped.startswith("_F{_PLAYER_ON_FIELD_CONSTANTS"):
            in_constants = True
            continue

        if stripped.startswith("_F}_PLAYER_ON_FIELD_CONSTANTS"):
            break

        if not in_constants:
            continue

        match = CONSTANT_PATTERN.match(stripped)
        if match:
            constants.append(
                {
                    "name": match.group(1),
                    "value": match.group(2).strip(),
                    "comment": (match.group(3) or "").strip(),
                    "line": line_number,
                }
            )

    return constants


def build_summary(lines: list[str], sections: list[Section], constants: list[dict[str, str | int]]) -> dict[str, object]:
    category_counts: dict[str, int] = {}
    for section in sections:
        category_counts[section.category] = category_counts.get(section.category, 0) + 1

    command_processing = next(section for section in sections if section.section_name == "_PLAYER_COMMAND_PROCESSING")
    group_dispatch = [
        "MAN_COVERAGE_TIGHT_COMMAND_START",
        "MAN_COVERAGE_LOOSE_COMMAND_START",
        "RANDOM_COMMAND_START",
        "BLOCK_COMMAND_START",
        "CHOP_BLOCK_COMMAND_START",
        "HANDOFF_COMMAND_START",
        "FAKE_HANDOFF_COMMAND_START",
        "PITCH_BALL_COMMAND_START",
        "PRE_SNAP_MOTION_COMMAND_START",
        "COM_PASS_COMMAND_START",
        "SET_TARGET_ORDER_COMMAND",
        "SET_AND_MOVE_KICKOFF_COMMAND_START",
    ]
    single_dispatch = [
        "QB_DROPBACK_COMMAND_START",
        "COM_WAIT_TO_PASS_COMMAND_START",
        "DO_ACTION_IF_COM_COMMAND_START",
        "COM_JUMP_BASED_ON_JUICE_COMMAND_START",
        "IF_COM_JUMP_COMMAND_START",
        "PASS_BLOCK_COMMAND_START",
        "CENTER_HIKE_COMMAND_START",
        "SHOTGUN_HIKE_COMMAND_START",
        "RECEIVE_SNAP_CENTER_COMMAND_START",
        "RECEIVE_SNAP_SHOTGUN_COMMAND_START",
        "MOVE_RELATIVE_COMMAND_START",
        "CHASE_BALL_AGRESSIVE_COMMAND_START",
        "COM_CONTROL_BALL_CARRIER_COMMAND_START",
        "MAN_TAKE_CONTROL_COMMAND_START",
        "KICKOFF_COMMAND_START",
        "PUNT_COMMAND_START",
        "KICK_FG_COMMAND_START",
        "KICK_XP_COMMAND_START",
        "RETURN_KICK_PUNT_COMMAND_START",
        "BRANCH_COMMAND_START",
        "JUMP_COMMAND_START",
    ]

    bridge_labels = [
        "BANK_JUMP_DO_NEXT_PLAYER_COMMAND",
        "BANK_JUMP_DO_MOVEMENT_COLL_LOGIC",
        "BANK_JUMP_WR_JUMP_DIVE_CHECK_PASS",
        "BANK_JUMP_DEF_JUMP_DIVE_CHECK_PASS",
    ]

    return {
        "sourceFile": SOURCE_PATH.name,
        "lineCount": len(lines),
        "sectionCount": len(sections),
        "constantCount": len(constants),
        "categoryCounts": category_counts,
        "commandDispatcher": {
            "sectionName": command_processing.section_name,
            "sourceStartLine": command_processing.source_start_line,
            "groupCommandCount": len(group_dispatch),
            "singleCommandCount": len(single_dispatch),
            "groupDispatchTargets": group_dispatch,
            "singleDispatchTargetsSample": single_dispatch,
        },
        "bridgeJumpExports": bridge_labels,
        "sourceFaithfulRuntimeBoundary": {
            "hostBank": "Bank19_20_on_field_gameplay_loop.asm",
            "contentBank": "Bank5_6_off_def_play_data.asm",
            "runtimeBank": SOURCE_PATH.name,
            "note": "Bank21_22 hosts the per-player command interpreter and semantic handlers while Bank19_20 controls play-phase orchestration and Bank5_6 supplies scripted content.",
        },
    }


def classify_section(section_name: str) -> str:
    if section_name == "_PLAYER_ON_FIELD_CONSTANTS":
        return "constants"
    if section_name in {"_PLAYER_COMMAND_PROCESSING", "_PLAYER_COMMAND_LENGTH_TABLES"}:
        return "dispatch-and-decoding"
    if "COMMAND" in section_name or "BRANCH_AND_JUMP" in section_name:
        return "command-semantics"
    if any(token in section_name for token in ["COLLISION", "TACKLE", "FUMBLE", "PASS", "BLOCK", "BALL_"]):
        return "gameplay-helpers"
    if "SPRITE" in section_name or "TILE" in section_name or "ANIMATION" in section_name:
        return "presentation-data-and-animation"
    if "TABLE" in section_name or "DATA" in section_name or "ARRAY" in section_name:
        return "lookup-tables"
    return "runtime-support"


def describe_section(section_name: str) -> str:
    descriptions = {
        "_PLAYER_COMMAND_PROCESSING": "Decodes the next opcode from a player's script cursor, advances the cursor using the command-length tables, and dispatches into Bank21_22 command handlers.",
        "_PLAYER_COMMAND_LENGTH_TABLES": "Preserves the source byte lengths for group and single command opcodes so the script cursor can advance faithfully.",
        "_MAN_COVERAGE_COMMAND": "Implements man-coverage assignment commands that bind a defender to a target offensive player for a scripted duration.",
        "_PASS_BLOCK_COMMAND": "Represents defensive pass-block behavior and related interaction checks instead of exposing bank-numbered runtime naming.",
        "_KICKOFF_COMMAND": "Handles kickoff meter, launch, and ball-state sequencing within the per-player command runtime.",
        "_KICK_PUNT_RETURN_LOGIC": "Coordinates return-man control handoff and kickoff/punt return sequencing after the ball is fielded.",
        "_BRANCH_AND_JUMP_COMMANDS": "Preserves script control-flow semantics such as one-byte branch and two-byte jump retargeting.",
    }
    return descriptions.get(section_name, "Source-faithful Bank21_22 runtime section kept so later MonoGame command/runtime ownership can stay traceable without bank-numbered production names.")


def write_json(path: Path, payload: dict[str, object]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2) + "\n")


if __name__ == "__main__":
    main()
