#!/usr/bin/env python3
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent.parent
ASM_PATH = ROOT / "reference" / "Tecmo_Super_Bowl_NES_Disassembly" / "Bank3_formation_metatile_data.asm"
FORMATIONS_OUT_DIR = ROOT / "content" / "game-data" / "formations" / "generated"
BACKGROUNDS_OUT_DIR = ROOT / "content" / "game-data" / "backgrounds" / "generated"
SUMMARY_OUT_DIR = ROOT / "content" / "game-data" / "bank3" / "generated"

LABEL_RE = re.compile(r"^([A-Z0-9_]+):")
HEX_BYTE_RE = re.compile(r"\$([0-9A-F]{1,2})")


def read_lines() -> list[str]:
    return ASM_PATH.read_text().splitlines()


def find_section(lines: list[str], start_marker: str, end_marker: str) -> list[tuple[int, str]]:
    start = next(i for i, line in enumerate(lines) if start_marker in line)
    end = next(i for i, line in enumerate(lines[start + 1 :], start + 1) if end_marker in line)
    return list(enumerate(lines[start:end], start + 1))


def find_section_after(lines: list[str], after_marker: str, end_marker: str) -> list[tuple[int, str]]:
    start = next(i for i, line in enumerate(lines) if after_marker in line) + 1
    end = next(i for i, line in enumerate(lines[start:], start) if end_marker in line)
    return list(enumerate(lines[start:end], start + 1))


