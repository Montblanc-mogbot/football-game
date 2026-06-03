# Bank21_22 conversion validation

Updated: 2026-06-03

## Scope

Validated the source-faithful Bank21_22 inventory/representation slice for:
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm`
- `development-tools/bank21_22/extract_bank21_22.py`
- `content/game-data/bank21_22/generated/section-map.json`
- `content/game-data/bank21_22/generated/summary.json`
- `src/FootballGame/Conversion/PlayCommands/*.cs`

## Validation steps

1. Ran:
   - `python development-tools/bank21_22/extract_bank21_22.py`
2. Ran a follow-up Python assertion/comparison pass to confirm:
   - the extractor found all 164 top-level `_F{...}` sections
   - the generated section map still contains 164 sections
   - the generated summary reports 117 top-of-bank constants
   - the command-dispatch summary preserves 12 group-command targets and 21 representative single-command targets
   - the exported bridge jumps remain present in the summary (`BANK_JUMP_DO_NEXT_PLAYER_COMMAND`, `BANK_JUMP_DO_MOVEMENT_COLL_LOGIC`, `BANK_JUMP_WR_JUMP_DIVE_CHECK_PASS`, `BANK_JUMP_DEF_JUMP_DIVE_CHECK_PASS`)
   - the typed loader still references `Sections`, `Constants`, and `CommandDispatcher`
3. Ran:
   - `git diff --check`

## Result

The Bank21_22 conversion slice passed the extractor run, the targeted comparison/assertion pass, and `git diff --check`.

## Notes

This validation intentionally stops at the conversion/runtime-boundary layer.
It does not claim that live gameplay runtime code has been implemented for Bank21_22 yet.
