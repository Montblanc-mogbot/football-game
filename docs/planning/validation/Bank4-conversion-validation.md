# Bank4 conversion validation

Updated: 2026-05-27

## Scope

This note validates the first full-bank conversion artifacts for `Bank4_def_spec_play_pointers_data.asm`.

## Artifacts covered

- `docs/planning/banks/Bank4-structure-and-representation.md`
- `development-tools/bank4/extract_bank4.py`
- `content/game-data/defense/generated/bank4-defense-play-pointers.json`
- `content/game-data/bank4/generated/summary.json`
- `src/FootballGame/GameData/Defense/Models/*.cs`

## Validation checks

### Defensive execution structure
Checked against:
- `_F{_DEFENSE_PLAY_POINTERS`
- `DEFENSE_PLAY_POINTERS`
- `DEFENSIVE_EXECUTION_1`
- representative later labels through `DEFENSIVE_EXECUTION_255`

Validated:
- 255 defensive execution tables are preserved in canonical source order
- every defensive execution table keeps exactly 11 ordered reaction labels
- the generated summary reports `defensiveExecutionTableCount = 255`

### Special defense-play structure
Checked against:
- `_F{_DEFENSE_SPECIAL_PLAY_POINTERS`
- `DEFENSE_SPECIAL_PLAY_POINTERS`
- `DEFENSIVE_PLAYERS_CRY_PLAY_POINTERS`
- representative later labels through `UNUSED_7_DEFENSE_SPECIAL_PLAY_POINTERS`

Validated:
- 16 special defense-play tables are preserved in canonical source order
- every special table keeps exactly 12 ordered reaction labels
- the generated summary reports `specialDefensePlayTableCount = 16`

## Important non-goals of this pass

This pass does **not** yet provide:
- runtime mapping from the 11 defensive entries onto specific Bank1_2 roster slots
- behavior-side semantics for what each defensive reaction label does at execution time
- playcall/runtime selection logic that chooses Bank4 tables during gameplay

## Outcome

Bank4 now has an automatic extractor for its defensive execution tables and special defense-play tables, plus a typed semantic model layer that keeps the bank’s ordering semantics explicit and aligned with the earlier Bank1_2 and Bank3 data foundations without consulting the older MonoGame repo.