def parse_word_tables(section: list[tuple[int, str]]) -> list[dict[str, object]]:
    tables: list[dict[str, object]] = []
    current: dict[str, object] | None = None

    for line_no, line in section:
        stripped = line.strip()
        label_match = LABEL_RE.match(stripped)
        if label_match:
            label = label_match.group(1)
            current = {
                "sourceLabel": label,
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


def parse_metatile_pointer_table(section: list[tuple[int, str]]) -> list[dict[str, object]]:
    pointers: list[dict[str, object]] = []
    for line_no, line in section:
        stripped = line.strip()
        if ".WORD" not in stripped:
            continue
        payload = stripped.split(".WORD", 1)[1].split(";", 1)[0].strip()
        pointers.append(
            {
                "pointerIndex": len(pointers),
                "line": line_no,
                "targetLabel": payload,
            }
        )
    return pointers


def parse_db_bytes(stripped: str) -> list[int]:
    return [int(match.group(1), 16) for match in HEX_BYTE_RE.finditer(stripped.split(";", 1)[0])]


def parse_hex_bytes(stripped: str) -> list[int]:
    payload = stripped.split(".HEX", 1)[1].split(";", 1)[0]
    payload = "".join(character for character in payload if character in "0123456789ABCDEFabcdef")
    if len(payload) % 2 != 0:
        raise ValueError(f"odd hex payload: {payload}")
    return [int(payload[index:index + 2], 16) for index in range(0, len(payload), 2)]


def finalize_metatile_record(record: dict[str, object]) -> dict[str, object]:
    raw_bytes: list[int] = record.pop("rawBytes")  # type: ignore[assignment]
    if len(raw_bytes) < 7:
        raise ValueError(f"{record['sourceLabel']} has too few bytes: {len(raw_bytes)}")

    header = {
        "chrBankPrimary": raw_bytes[0],
        "chrBankSecondary": raw_bytes[1],
        "tileBankOffset": raw_bytes[2],
        "backgroundPaletteSetIndex": raw_bytes[3],
        "heightInMetatiles": raw_bytes[4],
        "widthInMetatiles": raw_bytes[5],
        "startingScreenLocation": raw_bytes[6],
    }

    body = raw_bytes[7:]
    expected = header["heightInMetatiles"] * header["widthInMetatiles"]
    if len(body) != expected:
        raise ValueError(
            f"{record['sourceLabel']} body mismatch: expected {expected} bytes, got {len(body)}"
        )

    width = header["widthInMetatiles"]
    rows = [body[index:index + width] for index in range(0, len(body), width)]

    record["header"] = header
    record["metatileRows"] = rows
    record["metatileRowCount"] = len(rows)
    record["metatileCellCount"] = len(body)
    return record


def parse_metatile_records(section: list[tuple[int, str]]) -> list[dict[str, object]]:
    records: list[dict[str, object]] = []
    current: dict[str, object] | None = None

    for line_no, line in section:
        stripped = line.strip()
        label_match = LABEL_RE.match(stripped)
        if label_match:
            if current is not None:
                records.append(finalize_metatile_record(current))
            current = {
                "sourceLabel": label_match.group(1),
                "line": line_no,
                "rawBytes": [],
            }
            continue

        if current is None:
            continue

        if stripped.startswith(".DB"):
            current["rawBytes"].extend(parse_db_bytes(stripped))  # type: ignore[index]
        elif stripped.startswith(".HEX"):
            current["rawBytes"].extend(parse_hex_bytes(stripped))  # type: ignore[index]

    if current is not None:
        records.append(finalize_metatile_record(current))

    for order, record in enumerate(records):
        record["order"] = order

    return records


def build_formations(lines: list[str]) -> dict[str, object]:
    formation_tables = parse_word_tables(
        find_section(lines, "_F{_OFFENSIVE_FORMATION_POINTERS", "_F}_OFFENSIVE_FORMATION_POINTERS")
    )
    offensive_execution_tables = parse_word_tables(
        find_section(lines, "_F{_OFFENSIVE_PLAY_POINTERS", "_F}_OFFENSIVE_PLAY_POINTERS")
    )
    special_play_tables = parse_word_tables(
        find_section(lines, "_F{_SPECIAL_OFFENSIVE_PLAY_POINTERS", "_F}_SPECIAL_OFFENSIVE_PLAY_POINTERS")
    )

    return {
        "sourceFile": str(ASM_PATH.relative_to(ROOT)),
        "formationTableCount": len(formation_tables),
        "formationEntryCounts": sorted({table["entryCount"] for table in formation_tables}),
        "offensiveExecutionTableCount": len(offensive_execution_tables),
        "offensiveExecutionEntryCounts": sorted({table["entryCount"] for table in offensive_execution_tables}),
        "specialOffensivePlayTableCount": len(special_play_tables),
        "specialOffensivePlayEntryCounts": sorted({table["entryCount"] for table in special_play_tables}),
        "formationTables": formation_tables,
        "offensiveExecutionTables": offensive_execution_tables,
        "specialOffensivePlayTables": special_play_tables,
    }


def build_metatiles(lines: list[str]) -> dict[str, object]:
    pointer_table = parse_metatile_pointer_table(
        find_section(lines, "_F{_METATILE_DATA_POINTERS", "_F}_METATILE_DATA_POINTERS")
    )
    records = parse_metatile_records(
        find_section_after(lines, "_F}_METATILE_DATA_POINTERS", "_F}_METATILE_DATA")
    )

    record_labels = {record["sourceLabel"] for record in records}
    missing_labels = [entry["targetLabel"] for entry in pointer_table if entry["targetLabel"] not in record_labels]
    if missing_labels:
        raise ValueError(f"metatile pointer targets missing records: {missing_labels}")

    unique_dimensions = sorted(
        {
            f"{record['header']['heightInMetatiles']}x{record['header']['widthInMetatiles']}"
            for record in records
        }
    )

    return {
        "sourceFile": str(ASM_PATH.relative_to(ROOT)),
        "pointerCount": len(pointer_table),
        "recordCount": len(records),
        "uniqueDimensions": unique_dimensions,
        "pointerTable": pointer_table,
        "records": records,
    }


def write_json(path: Path, payload: dict[str, object]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2) + "\n")


def main() -> None:
    lines = read_lines()
    formations = build_formations(lines)
    metatiles = build_metatiles(lines)

    write_json(FORMATIONS_OUT_DIR / "bank3-formations.json", formations)
    write_json(BACKGROUNDS_OUT_DIR / "bank3-metatile-layouts.json", metatiles)
    write_json(
        SUMMARY_OUT_DIR / "summary.json",
        {
            "sourceFile": str(ASM_PATH.relative_to(ROOT)),
            "formationTableCount": formations["formationTableCount"],
            "formationEntryCounts": formations["formationEntryCounts"],
            "offensiveExecutionTableCount": formations["offensiveExecutionTableCount"],
            "offensiveExecutionEntryCounts": formations["offensiveExecutionEntryCounts"],
            "specialOffensivePlayTableCount": formations["specialOffensivePlayTableCount"],
            "specialOffensivePlayEntryCounts": formations["specialOffensivePlayEntryCounts"],
            "metatilePointerCount": metatiles["pointerCount"],
            "metatileRecordCount": metatiles["recordCount"],
            "metatileDimensions": metatiles["uniqueDimensions"],
        },
    )


if __name__ == "__main__":
    main()
