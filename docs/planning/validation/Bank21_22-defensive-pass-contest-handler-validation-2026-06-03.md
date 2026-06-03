# Bank21_22 defensive pass-contest handler validation — 2026-06-03

## Scope
- Task: Port the next Bank21_22 pass-contest runtime gap
- Chosen bounded slice: `DEFENSE_JUMP_DIVE_CATCH_PASS_START` from packet 21C, represented as a production-facing live pass-contest command.

## Why this was the next gap
- After the offensive pass-contest entry slice, the live `SET_PLAYERS_CLOSE_TO_PASS` seam still had no production-facing representation for the defensive jump/dive family.
- The source-visible bridge symbol `JUMP_DEF_JUMP_DIVE_CHECK_PASS` already exists in the boundary docs/holding area, so this was the next concrete missing command path to surface in runtime code.

## Source references
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:5125-5211`
  - `DEFENSE_JUMP_DIVE_CATCH_PASS_START` updates defender movement, loops on ball-collision plus jump/dive timing checks, and stops the defender at the final pass location when appropriate.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:5212-5294`
  - The same family handles defender-near-ball resolution and the end-of-pass-dive routing.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:5295-5459`
  - The same family continues into defender jump, tip, whiff, and landing behavior.

## Production-facing runtime changes
- Added `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/DefensiveJumpDiveCatchPassCommandHandler.cs`
  - Introduces `DefensiveJumpDiveCatchPassCommand` as the bounded live runtime representation of the defensive jump/dive contest family entry.
- Updated `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PassContestCommandDispatcher.cs`
  - Registers the new defensive pass-contest handler alongside the offensive and receiver-miss handlers.
- Updated `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
  - Changes the live `SET_PLAYERS_CLOSE_TO_PASS` sample entry from the offensive jump/dive sample to `DefensiveJumpDiveCatchPassCommand` / `DEFENSE_JUMP_DIVE_CATCH_PASS_START`.

## Bank19_20 / Bank21_22 seam check
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`
  - Still owns the host-side nearby-player ordering and bridge trigger.
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
  - Still keeps the jump/dive bridge symbols explicit, including `JUMP_DEF_JUMP_DIVE_CHECK_PASS`.
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
  - Still releases the host request first, then lets the runtime own the defender jump/dive contest step.

## Verification
- `git diff --check` ✅
- Direct code inspection ✅
  - Confirmed the live `SET_PLAYERS_CLOSE_TO_PASS` seam now uses `DefensiveJumpDiveCatchPassCommand` / `DEFENSE_JUMP_DIVE_CATCH_PASS_START`.
  - Confirmed the new handler is registered in `PassContestCommandDispatcher`.
  - Confirmed the host/runtime split remains explicit: host ranks the pass contestants, while runtime owns the defender-side contest entry.

## Notes
- This remains a bounded slice rather than the full combined offensive/defensive pass-contest family.
- The main improvement is that the defensive jump/dive entry now exists as a real production-facing runtime command instead of only as a documented future gap.
