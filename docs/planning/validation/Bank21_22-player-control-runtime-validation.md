# Bank21_22 player-control runtime validation

Date: 2026-06-05

## Scope

This validation slice ports the next bounded Bank21_22 player-control handoff seam after the recent movement/runtime work:

- `CpuControlBallCarrierCommand` (`COM_CONTROL_BALL_CARRIER_COMMAND_START`)
- `ManualTakeControlCommand` (`MAN_TAKE_CONTROL_COMMAND_START`)

## Source anchors

Reference bank source:

- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2778-2854`
  - CPU ball-carrier handoff
  - optional CPU juice/speed boosts
  - ball-carrier/facing assignment
  - long-running CPU movement loop ownership
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:3228-3315`
  - manual-control gate
  - offensive vs defensive manual paths
  - direction/input reset
  - long-running man-control loop ownership

## Runtime changes

Files added:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerControlCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IPlayerControlCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerControlCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CpuControlBallCarrierCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ManualTakeControlCommandHandler.cs`

Files updated:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`

Key semantic preservation:

- These commands stay Bank21_22-shaped instead of being flattened into generic coordinator flags.
- The runtime now preserves:
  - explicit CPU vs manual control ownership
  - ball-carrier assignment intent
  - source-visible facing/speed/velocity setup before the long-running control loop
  - the fact that these commands yield into durable across-frame control loops rather than completing in one step

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

This still does not finish the full Bank21_22 conversion. Strong remaining candidates nearby include `PRE_SNAP_MOTION_COMMAND_START`, `SET_TARGET_ORDER_COMMAND`, and `QB_DROPBACK_COMMAND_START`.
