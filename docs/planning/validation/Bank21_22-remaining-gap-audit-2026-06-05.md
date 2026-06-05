# Bank21_22 remaining-gap audit — 2026-06-05

## Scope

Audited the remaining Bank21_22 dispatcher targets against the current production-facing runtime bridge in `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/`, plus the existing host seam classes documented in `docs/planning/banks/Bank21_22-runtime-representation.md`.

Source inputs reviewed for this pass:
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm`
- `content/game-data/bank21_22/generated/summary.json`
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/*.cs`
- `src/FootballGame/Gameplay/OnField/Services/PlayAssignmentService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`

## Dispatcher coverage snapshot

### Group/single dispatcher targets already represented

Implemented source-visible targets now covered by the runtime bridge:
- `MAN_COVERAGE_TIGHT_COMMAND_START`
- `MAN_COVERAGE_LOOSE_COMMAND_START`
- `HANDOFF_COMMAND_START`
- `FAKE_HANDOFF_COMMAND_START`
- `PITCH_BALL_COMMAND_START`
- `RECEIVE_SNAP_CENTER_COMMAND_START`
- `RECEIVE_SNAP_SHOTGUN_COMMAND_START`
- `RECEIVE_SNAP_FG_XP_COMMAND_START`
- `MOVE_RELATIVE_COMMAND_START`
- `MOVE_ABS_VS_SNAP_LOC_COMMAND_START`
- `MOVE_ABS_VS_MIDDLE_COMMAND_START`
- `CHASE_BALL_AGRESSIVE_COMMAND_START`
- `CHASE_BALL_CARRIER_CONSERVATIVE_COMMAND_START`
- `MIRROR_BALL_CARRIER_WHILE_BEHIND_LOS_COMMAND_START`
- `COM_CONTROL_BALL_CARRIER_COMMAND_START`
- `MAN_TAKE_CONTROL_COMMAND_START`
- `DO_ACTION_IF_COM_COMMAND_START`
- `COM_JUMP_BASED_ON_JUICE_COMMAND_START`
- `IF_COM_JUMP_COMMAND_START`
- `BRANCH_COMMAND_START`
- `JUMP_COMMAND_START`
- `OFFENSE_JUMP_DIVE_CATCH_PASS_START`
- `DEFENSE_JUMP_DIVE_CATCH_PASS_START`
- `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH`
- runtime continuations for `RB_RECEIVES_HANDOFF_START`, `RB_FAKE_HANDOFF_ANIMATION`, and `WAIT_FOR_PLAYER_RECEIVES_PITCH`

### Remaining source-visible dispatcher gaps

Still unimplemented in production-facing runtime code:
- `RANDOM_COMMAND_START`
- `BLOCK_COMMAND_START`
- `CHOP_BLOCK_COMMAND_START`
- `PRE_SNAP_MOTION_COMMAND_START`
- `COM_PASS_COMMAND_START`
- `SET_TARGET_ORDER_COMMAND`
- `SET_AND_MOVE_KICKOFF_COMMAND_START`
- `QB_DROPBACK_COMMAND_START`
- `COM_WAIT_TO_PASS_COMMAND_START`
- `PASS_BLOCK_COMMAND_START`
- `CENTER_HIKE_COMMAND_START`
- `SHOTGUN_HIKE_COMMAND_START`
- `KICKOFF_COMMAND_START`
- `PUNT_COMMAND_START`
- `KICK_FG_COMMAND_START`
- `KICK_XP_COMMAND_START`
- `RETURN_KICK_PUNT_COMMAND_START`

## Host seam audit

The current Bank19_20 ⇄ Bank21_22 split is still structurally correct.

Existing seam owners already provide the right hook points for the remaining work:
- `PlayAssignmentService` still owns bulk script-family install/reassignment through `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`; this is the correct seam for future `RANDOM`, `SET_TARGET_ORDER`, `COM_PASS`, kickoff-coverage, and return-family follow-ups.
- `PreSnapControlService` still owns the snapped-bit gate for `DEFENDER_CHANGE_BEFORE_HIKE` and `CHECK_SNAP_PUNT`; this is the correct seam for snapper-side `CENTER_HIKE_COMMAND_START` / `SHOTGUN_HIKE_COMMAND_START` behavior.
- `PassTargetingService` still owns `SET_PLAYERS_CLOSE_TO_PASS`; no extra host seam is needed for the currently missing pass-adjacent commands.
- `OnFieldPlayCoordinator` already has the explicit live-step synthesis point (`CreateLiveStepDefinition(...)`) and explicit retarget continuation synthesis point (`CreateRetargetContinuationDefinition(...)`) needed to keep adding bounded runtime slices without collapsing back into Bank19_20 host state.
- `PlayerCommandRuntime` plus the dispatcher/handler pattern remains the correct landing zone for every remaining family; no new coordinator-scale abstraction is required first.

## Safe bounded closure completed in this pass

I closed the smallest clearly isolated leftover family in packet 21A: the snapper-side hike initiators.

Added:
- `CenterSnapInitiatorCommandHandler`
- `ShotgunSnapInitiatorCommandHandler`

Wiring updates:
- registered both handlers in `OffensiveExchangeCommandDispatcher`
- allowed them through `PlayerCommandRuntime.IsOffensiveExchangeContinuationCommand(...)`
- changed the live `CHECK_SNAP_PUNT` sample in `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` from the receiver-side `RECEIVE_SNAP_SHOTGUN_COMMAND_START` sample to the earlier snapper-side `SHOTGUN_HIKE_COMMAND_START`

Why this was safe:
- both commands are pure wait/release staging commands documented in packet 21A, with no new host ownership required
- they reuse the existing `OffensiveExchangeCommandState` seam cleanly
- they preserve the current Bank19_20 ownership of the snapped-bit decision and avoid broadening into the much larger kicking/return families

## Remaining blockers / next slices

Best next bounded slices after this audit:
1. `CENTER_HIKE_COMMAND_START` live sample entry from the existing pre-snap seam (`DEFENDER_CHANGE_BEFORE_HIKE`) so the runtime also exercises the under-center snapper-side wait gate.
2. `SET_TARGET_ORDER_COMMAND` + `COM_PASS_COMMAND_START` as a paired offensive passing setup slice, because `COM_PASS` depends on route-order state already primed through Bank19_20 assignment work.
3. `QB_DROPBACK_COMMAND_START` + `COM_WAIT_TO_PASS_COMMAND_START` as the next resumable passing-timing family.
4. `BLOCK_COMMAND_START` + `PASS_BLOCK_COMMAND_START` as a bounded blocking family once the runtime needs explicit target-block pursuit.
5. Leave `KICKOFF_COMMAND_START`, `PUNT_COMMAND_START`, `KICK_FG_COMMAND_START`, `KICK_XP_COMMAND_START`, and `RETURN_KICK_PUNT_COMMAND_START` for a dedicated special-teams runtime packet; they are materially larger because they own kick meters, ball-flight staging, and return possession transfer.

## Validation

Bounded compile gate after the code change:
- `dotnet build /tmp/tmp.ejJyzYPaVj/Bank21_22Subset.csproj -nologo`

Result: pass.
