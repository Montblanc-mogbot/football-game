# Bank21_22 absolute-move runtime validation

Date: 2026-06-05

## Scope

This validation slice ports the next adjacent bounded Bank21_22 movement-command pair after `MOVE_RELATIVE_COMMAND_START`:

- `MoveAbsoluteVsSnapLocationCommand` (`MOVE_ABS_VS_SNAP_LOC_COMMAND_START`)
- `MoveAbsoluteVsMiddleCommand` (`MOVE_ABS_VS_MIDDLE_COMMAND_START`)

## Source anchors

Reference bank source:

- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2578-2595`
  - resolves Y against `LOS_Y`
  - resolves X against `LOS_X`
  - conditionally inverts X for player 2
  - jumps into the shared location-staging path
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2606-2608`
  - resolves Y against the middle-of-field anchor
  - reuses the same shared absolute-move setup path

## Runtime changes

Files added:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/MoveAbsoluteVsSnapLocationCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/MoveAbsoluteVsMiddleCommandHandler.cs`

Files updated:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/MovementCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/MovementCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/MoveRelativeCommandHandler.cs`

Key semantic preservation:

- D8 and D9 remain explicit Bank21_22 commands instead of getting collapsed into a generic host move helper.
- The runtime now preserves:
  - anchor ownership (`LineOfScrimmage` vs `FieldMiddle`)
  - player-two X inversion semantics
  - normalized absolute target intent before the shared move-until-arrival loop
  - the same direction/speed/velocity refresh shape used by the surrounding movement family

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

This still does not finish the full Bank21_22 conversion. Strong remaining candidates nearby include `MAN_TAKE_CONTROL_COMMAND_START`, `COM_CONTROL_BALL_CARRIER_COMMAND_START`, `PRE_SNAP_MOTION_COMMAND_START`, and `SET_TARGET_ORDER_COMMAND`.
