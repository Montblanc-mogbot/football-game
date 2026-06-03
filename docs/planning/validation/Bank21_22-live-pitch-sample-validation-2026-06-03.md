# Bank21_22 live pitch sample validation — 2026-06-03

## Scope
- Task: Port the next live packet 21A alternate-exchange slice
- Chosen bounded variant: `PITCH_BALL_COMMAND_START` (`7x`) from `Bank21_22_play_commands_on_field_logic.asm`
- Runtime boundary preserved: Bank19_20 host still owns the `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` seam release; Bank21_22 runtime now uses that same live sample entry to exercise the explicit in-flight pitch path and target-runner continuation.

## Source references
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1575-1579`
  - `PITCH_BALL_COMMAND_START` stores the pitch target and jumps into shared `PITCH_COMMAND_LOGIC`.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:7850-7930`
  - `PITCH_COMMAND_LOGIC` stops the quarterback, sets QB ball-carrier state, turns toward the target, creates the ball-leaving-hand state, computes the final moving-ball path, clears QB ball-carrier ownership, and conditionally retargets the target runner into `WAIT_FOR_PLAYER_RECEIVES_PITCH`.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:7931-7944`
  - `WAIT_FOR_PLAYER_RECEIVES_PITCH` repeatedly reasserts target-runner ball-carrier/manual-control ownership until ball collision resolves the catch.

## Production-facing runtime wiring
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PitchExchangeCommandHandler.cs`
  - Already models the quarterback-side pitch launch, in-flight ball state, and explicit retarget into `ReceivePitchContinuationCommand`.
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ReceivePitchContinuationCommandHandler.cs`
  - Already models the target-runner wait-for-collision continuation.
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
  - Changes the live `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` sample entry from `BackfieldHandoffCommand` to `PitchBallCommand`.
  - Keeps the existing explicit retarget-continuation synthesis so the live seam now records both the quarterback-side pitch step and the target-runner receive-pitch continuation.

## Bank19_20 / Bank21_22 seam check
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
  - Still exposes `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` as the same host-owned bridge routine; no pitch selection logic was moved into the host holder.
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs:1167-1193`
  - Still releases the live runtime step from the host queue and then, only on the runtime side, follows any emitted `PlayerCommandRetargetRequest` into the explicit target-player continuation.

## Verification
- `git diff --check` ✅
- Direct code inspection ✅
  - Confirmed the live `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` seam now references `PitchBallCommand` / `PITCH_BALL_COMMAND_START`.
  - Confirmed the explicit target-player continuation path remains `ReceivePitchContinuationCommand` via `PlayerCommandRetargetRequest`.
  - Confirmed the host/runtime split is still explicit: the host releases the sample seam, while the runtime owns the pitch animation / retarget / receive semantics.

## Notes
- This slice does not remove the existing handoff/fake-handoff runtime handlers; it only changes which bounded packet-21A variant is currently used as the live sample entry so more of the converted runtime surface is exercised directly.
