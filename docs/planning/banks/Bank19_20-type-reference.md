# Bank19_20 type reference

Updated: 2026-06-03

## Purpose

This note documents the conversion and runtime types introduced during the Bank19_20 work.
It exists so the bank's class/type surface is reviewable without reconstructing it from task history or scanning the whole source tree.

## Conversion-layer types

### `Bank19OnFieldGameplayInventory`
Top-level typed container for the extracted Bank19_20 inventory. Holds entrypoints, script-pointer families, external jump constants, external dependencies, and section records.

### `Bank19EntryPointRecord`
Represents one explicit Bank19_20 bank entry label and its immediate target label.

### `Bank19ScriptPointerFamilyRecord`
Represents one named Bank19_20 script-pointer family constant used for mid-play script reassignment during kickoffs, interceptions, fumble recovery, and related transitions.

### `Bank19ExternalJumpConstantRecord`
Represents one named cross-bank jump constant declared in Bank19_20.

### `Bank19CrossBankDependencyRecord`
Represents one explicit dependency from Bank19_20 into another bank or external gameplay subsystem.

### `Bank19SectionRecord`
Represents one extracted `_F{ ... }` Bank19_20 section with source span, ownership, responsibility group, labels, dependency symbols, and Bank21_22 carry-forward metadata.

### `Bank19SectionLabelRecord`
Represents one global label discovered inside a Bank19_20 section.

### `Bank19ModernOwner`
Conversion-layer ownership enum that answers whether a section is best modeled as controller/coordinator logic or supporting-service logic.

### `Bank19ResponsibilityGroup`
Conversion-layer responsibility enum used to group sections into coarse gameplay areas such as play-phase routing, play outcome, pre-snap control, presentation, stats, and injury/cutscene handling.

### `Bank19OnFieldGameplayInventoryJsonLoader`
Deserializes the generated Bank19_20 JSON artifact into the typed conversion model and enforces the supported owner/responsibility classifications.

## Runtime host/state types

### `OnFieldPlayCoordinator`
Primary Bank19_20 runtime host. Owns the high-level on-field loop, kickoff/play-selection entry, regular-play and special-teams routing, possession changes, dead-ball finalization, and sequence-to-sequence transitions.

### `OnFieldGameState`
Mutable host-oriented state carrier for the current Bank19_20 slice. Tracks possession, play family, phase, kick/pass/turnover flags, pending next-snap spot information, routine history, and event log entries.

### `OnFieldRoutine`
Runtime-facing enum of Bank19_20 source section names. This is the main source-traceability vocabulary used by the runtime map and coordinator/service history logging.

### `OnFieldRoutinePlacement`
One runtime placement record for a Bank19_20 routine, including which runtime type currently owns it and why.

### `OnFieldRoutineOwnershipMap`
Static runtime map covering the full Bank19_20 routine set. It answers where each routine currently lives in the modern runtime.

### `OnFieldOwnerKind`
Runtime ownership enum used by `OnFieldRoutineOwnershipMap` to distinguish coordinator-owned versus service-owned responsibilities.

### `OnFieldTeam`
Player-side enum used throughout the on-field runtime to represent Player1 vs Player2 ownership.

### `OnFieldPhase`
High-level on-field phase enum covering kickoff, play selection, pre-snap, live play, and play-over flow.

### `OnFieldPlayType`
Broad play-family enum for regular offense, kickoff, punt, field goal, and extra point flow.

### `OnFieldKickoffStrategy`
CPU decision enum for kickoff strategy selection, currently normal vs onside.

### `OnFieldKickOutcome`
Host-side kick outcome enum used for punts, field goals, and extra points.

### `OnFieldPassOutcome`
Host-side pass outcome enum for in-flight, complete, tipped, intercepted, and incomplete pass states.

### `OnFieldTouchdownKind`
High-level touchdown classification enum used to distinguish offensive run/pass scores from defensive and special-teams return scores.

## Runtime supporting services

### `PlayAssignmentService`
Owns the Bank19_20 script-loading and script-reassignment responsibilities. Handles kickoff/play-selection setup plus turnover, punt, interception, onside, loose-ball, fumble-return, and touchdown reassignment flows.

### `PlayerSkillHydrationService`
Owns Bank19_20 player-skill loading and targeted special-teams/player-role hydration that happens during on-field setup.

### `TaskCoordinationService`
Owns the Bank19_20 task/game-status startup and teardown helpers around the live on-field loop.

### `OnFieldPresentationService`
Owns banner/music/scroll/LOS-marker and related draw/presentation helpers for the on-field host flow.

### `CpuPlayDecisionService`
Owns the narrow CPU kickoff/special-teams decision support that Bank19_20 needs for host routing.

### `PreSnapControlService`
Owns Bank19_20 pre-snap control helpers such as defender-change handling, snap gating, and punt snap preparation.

### `StatAccountingService`
Owns Bank19_20 play-distance, stats, turnover-series reset, and next-spot/hashmark update responsibilities.

### `InjuryCutsceneService`
Owns injury checks, injury replacement, recovery/touchdown cutscene selection, and related Bank19_20 cutscene outcome support.

### `PassTargetingService`
Owns pass-target update and pass-collision ordering helpers that support the Bank19_20 pass lifecycle.

## Bank21_22 bridge type

### `CommandRuntimeBoundaryHoldingArea`
Static holding area for the Bank19_20 responsibilities that still need explicit carry-forward attention when Bank21_22 command-runtime conversion is expanded. This keeps the host/interpreter boundary visible instead of letting it disappear into service code.

## How these types fit together

### Conversion side
`Bank19OnFieldGameplayInventoryJsonLoader` loads generated Bank19_20 artifacts into `Bank19OnFieldGameplayInventory`, which is composed from the various `Bank19*Record` types plus the ownership/responsibility enums.

### Runtime side
`OnFieldRoutineOwnershipMap` assigns each `OnFieldRoutine` to either `OnFieldPlayCoordinator`, one of the focused on-field services, or the Bank21_22 carry-forward holding area.

### State flow
`OnFieldPlayCoordinator` mutates `OnFieldGameState` and delegates narrow responsibility slices to the focused services. Those services record source-traceable routine and event history back into `OnFieldGameState`.

## Practical reading order

If you are trying to understand the Bank19_20 code surface quickly, read in this order:

1. `OnFieldRoutine`
2. `OnFieldRoutineOwnershipMap`
3. `OnFieldPlayCoordinator`
4. `OnFieldGameState`
5. the focused service types
6. `CommandRuntimeBoundaryHoldingArea`
7. the `Bank19*` conversion inventory types

That order moves from source-bank responsibilities, to runtime ownership, to runtime behavior, to conversion metadata.
