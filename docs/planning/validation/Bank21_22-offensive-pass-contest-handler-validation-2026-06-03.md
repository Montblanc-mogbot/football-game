# Bank21_22 offensive pass-contest handler validation — 2026-06-03

## Scope
- Task: Find and port the next real Bank21_22 on-field behavior gap after the current packet 21A live-sample coverage
- Chosen bounded slice: `OFFENSE_JUMP_DIVE_CATCH_PASS_START` from packet 21C, represented as a production-facing live pass-contest command that explicitly continues into the existing receiver-miss interception-window handler.

## Why this was the next real gap
- The host/runtime seam for `SET_PLAYERS_CLOSE_TO_PASS` already existed in live code, but it only sampled the post-miss edge case (`CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH`).
- That meant the actual offensive jump/dive contest setup (`OFFENSE_JUMP_DIVE_CATCH_PASS_START`) still had no production-facing runtime command even though the Bank19_20 side was already priming the receiver/defender ranking for it.

## Source references
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:4759-4848`
  - `OFFENSE_JUMP_DIVE_CATCH_PASS_START` primes rushing power, adjusts receiver direction/final location, updates movement/sprite state, and loops on ball-collision plus jump/dive eligibility checks.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:4849-4910`
  - The same source family handles the stationary wait and ball-collision resolution path before normal pass-result calculation.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:4911-4919`
  - `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH` provides the follow-on miss/interception branch that this bounded live slice now reaches through an explicit continuation.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:5554-5588`
  - `PASS_CALCULATION_TARGET_NOT_CLOSE_TO_BALL` remains the later bug-sensitive defender-only window, preserved by explicit policy in the continuation state.

## Production-facing runtime changes
- Added `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/OffensiveJumpDiveCatchPassCommandHandler.cs`
  - Introduces `OffensiveJumpDiveCatchPassCommand` as the bounded live runtime representation of the offensive jump/dive contest setup.
  - Records pass-contest state and emits an explicit same-player continuation into `ReceiverMissedBallInterceptionWindowCommand` so the live seam keeps progressing instead of stopping at the first setup step.
- Updated `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PassContestCommandDispatcher.cs`
  - Registers the new offensive pass-contest handler ahead of the existing receiver-miss handler.
- Updated `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
  - Changes the live `SET_PLAYERS_CLOSE_TO_PASS` sample entry from the post-miss edge case to `OffensiveJumpDiveCatchPassCommand` / `OFFENSE_JUMP_DIVE_CATCH_PASS_START`.
  - Reuses the existing explicit continuation synthesis to step directly into `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH` on the same player slot.

## Bank19_20 / Bank21_22 seam check
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`
  - Still owns the host-side nearby-player ordering and bridge trigger.
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
  - Still records `SET_PLAYERS_CLOSE_TO_PASS` as the explicit host/runtime seam with `JUMP_WR_JUMP_DIVE_CHECK_PASS`.
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
  - Still releases the host request first, then lets the runtime own the offensive jump/dive setup plus its explicit continuation.

## Verification
- `git diff --check` ✅
- Direct code inspection ✅
  - Confirmed the live `SET_PLAYERS_CLOSE_TO_PASS` seam now uses `OffensiveJumpDiveCatchPassCommand` / `OFFENSE_JUMP_DIVE_CATCH_PASS_START`.
  - Confirmed the new handler emits an explicit same-player continuation into `ReceiverMissedBallInterceptionWindowCommand`.
  - Confirmed the later bug-sensitive defender-only interception window remains explicit in runtime notes/state rather than being hidden.

## Notes
- This is still a bounded pass-contest slice, not the full offensive/defensive jump/dive family.
- The main improvement is that the live seam now begins at the real offensive command-family entry instead of only sampling the later miss branch.
