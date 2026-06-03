# Bank19_20 runtime code review — 2026-06-03

## Scope

This review covers the actual runtime code currently present under:

- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldGameState.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldRoutineOwnershipMap.cs`
- `src/FootballGame/Gameplay/OnField/Services/*.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`

This is intentionally **not** a review of planning notes or conversion-inventory class names.
It is a direct review of the code that currently exists in the runtime layer.

## Naming reality check

The actual runtime host code does **not** use `Bank19`-prefixed class names for the main gameplay coordinator/services.
The currently implemented runtime-facing types use generic production-facing names such as:

- `OnFieldPlayCoordinator`
- `OnFieldGameState`
- `PlayAssignmentService`
- `PreSnapControlService`
- `StatAccountingService`
- `InjuryCutsceneService`
- `PassTargetingService`

The `Bank19*` names are currently concentrated in the conversion/inventory layer, not the main gameplay runtime layer.

## What the runtime layer is mechanically

The present Bank19_20 runtime code is best described as a **source-traceable host-flow state machine**.

Mechanically, the coordinator and services do four main things:

1. inspect flags and enums on `OnFieldGameState`
2. mutate those flags to advance host flow
3. delegate narrow responsibilities to focused service classes
4. record `OnFieldRoutine` history and human-readable event-log entries for traceability

This is real code, not just planning scaffolding, but it is still much closer to a **host orchestration shell** than to a full football simulation runtime.

## `OnFieldPlayCoordinator`: what it actually does

`OnFieldPlayCoordinator` is the main runtime host for the current Bank19_20 slice.
It is a flag-driven dispatcher over `OnFieldGameState`.

### Constructor shape

The coordinator depends on nine services:

- `PlayAssignmentService`
- `PlayerSkillHydrationService`
- `TaskCoordinationService`
- `OnFieldPresentationService`
- `CpuPlayDecisionService`
- `PreSnapControlService`
- `StatAccountingService`
- `InjuryCutsceneService`
- `PassTargetingService`

This means the class is not acting alone; it already has a service split.
However, the coordinator still owns most transition control itself.

### Top-level responsibilities in current code

The coordinator currently owns:

- entry into the on-field gameplay loop
- kickoff setup and kickoff flow
- play-selection exit routing
- regular-play startup
- pass-play loop routing
- punt flow
- field-goal / extra-point flow
- possession-change handling
- play-over resolution
- interception return flow
- onside and loose-ball recovery flow
- touchdown aftermath
- safety resolution
- dead-ball finalization and next-sequence queueing

### Main dispatcher behavior

The mechanical center of the class is `AdvanceActivePlayPhase`.
It routes by checking current state in this order:

1. `PlaySelection` phase
2. active turnover return
3. regular play with manual passing enabled
4. kickoff play type
5. punt play type
6. field-goal / extra-point play type
7. fallback placeholder event

That ordering matters because the coordinator is essentially a manual state machine.
The behavior depends heavily on the correctness of the flag combinations in `OnFieldGameState`.

### Kickoff flow

The kickoff path is not just a planning stub.
It has real branching for:

- kickoff start by team
- CPU kickoff strategy choice
- kickoff script loading
- kickoff presentation setup
- onside path vs normal kickoff path
- touchback handling
- return wait states
- dead-ball return resolution
- touchdown/safety exits
- series handoff into next offensive play

The flow is still mostly boolean-driven, but the routing is materially present.

### Regular play / pass flow

The coordinator distinguishes regular-play startup from special-teams startup.
Regular play then routes into run vs pass opening logic.

The pass path is one of the more explicit pieces of runtime logic currently present:

- enters a pass-play host flow
- enables/disables manual passing
- tracks whether a pass was attempted
- tracks whether the QB crossed the LOS
- tracks whether quarter-pass flight has completed
- waits for pass outcome resolution
- branches into complete, tipped, intercepted, or incomplete handling
- transitions no-throw situations into sack/scramble handling

This is still host-routing logic, not deep pass simulation logic, but it is a real control-flow implementation.

### Special-teams flow

The punt / field-goal / extra-point paths are explicitly implemented rather than left as placeholders.
In current code they include:

- wait states before the kick happens
- wait states for special-teams cutscene/flight staging
- blocked-kick handling
- touchback handling
- live return flow for punts
- made/missed/blocked field-goal routing
- extra-point exit routing back into kickoff setup

### Turnover and recovery flow

The coordinator has explicit flow for:

- interceptions
- loose-ball entry
- same-team fumble recovery
- turnover fumble recovery
- blocked-punt loose-ball aftermath
- blocked-field-goal loose-ball aftermath
- onside recovery

These are not deep physics/gameplay systems yet, but the coordinator does contain real post-event routing and next-state setup.

### Touchdown / safety / next-play routing

The coordinator also owns the scoring and restart shell:

- touchdown presentation and aftermath
- extra-point setup
- kickoff reset after scoring
- safety resolution
- dead-ball finalization
- quarter-over check recording
- next offensive snap vs kickoff re-entry

That means the coordinator is already handling a meaningful portion of the match-host lifecycle.

## `OnFieldGameState`: what it actually is

`OnFieldGameState` is a mutable host-state bag for this runtime slice.
It is intentionally lightweight and traceable.

### What it currently contains

It holds:

- high-level side/phase/play-type state
- kickoff/safety/turnover flags
- pass-specific flags
- kick-specific flags
- special-teams cutscene readiness
- current presentation keys
- pending next-snap yard-line adjustment
- routine history
- event-log history

### What it does not currently look like

It is **not** yet a deep gameplay-world model.
From direct inspection, it does not look like the owner of:

- full player objects
- ball physics
- field geometry/state grids
- actual script runtime objects
- rich animation systems
- robust playbook object graphs

Mechanically, the coordinator uses it as a central control surface for booleans/enums/string keys plus trace logs.

## `OnFieldRoutineOwnershipMap`: what it contributes

`OnFieldRoutineOwnershipMap` is useful and real.
It gives the current runtime a concrete answer to:

- which `OnFieldRoutine` is owned by the coordinator
- which is owned by a supporting service
- which routines are still deferred into the Bank21_22 bridge holding area

This is one of the better runtime artifacts because it makes the current ownership split inspectable.

## Service-by-service mechanical review

## `PlayAssignmentService`

Mechanically, this service owns script-loading and script-reassignment style responsibilities.
In the current runtime, it appears to:

- load kickoff/play-selection setup keys
- apply man-controlled-player policy
- reassign scripts for interception returns
- reassign scripts for punts, onside kicks, loose balls, fumble returns, and touchdown celebration

Current reality: this is a **routing/support service**, not yet a full script-runtime implementation.
It mostly coordinates state changes and traceability.

## `PlayerSkillHydrationService`

Mechanically, this service owns skill-loading entrypoints for:

- full-squad skill hydration
- single-player role hydration
- special-teams skill overrides

Current reality: it reads as a lightweight runtime boundary service.
It exists and is wired into real flows, but it does not appear to own a deep player-model hydration system yet.

## `TaskCoordinationService`

Mechanically, this service is very thin.
It:

- records on-field task start
- records Bank19_20-specific task teardown
- logs those transitions

Current reality: real, but extremely small.
This is more of a task-lifecycle wrapper than a substantive domain service.

## `OnFieldPresentationService`

Mechanically, this service owns the presentation hooks the coordinator needs, including methods that prepare:

- kickoff presentation
- play-selection presentation
- regular-play presentation
- special-teams presentation
- punt-return presentation
- field-goal presentation
- incomplete-pass presentation
- sack presentation
- interception presentation
- loose-ball presentation
- touchdown presentation
- side-change presentation

Current reality: the service is real and central to host flow, but it appears to operate largely by setting presentation keys / recording events rather than by owning a rich rendering/presentation model.

## `CpuPlayDecisionService`

Mechanically, this service currently appears narrow.
It is used for kickoff strategy selection and related support.

Current reality: it is a real dependency, but it does not appear to contain a deep CPU football decision engine in this slice.

## `PreSnapControlService`

Mechanically, this service owns pre-snap host prep such as:

- regular-play pre-snap setup
- punt snap preparation
- related control-state routing

Current reality: useful and correctly separated in concept, but still fairly thin in implementation.

## `StatAccountingService`

This is one of the more concrete services in the current slice.
Mechanically it:

- records stat-update routine coverage
- records play-distance calculation coverage
- resets down-and-distance state after turnovers
- updates next-snap spot/hashmark
- consumes `PendingNextSnapYardLine` when present

Current reality: still light, but more mechanically meaningful than several other services because it performs a specific next-spot adjustment and centralizes turnover-series reset semantics.

## `InjuryCutsceneService`

Mechanically, this service owns:

- pass-start cutscene-state clearing
- cutscene resolution by named outcome key
- recovery cutscene handling
- touchdown cutscene handling
- injury-check routing

Current reality: real and integrated, but largely key/event-driven at present.
It does not yet read like a full injury domain system.

## `PassTargetingService`

Mechanically, this service owns:

- pass-target indicator updates
- pass-collision ordering after partial flight progress

Current reality: it is a valid slice boundary, but from direct review it still looks much more like an orchestration helper than a deep targeting engine.

## `CommandRuntimeBoundaryHoldingArea`

This static type is not planning fluff.
In code, it explicitly marks routines that are still deferred toward the Bank21_22 command-runtime boundary:

- `DEFENDER_CHANGE_BEFORE_HIKE`
- `CHECK_SNAP_PUNT`
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `SET_PLAYERS_CLOSE_TO_PASS`

This is useful because it makes the currently incomplete interpreter/runtime boundary visible in code instead of hiding it.

## Strengths of the current runtime code

From direct code review, the strongest current properties are:

- runtime-facing class names are already generic and production-acceptable
- the coordinator does contain real football-host flow routing, not just notes
- kickoff / punt / FG / XP / interception / fumble / touchdown / safety paths are all materially represented
- dead-ball finalization and next-sequence routing are centralized better than they were in earlier slices
- routine/event logging makes the current runtime highly source-traceable
- the ownership map and bridge holding area make the partial architecture explicit

## Weaknesses / risks of the current runtime code

### 1. `OnFieldPlayCoordinator` is still very large

This class owns too many transitions directly.
Even though services exist, the coordinator still contains the majority of the mechanical branching logic.
It is already trending toward a god-class.

### 2. The system is heavily flag-driven

Correct behavior depends on combinations of booleans such as:

- `BallKicked`
- `BallRecovered`
- `PlayOverTriggered`
- `TurnoverReturnActive`
- `TouchbackTriggered`
- `SafetyTriggered`
- `NextPlayRequiresKickoff`
- `QuarterPassFlightComplete`

That is workable for a traceable conversion shell, but it will become fragile if behavior depth increases without stronger typed sub-state models.

### 3. Several services are still shallow

A number of the current services are structurally helpful but behaviorally light.
They often:

- record routines
- log events
- set a few keys/flags

rather than owning rich domain models or invariant-heavy logic.

### 4. Runtime state is still somewhat stringly typed

Examples visible in the current code include:

- formation/play keys
- banner keys
- song-side keys
- cutscene outcome keys
- single-player role IDs

This is acceptable during parity scaffolding, but it is not ideal long-term runtime design.

### 5. The current layer is still host routing more than full simulation

The code is real, but it still reads more like:

- a transition coordinator
- a traceability layer
- a runtime shell over source-bank responsibilities

than like a fully mature football gameplay engine.

## Bottom line

The actual runtime layer for Bank19_20 is real and useful.
It is **not** just planning references to imaginary bank-numbered classes.

However, the direct code review also shows that the current implementation is still primarily:

- coordinator-driven
- flag-driven
- traceability-oriented
- service-split but still shallow in several domains

So the honest conclusion is:

- the runtime naming is already in decent shape
- the coordinator/services genuinely exist in code now
- the implementation already contains meaningful host-flow mechanics
- but the layer is still closer to a structured runtime shell than to a deep production-complete gameplay runtime

## Recommended next review follow-up

If we want to tighten this slice further, the best next runtime-only review would be to classify each current runtime type as one of:

- solid as-is
- should be split/extracted
- too shallow / placeholder-like
- too stringly typed
- waiting on Bank21_22 runtime follow-through

That would give us a concrete cleanup map for the actual code rather than for planning documents.
