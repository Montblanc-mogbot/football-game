# Bank21_22 snap-receive handler validation — 2026-06-03

## Scope

This validation covers the next live Bank21_22 runtime slice after the packet-21C pass-contest work.

Chosen family:
- packet `21A`
- source bank: `Bank21_22_play_commands_on_field_logic.asm`
- bounded production-facing family: the live snap/long-snap receive commands already reached by the Bank19_20 host seam

## Files inspected/updated

### Runtime bridge / handlers
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/OffensiveExchangeCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IOffensiveExchangeCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/OffensiveExchangeCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/UnderCenterSnapReceiveCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/FieldGoalSnapReceiveCommandHandler.cs`

### Host/runtime seam
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`

## Source references used

### Bank19_20 host-side snap ownership
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm`
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `CHECK_SNAP_PUNT`
- `SET_BALL_SNAPPED_START_CLOCK_EXCEPT_XP`

### Bank21_22 under-center receive path
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2436-2447`
- `RECEIVE_SNAP_CENTER_COMMAND_START`

### Bank21_22 FG/XP holder receive path
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2501-2534`
- `RECEIVE_SNAP_FG_XP_COMMAND_START`

## What changed

### 1. The runtime gained a separate offensive-exchange dispatcher/state family

New production-facing runtime types:
- `OffensiveExchangeCommandDispatcher`
- `IOffensiveExchangeCommandHandler`
- `UnderCenterSnapReceiveCommandHandler`
- `FieldGoalSnapReceiveCommandHandler`
- `OffensiveExchangeCommandState`

This keeps the packet-21A snap-receive semantics separate from both the packet-21B defensive reaction state and the packet-21C pass-contest state.

### 2. The live seam now carries real packet-21A receive state instead of only a generic placeholder step

`OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` still keeps Bank19_20 as the owner of the snap gate, but the emitted live commands now carry source-specific packet-21A notes and operands:
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` → `UnderCenterSnapReceiveCommand`
- `CHECK_SNAP_PUNT` → `FieldGoalSnapReceiveCommand`

`PlayerCommandRuntime.StepPlayerCommand(...)` now dispatches those triggers into the new offensive-exchange dispatcher and records the resulting continuation state in `PlayerCommandExecutionContext.OffensiveExchangeState`.

### 3. Snap gating remains explicitly host-owned

The new handlers deliberately record that they waited on a host-owned snap gate instead of trying to move the snap decision into Bank21_22 runtime ownership.

That preserves the intended split:
- Bank19_20 decides when `BallSnapped` flips
- Bank21_22 receive commands react to that snapped state and handle post-snap receive semantics

## Wiring check against the documented split

### Still host-owned (Bank19_20 side)
- `PreSnapControlService` and the coordinator still own the snap timing gate
- `CommandRuntimeBoundaryHoldingArea` still exposes `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` and `CHECK_SNAP_PUNT` as bridge routines
- `OnFieldPlayCoordinator.TryAdvanceLiveCommandRuntime(...)` still refuses to step the runtime until `BallSnapped` is already true

### Newly runtime-owned (Bank21_22 side)
- under-center snap receive continuation state now lives in `OffensiveExchangeCommandState`
- FG/XP holder long-snap receive continuation state now lives in `OffensiveExchangeCommandState`
- `PlayerCommandRuntime` now has explicit packet-21A dispatch instead of only packet-21B/21C families

## Bounded validation evidence

### `git diff --check`
- passed after the packet-21A live snap-receive slice changes

### Direct code inspection checks
Confirmed by inspection that:
- `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` emits packet-21A-specific notes/operands for both `UnderCenterSnapReceiveCommand` and `FieldGoalSnapReceiveCommand`
- `PlayerCommandRuntime.StepPlayerCommand(...)` dispatches `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` and `CHECK_SNAP_PUNT` into the new offensive-exchange dispatcher
- `PlayerCommandExecutionContext` and `PlayerCommandStepResult` now preserve `OffensiveExchangeState`
- the host seam still requires `BallSnapped` before any live runtime step occurs, so the snap gate remains clearly Bank19_20-owned

## Structural validation limit

A full build gate remains structurally unavailable in this repo because there is still no `.csproj` or `.sln` present under `/home/montblanc/repos/football-game`.
