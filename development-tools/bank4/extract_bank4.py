#!/usr/bin/env python3
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent.parent
ASM_PATH = ROOT / "reference" / "Tecmo_Super_Bowl_NES_Disassembly" / "Bank4_def_spec_play_pointers_data.asm"
OUT_DIR = ROOT / "content" / "game-data" / "defense" / "generated"
SUMMARY_OUT_DIR = ROOT / "content" / "game-data" / "bank4" / "generated"

LABEL_RE = re.compile(r"^([A-Z0-9_]+):")


def read_lines() -> list[str]:
    return ASM_PATH.read_text().splitlines()


def find_section(lines: list[str], start_marker: str, end_marker: str) -> list[tuple[int, str]]:
    start = next(i for i, line in enumerate(lines) if start_marker in line)
    end = next(i for i, line in enumerate(lines[start + 1 :], start + 1) if end_marker in line)
    return list(enumerate(lines[start:end], start + 1))


def parse_word_tables(section: list[tuple[int, str]]) -> list[dict[str, object]]:
    tables: list[dict[str, object]] = []
    current: dict[str, object] | None = None

    for line_no, line in section:
        stripped = line.strip()
        label_match = LABEL_RE.match(stripped)
        if label_match:
            current = {
                "sourceLabel": label_match.group(1),
                "line": line_no,
                "entries": [],
            }
            tables.append(current)
            continue

        if current is None or ".WORD" not in stripped:
            continue

        payload = stripped.split(".WORD", 1)[1].split(";", 1)[0]
        entries = [part.strip() for part in payload.split(",") if part.strip()]
        current["entries"].extend(entries)

    tables = [table for table in tables if table["entries"]]

    for order, table in enumerate(tables):
        table["order"] = order
        table["entryCount"] = len(table["entries"])

    return tables


def build_defense(lines: list[str]) -> dict[str, object]:
    defensive_execution_tables = parse_word_tables(
        find_section(lines, "_F{_DEFENSE_PLAY_POINTERS", "_F}_DEFENSE_PLAY_POINTERS")
    )
    special_defense_tables = parse_word_tables(
        find_section(lines, "_F{_DEFENSE_SPECIAL_PLAY_POINTERS", "_F}_DEFENSE_SPECIAL_PLAY_POINTERS")
    )

    return {
        "sourceFile": str(ASM_PATH.relative_to(ROOT)),
        "defensiveExecutionTableCount": len(defensive_execution_tables),
        "defensiveExecutionEntryCounts": sorted({table["entryCount"] for table in defensive_execution_tables}),
        "specialDefensePlayTableCount": len(special_defense_tables),
        "specialDefensePlayEntryCounts": sorted({table["entryCount"] for table in special_defense_tables}),
        "defensiveExecutionTables": defensive_execution_tables,
        "specialDefensePlayTables": special_defense_tables,
    }


def write_json(path: Path, payload: dict[str, object]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2) + "\n")


def main() -> None:
    lines = read_lines()
    defense = build_defense(lines)

    write_json(OUT_DIR / "bank4-defense-play-pointers.json", defense)
    write_json(
        SUMMARY_OUT_DIR / "summary.json",
        {
            "sourceFile": str(ASM_PATH.relative_to(ROOT)),
            "defensiveExecutionTableCount": defense["defensiveExecutionTableCount"],
            "defensiveExecutionEntryCounts": defense["defensiveExecutionEntryCounts"],
            "specialDefensePlayTableCount": defense["specialDefensePlayTableCount"],
            "specialDefensePlayEntryCounts": defense["specialDefensePlayEntryCounts"],
        },
    )


if __name__ == "__main__":
    main()
