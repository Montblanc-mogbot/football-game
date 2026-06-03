# Bank21_22 backfield-transfer handler validation — 2026-06-03

## Scope

This validation covers the remaining live packet-21A slice after the snap-receive handlers.

Chosen family:
- packet `21A`
- source bank: `Bank21_22_play_commands_on_field_logic.asm`
- bounded production-facing family: immediate post-snap handoff / fake-handoff / pitch exchange commands plus one explicit target-player continuation

## Files inspected/updated

### Runtime bridge / handlers
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/OffensiveExchangeCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/OffensiveExchangeCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/HandoffExchangeCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PitchExchangeCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/RunnerReceiveHandoffCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ReceivePitchContinuationCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRetargetRequest.cs`

### Host/runtime seam
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntimeBoundary.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`

## Source references used

### Bank21_22 handoff / fake-handoff family
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm`
- `HANDOFF_COMMAND_START`
- `FAKE_HANDOFF_COMMAND_START`
- shared `HANDOFF_COMMAND_LOGIC`
- `RB_RECEIVES_HANDOFF_START`
- `RB_FAKE_HANDOFF_ANIMATION`

### Bank21_22 pitch family
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm`
- `PITCH_BALL_COMMAND_START`
- shared `PITCH_COMMAND_LOGIC`
- `WAIT_FOR_PLAYER_RECEIVES_PITCH`

### Bank19_20 host-side split preserved
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `CommandRuntimeBoundaryHoldingArea.CreateHostRequests(...)`
- `OnFieldPlayCoordinator.TryAdvanceLiveCommandRuntime(...)`

## What changed

### 1. The offensive-exchange runtime gained explicit backfield-transfer handlers

New production-facing handler types:
- `HandoffExchangeCommandHandler`
- `PitchExchangeCommandHandler`
- `RunnerReceiveHandoffCommandHandler`
- `ReceivePitchContinuationCommandHandler`

This extends the packet-21A runtime family from pure snap receipt into the immediate post-snap handoff / fake-handoff / pitch slice.

### 2. Cross-player retargeting now stays explicit in runtime state/dispatch

`OffensiveExchangeCommandState` plus `PlayerCommandRetargetRequest` now record:
- the retargeted player slot
- the retargeted continuation command name / source label
- whether the retarget was skipped because the target runner was invalid
- whether the exchange was fake vs live
- whether the quarterback explicitly released ball-carrier state
- whether a pitch created an in-flight ball state

That keeps the cross-player retarget visible on the Bank21_22 side rather than silently folding it into Bank19_20 host state.

### 3. The live seam now enters a bounded handoff command and immediately records one explicit target-player continuation

`OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now uses the `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` live seam to emit `BackfieldHandoffCommand` instead of stopping at a generic receive-only placeholder.

When that command retargets `RB1` into `RunnerReceiveHandoffCommand`, `TryAdvanceLiveCommandRuntime(...)` immediately records a second bounded runtime step for the target runner. That gives the slice one production-facing continuation where possession transfer occurs on the runtime side.

## Wiring check against the documented split

### Still host-owned (Bank19_20 side)
- `CommandRuntimeBoundaryHoldingArea` still owns the same Bank19_20 bridge inventory
- the host still decides when `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` is emitted
- `OnFieldPlayCoordinator` still owns the high-level live-step release and history recording

### Newly runtime-owned (Bank21_22 side)
- the quarterback-side handoff / fake-handoff staging now lives in `HandoffExchangeCommandHandler`
- the pitch-release / in-flight-ball staging now lives in `PitchExchangeCommandHandler`
- the runner-side receive-handoff possession transfer now lives in `RunnerReceiveHandoffCommandHandler`
- pitch receive continuation now lives in `ReceivePitchContinuationCommandHandler`
- retarget metadata now lives in `PlayerCommandRetargetRequest` plus `OffensiveExchangeCommandState`

## Bounded validation evidence

### `git diff --check`
- passed after the packet-21A backfield-transfer slice changes

### Direct code inspection checks
Confirmed by inspection that:
- `OffensiveExchangeCommandDispatcher` now includes dedicated backfield-transfer handlers instead of only snap-receive handlers
- `PlayerCommandRuntime.StepPlayerCommand(...)` accepts explicit offensive-exchange continuation commands without requiring Bank19_20 trigger ownership for every second-player step
- `PlayerCommandRetargetRequest` plus `OffensiveExchangeCommandState` carry explicit retarget metadata (`TargetPlayerSlotKey`, `ContinuationCommandName`, `RetargetedPlayerSlot`, `RetargetedContinuationCommand`, `RetargetSkippedBecauseTargetInvalid`) so cross-player redirection stays visible in runtime state
- `OnFieldPlayCoordinator.TryAdvanceLiveCommandRuntime(...)` now records both the quarterback-side `BackfieldHandoffCommand` step and the explicit `RunnerReceiveHandoffCommand` target-player step through the existing `PlayerCommandRuntimeBoundary`

## Structural validation limit

A full build gate remains structurally unavailable in this repo because there is still no `.csproj` or `.sln` present under `/home/montblanc/repos/football-game`.
