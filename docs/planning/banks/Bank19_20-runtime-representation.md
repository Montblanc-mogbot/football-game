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
- `src/FootballGame/Gameplay/OnField/Bank21Bridge/Bank19ToBank21BoundaryHoldingArea.cs`

This mirrors the important boundary sections that are still represented by Bank19_20 services but must be carried forward into the later command-runtime conversion:
- `DEFENDER_CHANGE_BEFORE_HIKE`
- `CHECK_SNAP_PUNT`
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `SET_PLAYERS_CLOSE_TO_PASS`

## Coverage manifest

The complete section-to-owner placement lives in:
- `src/FootballGame/Gameplay/OnField/Bank19RuntimeRepresentation.cs`

Supporting runtime-facing types:
- `src/FootballGame/Gameplay/OnField/Bank19SectionName.cs`
- `src/FootballGame/Gameplay/OnField/Bank19RuntimeOwnerKind.cs`
- `src/FootballGame/Gameplay/OnField/Bank19RuntimeSectionPlacement.cs`

## Why this is the right Bank19_20 shape

Bank19_20 is not mostly content data.
It is mostly host/runtime behavior.

So the full-bank conversion for Bank19_20 should primarily end up as:
- coordinator code
- service code
- one explicit boundary artifact for Bank21_22 carry-forward work

That is what this pass now provides.
