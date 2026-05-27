# Bank3 conversion validation

Updated: 2026-05-27

## Scope

This note validates the first full-bank conversion artifacts for `Bank3_formation_metatile_data.asm`.

## Artifacts covered

- `docs/planning/banks/Bank3-structure-and-representation.md`
- `development-tools/bank3/extract_bank3.py`
- `content/game-data/formations/generated/bank3-formations.json`
- `content/game-data/backgrounds/generated/bank3-metatile-layouts.json`
- `content/game-data/bank3/generated/summary.json`
- `src/FootballGame/GameData/Formations/Models/*.cs`
- `src/FootballGame/GameData/Backgrounds/Models/*.cs`

## Validation checks

### Formation-family structure
Checked against:
- `_F{_OFFENSIVE_FORMATION_POINTERS`
- `KICKOFF_FORMATION_POINTERS`
- representative later labels through `ONEBACK_2_FORMATION_POINTERS`

Validated:
- 22 formation families are preserved in canonical source order
- every formation family keeps exactly 11 ordered reaction labels
- the generated summary reports `formationTableCount = 22`

### Offensive execution structure
Checked against:
- `_F{_OFFENSIVE_PLAY_POINTERS`
- `OFFENSIVE_EXECUTION_1`
- `OFFENSIVE_EXECUTION_92`
- `_F{_SPECIAL_OFFENSIVE_PLAY_POINTERS`

Validated:
- the normal offensive execution layer preserves 92 tables with 11 entries each
- the special offensive-play layer preserves 16 tables with 12 entries each
- the generated summary reports `offensiveExecutionTableCount = 92` and `specialOffensivePlayTableCount = 16`

### Metatile pointer/data structure
Checked against:
- `_F{_METATILE_DATA_POINTERS`
- `METATILE_DATA_POINTERS`
- `DOUBLE_DECKER_STADIUM_WITH_SCOREBOARD_METATILE_DATA`
- `DEFAULT_HELMET_SHELL_METATILE_DATA`
- representative later records through `BENGALS_HELMET_SHELL_P2_MATCHUP_METATILE_DATA`

Validated:
- the metatile pointer table preserves 76 source-order indices
- the extractor keeps the intentional pointer alias where indices `0x3F` and `0x40` both target `DEFAULT_HELMET_SHELL_METATILE_DATA`
- 75 unique layout records are emitted because the shared helmet-shell layout is not duplicated artificially
- each metatile record is validated as `7-byte header + height*width body`
- the generated summary reports `metatilePointerCount = 76` and `metatileRecordCount = 75`

## Important non-goals of this pass

This pass does **not** yet provide:
- runtime formation selection/gameplay integration
- Bank5/Bank21 behavior consumers of the extracted reaction labels
- MonoGame rendering code that consumes the metatile layouts directly

## Outcome

Bank3 now has an automatic extractor for its formation tables, offensive execution tables, special offensive-play tables, and metatile layouts, plus a typed semantic model layer that keeps the bank’s ordering and layout semantics explicit without consulting the older MonoGame repo.
