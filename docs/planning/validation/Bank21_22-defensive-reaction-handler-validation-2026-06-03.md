# Bank21_22 defensive reaction handler validation — 2026-06-03

## Scope

This validation covers the first **post-21A live command-handler family** implementation on top of the existing Bank19_20 ⇄ Bank21_22 live stepping seam.

Chosen family:
- packet `21B`
- source bank: `Bank21_22_play_commands_on_field_logic.asm`
- bounded production-facing family: defensive reaction handlers for:
  - `MAN_COVERAGE_TIGHT_COMMAND_START`
  - `MAN_COVERAGE_LOOSE_COMMAND_START`
  - `MIRROR_BALL_CARRIER_WHILE_BEHIND_LOS_COMMAND_START`
  - `CHASE_BALL_AGRESSIVE_COMMAND_START`
  - `CHASE_BALL_CARRIER_CONSERVATIVE_COMMAND_START`

## Files inspected/updated

### Runtime bridge / handlers
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandDefinition.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/DefensiveReactionCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/DefensiveReactionCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IDefensiveReactionCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ManCoverageAssignmentCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/MirrorBallCarrierBehindLineCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/AggressiveBallCarrierChaseCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ConservativeBallCarrierChaseCommandHandler.cs`

### Host/runtime seam
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntimeBoundary.cs`

## Source references used

### Man coverage
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1462-1476`
- `MAN_COVERAGE_TIGHT_COMMAND_START`
- `MAN_COVERAGE_LOOSE_COMMAND_START`
- `SET_MAN_COVERAGE_DEFEND_TIME`
- `DEFNDER_MAN_TO_MAN_PASS_COVERAGE_START`

### Mirror chase
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2644-2687`
- `MIRROR_BALL_CARRIER_WHILE_BEHIND_LOS_COMMAND_START`

### Aggressive chase
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2618-2635`
- `CHASE_BALL_AGRESSIVE_COMMAND_START`

### Conservative chase
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2714-2772`
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:3368-3383`
- `CHASE_BALL_CARRIER_CONSERVATIVE_COMMAND_START`
- `CHASE_CONSERVATIVE_TURN_TABLE`

## What changed

### 1. The live seam now enters a post-21A defensive family

`OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` no longer uses the `DEFENDER_CHANGE_BEFORE_HIKE` seam to fake another packet-21A snap command.
It now emits:
- `ManCoverageAssignmentCommand` for `DEFENDER_CHANGE_BEFORE_HIKE`
- `ConservativeBallCarrierChaseCommand` for `SET_PLAYERS_CLOSE_TO_PASS`

That keeps the live stepping seam source-traceable while moving into the first bounded family after packet 21A.

### 2. The runtime gained production-facing handler types instead of bank-numbered placeholders

The new execution slice is routed through:
- `DefensiveReactionCommandDispatcher`
- `IDefensiveReactionCommandHandler`
- `ManCoverageAssignmentCommandHandler`
- `MirrorBallCarrierBehindLineCommandHandler`
- `AggressiveBallCarrierChaseCommandHandler`
- `ConservativeBallCarrierChaseCommandHandler`

This matches the documented production naming direction in `docs/planning/banks/Bank21_22-runtime-representation.md`.

### 3. Handler output stays runtime-local and does not absorb Bank19_20 ownership

The handlers only produce runtime-local continuation summaries/state via:
- `PlayerCommandHandlerResult`
- `DefensiveReactionCommandState`

They do **not** take over:
- script installation / reassignment from `PlayAssignmentService`
- pre-snap defender-control handoff from `PreSnapControlService`
- pass-collision candidate ordering from `PassTargetingService`

That preserves the documented Bank19_20/Bank21_22 split.

## Wiring check against the documented split

### Still host-owned (Bank19_20 side)
- `PlayAssignmentService` continues to queue/prime `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `PreSnapControlService` continues to queue/prime `DEFENDER_CHANGE_BEFORE_HIKE` and `CHECK_SNAP_PUNT`
- `PassTargetingService` continues to queue/prime `SET_PLAYERS_CLOSE_TO_PASS`
- `CommandRuntimeBoundaryHoldingArea` still exposes the same four bridge routines and bridge symbols

### Newly runtime-owned (Bank21_22 side)
- `PlayerCommandRuntime` now dispatches production-facing defensive reaction handlers when the trigger routine is:
  - `DEFENDER_CHANGE_BEFORE_HIKE`
  - `SET_PLAYERS_CLOSE_TO_PASS`
- per-step continuation details now live in `PlayerCommandExecutionContext.DefensiveReactionState`

This matches the intended split from `docs/planning/banks/Bank21_22-runtime-representation.md`: host-side services choose/prime the script family, while the `PlayerCommand*` runtime owns per-command continuation semantics.

## Bounded validation evidence

### `git diff --check`
- passed after the handler slice changes

### Direct code inspection checks
Confirmed by inspection that:
- `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now routes one live path into a packet-21B family
- `PlayerCommandRuntime.StepPlayerCommand(...)` dispatches defensive handlers only for the host-side trigger routines that already form the documented boundary
- `CommandRuntimeBoundaryHoldingArea`, `PlayAssignmentService`, `PreSnapControlService`, and `PassTargetingService` remain explicit host-side owners rather than being absorbed into handler code

## Structural validation limit

A full build gate remains structurally unavailable in this repo because there is still no `.csproj` or `.sln` present under `/home/montblanc/repos/football-game`.
