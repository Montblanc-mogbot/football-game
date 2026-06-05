# Bank21_22 presnap/targeting runtime validation

Date: 2026-06-05

## Scope

This validation slice ports the next bounded Bank21_22 presnap/targeting semantics after the recent player-control and movement work:

- `PreSnapMotionCommand` (`PRE_SNAP_MOTION_COMMAND_START`)
- `SetTargetOrderCommand` (`SET_TARGET_ORDER_COMMAND`)

## Source anchors

Reference bank source:

- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1592-1638`
  - stores the mirrored motion target in extra player RAM
  - loops in repeating 9-frame-plus-random follow delays until the ball is snapped
  - mirrors the offensive motion player vertically until the defender is within the Y-distance window
  - explicitly resets facing / standing state on the hold branch before repeating
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1714-1723`
  - writes the current player's position id into `PASS_TARGETS[target_priority]`
  - only updates `CURRENT_PASS_TARGET` when the written priority slot is zero
  - immediately falls through to `DO_NEXT_PLAYER_COMMAND`

## Runtime changes

Files added:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PreSnapTargetingCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IPreSnapTargetingCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PreSnapTargetingCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PreSnapMotionCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/SetTargetOrderCommandHandler.cs`

Files updated:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldGameState.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`

Key semantic preservation:

- The presnap mirror loop remains a Bank21_22 runtime-owned continuation rather than being collapsed into host-only defender state.
- The runtime now preserves:
  - the mirrored motion target slot
  - the repeating follow-delay / vertical-window semantics
  - the standing/facing reset that happens while the defender holds close to the motion player
  - the exact first-target side effect of `SET_TARGET_ORDER_COMMAND` instead of flattening route ordering into a generic host flag
- The Bank19_20 / Bank21_22 split stays explicit:
  - `PreSnapControlService` still owns the snap gate and initial presnap handoff
  - `PassTargetingService` still owns host-side pass-target ordering state/application
  - `OnFieldPlayCoordinator` only applies the smallest host-visible seam effects after the runtime step completes

## Verification

Bounded compile gate executed successfully with a temporary SDK project that compiled:

- `src/FootballGame/Gameplay/OnField/**/*.cs`

Command shape:

```bash
dotnet build /tmp/.../Bank21_22Subset.csproj -nologo
```

Result:

- Build succeeded
- 0 warnings
- 0 errors

## Notes

This still does not finish the full Bank21_22 conversion. A likely nearby next slice is `QB_DROPBACK_COMMAND_START` or the next coherent post-targeting pass-setup command family that follows this route-order work.
