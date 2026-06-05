# Bank21_22 special-teams runtime validation — 2026-06-05

## Scope
- Bounded Bank21_22 command-runtime slice for the special-teams/return family:
  - `SET_AND_MOVE_KICKOFF_COMMAND_START`
  - `KICKOFF_COMMAND_START`
  - `PUNT_COMMAND_START`
  - `KICK_FG_COMMAND_START`
  - `KICK_XP_COMMAND_START`
  - `RETURN_KICK_PUNT_COMMAND_START`
- Preserve the current Bank19_20 host ownership split while letting the existing live seam route into production-facing Bank21_22 runtime state/handlers.

## Source anchors
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1750-1835` — kickoff pre-placement and move-relative-to-final-ball setup (`SET_AND_MOVE_KICKOFF_COMMAND_START`)
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:3421-3537` — kickoff meter, CPU/manual timing, and kick release (`KICKOFF_COMMAND_START`)
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:3540-3713` — punt snap wait, power bar, CPU/manual release, and punt-distance computation (`PUNT_COMMAND_START`)
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:3716-4221` — shared FG/XP kick path, CPU arrow accuracy boost, avoid-block bug, and cutscene/outcome branching (`KICK_FG_COMMAND_START`, `KICK_XP_COMMAND_START`)
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:4222-4269` — returner icon/manual-control handoff, run-to-catch spot, turn-to-ball, and catch completion (`RETURN_KICK_PUNT_COMMAND_START`)

## Production-facing runtime additions
- Added a dedicated special-teams runtime family under `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/`:
  - `ISpecialTeamsCommandHandler.cs`
  - `SpecialTeamsCommandDispatcher.cs`
  - `SpecialTeamsCommandState.cs`
  - `SetAndMoveKickoffCommandHandler.cs`
  - `KickoffCommandHandler.cs`
  - `PuntCommandHandler.cs`
  - `FieldGoalKickCommandHandler.cs`
  - `ExtraPointKickCommandHandler.cs`
  - `ReturnKickPuntCommandHandler.cs`
- Extended the shared runtime result/state carriers so Bank21_22 steps can retain special-teams continuation state without flattening them into generic host flags:
  - `PlayerCommandHandlerResult.cs`
  - `PlayerCommandStepResult.cs`
  - `PlayerCommandExecutionContext.cs`
  - `PlayerCommandRuntime.cs`

## Host/runtime seam wiring
- Extended `PlayerCommandRuntimeHostRequest` with optional live-command overrides so the Bank19_20 host can keep owning the trigger routine while selecting a bounded Bank21_22 special-teams command sample.
- Updated `CommandRuntimeBoundaryHoldingArea.CreateHostRequests(...)` to expose special-teams seam entries alongside the existing bridge routines:
  - `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` can now explicitly hand the live seam into `SetAndMoveKickoffCommand` or `ReturnKickPuntCommand` when kickoff/return script installation is the relevant Bank19_20 action.
  - `CHECK_SNAP_PUNT` can now explicitly hand the live seam into `PuntCommand`, `FieldGoalKickCommand`, or `ExtraPointKickCommand` after the host snap gate remains satisfied.
- Updated `PlayAssignmentService` and `PreSnapControlService` request filtering so only the relevant special-teams live request is queued for the current host flow; the existing Bank19_20 owners still decide when to queue and prime the boundary.
- Updated `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` and the player-slot selection logic so host requests with live overrides emit the new production-facing Bank21_22 special-teams commands through the existing stepper.

## Bank split preserved
- `OnFieldPlayCoordinator`, `PlayAssignmentService`, and `PreSnapControlService` still own kickoff/punt/FG/XP flow entry, snap gating, script-family installation, and possession/phase transitions.
- The new runtime slice only models the bounded per-player command semantics already visible in Bank21_22: kick meters/arrows, CPU/manual timing windows, relative-to-ball coverage movement, returner catch setup, and the explicit FG avoid-block bug policy.
- No Bank19_20 special-teams routing or result adjudication was collapsed into the Bank21_22 runtime layer.

## Validation
- Bounded compile gate: created a temporary subset project at `/tmp/Bank21_22Subset.csproj` covering `src/FootballGame/Gameplay/OnField/**/*.cs`, then ran `dotnet build /tmp/Bank21_22Subset.csproj`.
- Result: **passed** on 2026-06-05 (`0 Warning(s)`, `0 Error(s)`).
- Note: the first attempt used repo-relative compile includes from `/tmp` and failed with missing-source `CS2001` paths; reran with absolute compile includes and the bounded gate succeeded cleanly.

## Notes / explicit parity policies
- FG/XP handlers keep the source-visible avoid-kick-block index bug as explicit runtime policy (`PreservesAvoidBlockBugByPolicy = true`) rather than silently fixing it.
- The kickoff pre-placement and move-relative-to-final-ball setup remain separate modes inside `SetAndMoveKickoffCommandState`, matching the assembly split at the top of `SET_AND_MOVE_KICKOFF_COMMAND_START`.
