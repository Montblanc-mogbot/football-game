# Bank19_20 — runtime representation

Updated: 2026-06-02

## Purpose

This note records the runtime-facing class split used to represent the full `Bank19_20_on_field_gameplay_loop.asm` bank in MonoGame-side code.

The rule for this pass is simple:
- every Bank19_20 section must land in the `OnFieldPlayCoordinator`
- or in one Bank19_20 service class
- and any Bank19_20-to-Bank21_22 boundary material must also be mirrored into an explicit holding area for later Bank21_22 work

## Runtime-facing owner map

### Coordinator
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`

Owns:
- game entry
- live play phase routing
- possession-change flow
- play-over / touchdown / safety / touchback / fumble outcome progression
- quarter-over / XP-kickoff state progression

This class covers the host-owned `play-phase-routing` and `play-outcome` section groups.

### Services
- `src/FootballGame/Gameplay/OnField/Services/TaskCoordinationService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PlayAssignmentService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PlayerSkillHydrationService.cs`
- `src/FootballGame/Gameplay/OnField/Services/OnFieldPresentationService.cs`
- `src/FootballGame/Gameplay/OnField/Services/CpuPlayDecisionService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`
- `src/FootballGame/Gameplay/OnField/Services/StatAccountingService.cs`
- `src/FootballGame/Gameplay/OnField/Services/InjuryCutsceneService.cs`

These classes cover the remaining Bank19_20 section groups.

### Bank21_22 holding area
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`

This mirrors the important boundary sections that are still represented by Bank19_20 services but must be carried forward into the later command-runtime conversion:
- `DEFENDER_CHANGE_BEFORE_HIKE`
- `CHECK_SNAP_PUNT`
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `SET_PLAYERS_CLOSE_TO_PASS`

## Coverage manifest

The complete routine-to-owner placement lives in:
- `src/FootballGame/Gameplay/OnField/OnFieldRoutineOwnershipMap.cs`

Supporting runtime-facing types:
- `src/FootballGame/Gameplay/OnField/OnFieldRoutine.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldOwnerKind.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldRoutinePlacement.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldGameState.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldTeam.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldPhase.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldPlayType.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldKickoffStrategy.cs`

## First implemented coordinator slice

`OnFieldPlayCoordinator` now contains a real first logic slice instead of being only a coverage shell.

Implemented host logic currently includes:
- entry through `GAME_PLAY_START_CHECK_FOR_KICK_TEAM`
- kickoff-side routing into `P1_KICKOFF` or `P2_KICKOFF`
- kickoff setup through play assignment, skill hydration, task startup, presentation setup, and CPU kickoff strategy selection
- `P1_PLAY_SELECT_AND_PLAY_LOAD` / `P2_PLAY_SELECT_AND_PLAY_LOAD` as explicit play-start host entrypoints
- transition out of play-selection into regular-play vs special-teams routing
- regular-play pre-snap setup through presentation, defender-change prep, and play-assignment policy
- run-play vs pass-play host entry
- punt vs field-goal / extra-point host entry for special teams
- normal play-over resolution back into play selection
- turnover-on-downs possession-change routing
- safety outcome routing back into kickoff flow
- pass lifecycle staging before and after the throw
- scramble transition when the QB crosses the LOS without throwing
- interception/tip/incomplete host routing
- no-throw / sack outcome handling
- punt progression from snap to return/touchback/side-change outcome
- field-goal and extra-point progression from snap to made/missed/blocked outcome
- interception return staging through dead-ball possession-change handling
- touchdown aftermath routing into extra-point/kickoff reset
- onside recovery/restart flow
- loose-ball / fumble recovery branching into same-team recovery vs turnover restart
- kickoff progression and onside-routing entry wired into the active coordinator phase loop
- normalized possession-change helpers reused across interception, punt, kickoff, safety, and turnover-on-downs outcomes
- kickoff/punt return host flow now stages ball-fielded → return-live → dead-ball resolution instead of collapsing directly to next-play setup
- extra-point aftermath now routes into kickoff setup for made, missed, and blocked outcomes
- blocked field-goal aftermath now enters explicit loose-ball / live-recovery handling rather than stopping at a placeholder note
- onside return scoring now uses a special-teams touchdown classification
- dead-ball transition cleanup is centralized so play-end / turnover / scoring restart paths share one finalization step before the next sequence begins
- kickoff, punt, interception, onside, and fumble return branches now explicitly record the shared Bank19_20 outcome-evaluator family (`CHECK_FOR_TD`, `CHECK_FOR_FUMBLES_TOSS_AND_NORMAL`, `CHECK_FOR_PLAY_OVER`) during host-side return progression
- safety resolution and post-turnover snap-reset routing now explicitly record `P1_SAFETIED` / `P2_SAFETIED`, `CHECK_FOR_FIRST_DOWN_OR_TOD`, and `UPDATE_HASHMARK_FOR_NEXT_SNAP` in the coordinator trace as well

This is still an incremental implementation slice, but the coordinator now owns real Bank19_20 entry, routing, pass-flow, special-teams-flow, kickoff-flow, turnover-flow, scoring-flow, and outcome-handling logic rather than only ownership metadata.

## Why this is the right Bank19_20 shape

Bank19_20 is not mostly content data.
It is mostly host/runtime behavior.

So the full-bank conversion for Bank19_20 should primarily end up as:
- coordinator code
- service code
- one explicit boundary artifact for Bank21_22 carry-forward work

That is what this pass now provides.
