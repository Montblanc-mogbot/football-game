# Bank19_20 ⇄ Bank21_22 live stepping seam validation — 2026-06-03

## Scope

This note validates the first live Bank19_20 ⇄ Bank21_22 stepping seam added for the task:
- `OPENCLAW_TASKS.md` → "Implement the first Bank19_20 ⇄ Bank21_22 live stepping seam"

The goal of this slice is intentionally bounded:
- keep `PlayAssignmentService`, `PreSnapControlService`, `PassTargetingService`, and `CommandRuntimeBoundaryHoldingArea` as explicit host-side owners
- let those host-side owners now prime one production-facing `PlayerCommandRuntimeBoundary`
- allow `OnFieldPlayCoordinator` to advance one live `PlayerCommandRuntime` step after the host marks the snap/handoff boundary ready

## What changed

### Host state now carries explicit command-runtime seam state
- `src/FootballGame/Gameplay/OnField/OnFieldGameState.cs`
  - added `BallSnapped`
  - added optional `CommandRuntimeBoundary`
  - added `PendingCommandRuntimeRequests`
  - added `CommandRuntimeStepHistory`

This keeps the Bank19_20 host/runtime seam visible in state rather than letting services hide it internally.

### `PlayAssignmentService` now primes runtime-boundary requests without owning runtime stepping
- `src/FootballGame/Gameplay/OnField/Services/PlayAssignmentService.cs`

`LOAD_UPDATE_PLAY_CODE_FUNCTIONS`-driven entry/reassignment still belongs to `PlayAssignmentService`.
The new behavior is only:
- create one explicit host request from `CommandRuntimeBoundaryHoldingArea`
- prime a named execution context through `PlayerCommandRuntimeBoundary`
- preserve script-family ownership on the host side

The service still does **not** step commands or decode runtime semantics.

### `PreSnapControlService` now owns the snap gate and only then unlocks stepping
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`

`DEFENDER_CHANGE_BEFORE_HIKE` and `CHECK_SNAP_PUNT` now:
- clear `BallSnapped`
- prime one execution context for the relevant host seam

`MarkBallSnapped(...)` is the explicit host-side release point.
That preserves the packet-21A rule that Bank19_20 decides when the snap occurs and Bank21_22 only resumes after that gate clears.

### `PassTargetingService` still owns pass-collision ordering, but now primes the runtime redirect seam
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`

`SET_PLAYERS_CLOSE_TO_PASS` still records the host-side ranking/query step.
It now also primes a bounded runtime request for the later jump/dive continuation path.

### `OnFieldPlayCoordinator` now advances one bounded live runtime step
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`

The coordinator now:
- injects the optional `PlayerCommandRuntimeBoundary` into host state
- checks `TryAdvanceLiveCommandRuntime(...)` before continuing the rest of active-phase routing
- only advances runtime stepping when `BallSnapped` is true and a pending host request exists
- converts the host request into one bounded `PlayerCommandDefinition`
- records the resulting `PlayerCommandStepResult`

This keeps the live seam explicit:
- host services request / prime
- host snap gate releases
- coordinator advances one runtime boundary step
- runtime still remains intentionally shallow and source-traceable

## Source-boundary alignment preserved

### Host-owned Bank19_20 responsibilities remain explicit
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` → `PlayAssignmentService`
- `DEFENDER_CHANGE_BEFORE_HIKE` → `PreSnapControlService`
- `CHECK_SNAP_PUNT` → `PreSnapControlService`
- `SET_PLAYERS_CLOSE_TO_PASS` → `PassTargetingService`
- boundary inventory / bridge symbols → `CommandRuntimeBoundaryHoldingArea`

### First live runtime step now exists
The current bounded step mapping is:
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` → `UnderCenterSnapReceiveCommand` / `RECEIVE_SNAP_CENTER_COMMAND_START`
- `DEFENDER_CHANGE_BEFORE_HIKE` → `CenterSnapInitiatorCommand` / `CENTER_HIKE_COMMAND_START`
- `CHECK_SNAP_PUNT` → `FieldGoalSnapReceiveCommand` / `RECEIVE_SNAP_FG_XP_COMMAND_START`
- `SET_PLAYERS_CLOSE_TO_PASS` → `PassContestRedirectCommand` / `JUMP_WR_JUMP_DIVE_CHECK_PASS`

These names are still runtime-facing production names, not bank-numbered production types.

## Validation run

### `git diff --check`
- passed

## Structural validation note

A full build/test gate is still structurally unavailable in this repo because there is no `.csproj` or `.sln` present.
So the bounded validation for this slice is:
- direct code inspection of the wiring points above
- `git diff --check`

That is the smallest meaningful verification currently available from the repo state.
