# Bank21_22 backfield continuation wiring validation — 2026-06-03

## Scope

This validation covers a coherence repair/follow-up on the live packet-21A backfield-transfer runtime slice.

Chosen repair scope:
- packet `21A`
- source bank: `Bank21_22_play_commands_on_field_logic.asm`
- bounded production-facing fix: make the current live backfield continuation wiring internally consistent, add the missing fake-handoff target-player continuation, and keep pitch continuation dispatchable through the same explicit retarget path

## Files inspected/updated

### Runtime bridge / handlers
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/OffensiveExchangeCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/RunnerFakeHandoffAnimationCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/HandoffExchangeCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PitchExchangeCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/RunnerReceiveHandoffCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ReceivePitchContinuationCommandHandler.cs`

### Host/runtime seam
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`

## Source references used

### Bank21_22 handoff / fake-handoff family
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1559-1567`
- `HANDOFF_COMMAND_START`
- `FAKE_HANDOFF_COMMAND_START`
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:7748-7848`
- shared `HANDOFF_COMMAND_LOGIC`
- `RB_RECEIVES_HANDOFF_START`
- `RB_FAKE_HANDOFF_ANIMATION`

### Bank21_22 pitch continuation family
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:7850-7944`
- `PITCH_COMMAND_LOGIC`
- `WAIT_FOR_PLAYER_RECEIVES_PITCH`

## What changed

### 1. Continuation command naming is now internally consistent

Before this repair, the live seam mixed:
- `RunnerReceiveHandoffCommand` in the handler layer
- `ReceiveHandoffContinuationCommand` in the coordinator continuation synthesis

That meant the explicit continuation path described by the runtime slice was not actually coherent in the current code.

The coordinator now synthesizes the same continuation command names that the runtime handlers and retarget requests already use:
- `RunnerReceiveHandoffCommand`
- `RunnerFakeHandoffAnimationCommand`
- `ReceivePitchContinuationCommand`

### 2. The coordinator now builds explicit retarget continuations from `PlayerCommandRetargetRequest`

`OnFieldPlayCoordinator` no longer hard-codes only one handoff continuation shape.

Instead, `TryCreateExplicitRetargetContinuation(...)` now:
- reads the emitted `PlayerCommandRetargetRequest`
- builds the appropriate continuation `PlayerCommandDefinition`
- steps the target player explicitly through the existing `PlayerCommandRuntimeBoundary`

That keeps the cross-player Bank21_22 retarget contract visible and reusable across:
- regular handoff
- fake handoff
- pitch receive continuation

### 3. The live `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` sample now actually lands in the backfield-transfer family

`OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now emits:
- `BackfieldHandoffCommand`
- source label `HANDOFF_COMMAND_START`

That makes the live sample consistent with the later packet-21A backfield-transfer work instead of still entering the older under-center receive placeholder from the earlier slice.

### 4. The missing fake-handoff target-player continuation now exists in runtime code

Added:
- `RunnerFakeHandoffAnimationCommandHandler`

This preserves the source distinction that the target runner still receives a retargeted animation continuation even though possession does not transfer.

## Wiring check against the documented split

### Still host-owned (Bank19_20 side)
- `OnFieldPlayCoordinator` still owns release of the live Bank21_22 step from `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- the host still owns snap gating and the higher-level timing of when the live seam is entered

### Newly/cleanly runtime-owned (Bank21_22 side)
- cross-player retarget command names now line up with the actual handlers
- fake-handoff target animation is now a first-class runtime continuation
- pitch receive continuation remains dispatchable through the same explicit retarget path

## Bounded validation evidence

### `git diff --check`
- passed after the continuation-wiring repair

### Direct code inspection checks
Confirmed by inspection that:
- no `ReceiveHandoffContinuationCommand` string remains in the live runtime code path
- `PlayerCommandRuntime.IsOffensiveExchangeContinuationCommand(...)` now recognizes `RunnerReceiveHandoffCommand`, `RunnerFakeHandoffAnimationCommand`, and `ReceivePitchContinuationCommand`
- `OffensiveExchangeCommandDispatcher` now includes `RunnerFakeHandoffAnimationCommandHandler`
- `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now emits `BackfieldHandoffCommand` for the live `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` sample
- `OnFieldPlayCoordinator.TryCreateExplicitRetargetContinuation(...)` now synthesizes explicit target-player continuations from `PlayerCommandRetargetRequest`

## Structural validation limit

A full build gate remains structurally unavailable in this repo because there is still no `.csproj` or `.sln` present under `/home/montblanc/repos/football-game`.
