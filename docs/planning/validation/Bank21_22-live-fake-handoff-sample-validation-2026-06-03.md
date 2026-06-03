# Bank21_22 live fake-handoff sample validation — 2026-06-03

## Scope
- Task: Port the next live packet 21A alternate-exchange slice
- Chosen bounded variant: `FAKE_HANDOFF_COMMAND_START` (`6x`) from `Bank21_22_play_commands_on_field_logic.asm`
- Runtime boundary preserved: Bank19_20 host still owns the `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` seam release; Bank21_22 runtime now uses that live sample entry to exercise the fake-handoff retarget path without transferring possession.

## Source references
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1563-1567`
  - `FAKE_HANDOFF_COMMAND_START` sets the fake-handoff bit on the target slot and enters shared `HANDOFF_COMMAND_LOGIC`.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:7748-7810`
  - shared `HANDOFF_COMMAND_LOGIC` stops the quarterback, starts the exchange timing, restores displayed-name status for the fake branch, and retargets the runner through `UPDATE_LOCAL_PLAYER_COMMAND_ADDR_IF_VALID`.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:7839-7848`
  - `RB_FAKE_HANDOFF_ANIMATION` plays the two-phase fake-take runner animation and returns to normal stepping without assigning ball-carrier ownership.

## Production-facing runtime wiring
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/HandoffExchangeCommandHandler.cs`
  - Already models the `BackfieldHandoffCommand` fake branch using `fakeExchange = true` and emits an explicit retarget request for `RunnerFakeHandoffAnimationCommand`.
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/RunnerFakeHandoffAnimationCommandHandler.cs`
  - Already models the runner-side fake-handoff continuation without moving possession.
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
  - Changes the live `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` sample entry from the pitch sample to the fake-handoff branch by emitting `BackfieldHandoffCommand` with source label `FAKE_HANDOFF_COMMAND_START` and `fakeExchange = true`.

## Bank19_20 / Bank21_22 seam check
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
  - Still exposes `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` as the same host-owned bridge routine.
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs:1167-1193`
  - Still releases the live runtime step from the host queue and then follows the runtime-emitted `PlayerCommandRetargetRequest` into the explicit target-player continuation.

## Verification
- `git diff --check` ✅
- Direct code inspection ✅
  - Confirmed the live `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` seam now references `FAKE_HANDOFF_COMMAND_START`.
  - Confirmed the explicit target-player continuation path is now `RunnerFakeHandoffAnimationCommand`.
  - Confirmed the host/runtime split is still explicit: the host releases the seam, while the runtime owns the fake-exchange timing and runner animation semantics.

## Notes
- This slice does not replace the existing handoff or pitch runtime handlers; it only changes which bounded packet-21A variant is currently sampled live so another real Bank21_22 branch is exercised directly.
