# Bank21_22 move-relative runtime validation

Date: 2026-06-05

## Scope

This validation slice ports the next bounded Bank21_22 movement command seam after the control-flow work: `MOVE_RELATIVE_COMMAND_START`.

Implemented runtime support:

- `MoveRelativeCommand` (`MOVE_RELATIVE_COMMAND_START`)

## Source anchor

Reference bank source:

- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2547-2570`
  - saves relative Y
  - conditionally inverts X for player 2
  - stages the movement target in player RAM
  - queues direction/speed/velocity refresh
  - yields through a move-until-arrival loop before returning to `DO_NEXT_PLAYER_COMMAND`

## Runtime changes

Files added:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/MovementCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IMovementCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/MovementCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/MoveRelativeCommandHandler.cs`

Files updated:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`

Key semantic preservation:

- The handler keeps the command Bank21_22-shaped instead of flattening it into a generic host move:
  - relative target payload remains explicit
  - player-two X inversion remains explicit
  - direction/speed initialization is represented as queued movement work
  - the command remains a multi-frame continuation that waits for arrival

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

This is still a bounded runtime seam, not the end of the full Bank21_22 conversion. Nearby movement commands such as `MOVE_ABS_VS_SNAP_LOC_COMMAND_START` and `MOVE_ABS_VS_MIDDLE_COMMAND_START` remain candidates for the next slice.
