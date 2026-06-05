# Bank21_22 presnap / targeting runtime validation — 2026-06-05

## Scope
Implemented the next coherent Bank21_22 presnap / targeting family through the existing on-field host/runtime seam:
- `PRE_SNAP_MOTION_COMMAND_START`
- `SET_TARGET_ORDER_COMMAND`
- directly adjacent host-state setup semantics needed to keep the seam explicit (`PreSnapControlService`, `PassTargetingService`, `OnFieldPlayCoordinator`, `OnFieldGameState`)

## Source anchors
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1592-1631`
  - `PRE_SNAP_MOTION_COMMAND_START`
  - stores the mirrored offensive player id, waits 9 frames between checks, exits on snap, and otherwise either stops when aligned within the Y threshold or requeues direction/speed updates toward the motion player.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1714-1725`
  - `SET_TARGET_ORDER_COMMAND`
  - writes the current player into `PASS_TARGETS,X`, and only when `X == 0` also sets `CURRENT_PASS_TARGET` before returning immediately to `DO_NEXT_PLAYER_COMMAND`.

## Production-facing runtime changes
### New bounded runtime command family for pre-snap motion
Added:
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PreSnapCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IPreSnapCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PreSnapMotionCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PreSnapCommandState.cs`

This command family records:
- followed motion target slot
- 9-frame follow delay
- near-target Y threshold
- snap-gated exit behavior
- aligned-stop vs move-toward-target continuation shape

### New bounded runtime command family for target ordering
Added:
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/TargetingCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ITargetingCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/SetTargetOrderCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PassTargetOrderCommandState.cs`

This command family records:
- target-priority index
- receiver slot installed into the pass-target array
- whether that write also becomes the current first-read target

### Runtime integration updates
Updated:
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`

The runtime now carries these new command-state families alongside the existing defensive/pass-contest/offensive-exchange/movement/player-control/control-flow slices instead of inventing a parallel path.

## Host/runtime seam updates
Updated:
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldGameState.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`

Key seam behavior:
- `PreSnapControlService` still owns defender switching and snap gating, but now primes host-visible motion-follow context (`ActiveDefenderSlotKey`, `MotionFollowTargetSlotKey`) before handing one bounded step to the runtime.
- `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now maps:
  - `DEFENDER_CHANGE_BEFORE_HIKE` → `PreSnapMotionCommand` / `PRE_SNAP_MOTION_COMMAND_START`
  - `SET_PLAYERS_CLOSE_TO_PASS` → `SetTargetOrderCommand` / `SET_TARGET_ORDER_COMMAND`
- `OnFieldPlayCoordinator.ApplyRuntimeStateToHost(...)` mirrors the new runtime side effects back into explicit host state:
  - pre-snap motion target tracking
  - `PassTargets` array semantics
  - `CurrentPassTargetPriority`
- `PassTargetingService.UpdatePassTargetIndicator(...)` now reports the current first-read target when runtime target-order state has installed one.

## Why this stays within the required seam
This slice does **not** add a new integration path.
It continues to use:
- `OnFieldPlayCoordinator`
- `CommandRuntimeBoundaryHoldingArea`
- `PlayAssignmentService`
- `PreSnapControlService`
- `PassTargetingService`
- `PlayerCommandRuntime` command-family dispatch

The host still decides **when** pre-snap control and pass-target ordering are primed.
The runtime now owns the source-visible Bank21_22 per-command side effects for the presnap-motion and target-order commands themselves.

## Validation evidence
### Bounded compile gate
Passed:
- `dotnet build /tmp/Bank21_22Subset.csproj`

This compile gate covers the on-field runtime seam slice and succeeded cleanly on 2026-06-05.

### Working-tree hygiene gate
Passed:
- `git diff --check`

## Notes
This closes the requested presnap / targeting slice at the command-family level, but does not yet cover the next QB/pass-control family (`QB_DROPBACK_COMMAND_START`, `COM_WAIT_TO_PASS_COMMAND_START`, `COM_PASS_COMMAND_START`).
