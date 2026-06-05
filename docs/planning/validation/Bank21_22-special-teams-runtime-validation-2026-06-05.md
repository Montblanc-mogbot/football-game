# Bank21_22 special-teams / return runtime validation — 2026-06-05

## Scope

Agent slice C implemented the remaining bounded Bank21_22 special-teams-facing runtime families through the existing on-field host/runtime seam:

- `SET_AND_MOVE_KICKOFF_COMMAND_START`
- `KICKOFF_COMMAND_START`
- `PUNT_COMMAND_START`
- `KICK_FG_COMMAND_START`
- `KICK_XP_COMMAND_START`
- `RETURN_KICK_PUNT_COMMAND_START`

## Source anchors

- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1750-1834` — kickoff setup / move-relative-to-final-ball-location command family (`SET_AND_MOVE_KICKOFF_COMMAND_START`)
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:3421-3538` — kickoff meter / direction / release family (`KICKOFF_COMMAND_START`)
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:3540-3714` — punt snap / receive / meter / punt-release family (`PUNT_COMMAND_START`)
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:3716-3828` — FG / XP shared kick entry (`KICK_FG_COMMAND_START`, `KICK_XP_COMMAND_START`)
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:4222-4288` — kickoff/punt returner retarget / receive family (`RETURN_KICK_PUNT_COMMAND_START`)

## Runtime artifacts added

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/SpecialTeamsCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ISpecialTeamsCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/SpecialTeamsCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/SetAndMoveKickoffCoverageCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/KickoffCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PuntCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/KickFieldGoalCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/KickExtraPointCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ReturnKickOrPuntCommandHandler.cs`

## Host/runtime seam wiring

- `PlayerCommandRuntime` now dispatches the bounded special-teams family through `SpecialTeamsCommandDispatcher` without inventing a parallel integration layer.
- `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now maps the existing `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` seam to kickoff / kickoff-coverage samples and the existing `CHECK_SNAP_PUNT` seam to the punt command family.
- `OnFieldPlayCoordinator.ApplyRuntimeStateToHost(...)` mirrors the special-teams runtime side effects back into explicit host state (`BallKicked`, `BallReceivedByReturnTeam`, `SpecialTeamsKickMeterActive`, `SpecialTeamsKickArrowActive`, `ReturnerAwaitingCatch`) instead of hiding those transitions inside generic flags.

## Validation

- `dotnet build /tmp/Bank21_22Subset.csproj` — passed on 2026-06-05
- `git diff --check` — passed on 2026-06-05

## Notes / bounded decisions

- This slice stayed on the existing seam (`OnFieldPlayCoordinator`, `CommandRuntimeBoundaryHoldingArea`, `PlayAssignmentService`, `PreSnapControlService`, `PlayerCommandRuntime`) and did not broaden into a second special-teams integration path.
- FG/XP remain represented as the shared source entry with distinct production-facing command names so the host can keep `OnFieldPlayType.FieldGoal` vs `OnFieldPlayType.ExtraPoint` explicit while the runtime preserves the common Bank21_22 kick staging.
