# Bank19_20 implementation review — 2026-06-03

## Scope

This review summarizes the full Bank19_20 conversion slice for `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm`, from the first inventory pass through the current host/runtime implementation and follow-up audits.

## What was completed

### 1. Bank architecture and source-faithful conversion foundation

We first treated Bank19_20 as a behavior/orchestration bank rather than a table-only bank.
That produced the durable planning/conversion foundation:

- `docs/planning/banks/Bank19_20-inventory-and-responsibility-map.md`
- `docs/planning/banks/Bank19_20-and-Bank21_22-monogame-class-sketch.md`
- `docs/planning/banks/Bank19_20-script-pointer-and-reassignment-matrix.md`
- `docs/planning/banks/Bank19_20-structure-and-representation.md`
- `docs/planning/banks/Bank19_20-loader-layer.md`
- `docs/planning/validation/Bank19_20-conversion-validation.md`
- `development-tools/bank19_20/extract_bank19_20.py`
- `content/game-data/on-field/generated/bank19_20-section-map.json`
- `content/game-data/bank19_20/generated/summary.json`

That work established the bank as:

- an on-field host/controller bank
- a script-assignment and retargeting bank
- a pre-snap control bank
- a play-outcome and special-teams adjudication bank
- a bridge into Bank21_22 command-runtime behavior

### 2. Typed conversion inventory layer

We added a conversion-side semantic model under `src/FootballGame/Conversion/OnField/` so the extracted JSON is represented as typed Bank19_20 inventory data rather than anonymous blobs.

This layer now preserves:

- explicit bank entrypoints
- script-pointer families
- external jump constants
- cross-bank dependencies
- section ownership/responsibility metadata
- Bank21_22 carry-forward bridges

### 3. Runtime-facing ownership map

We then represented the whole bank in runtime-facing code instead of leaving the bank as planning notes only.

Key runtime artifacts:

- `src/FootballGame/Gameplay/OnField/OnFieldRoutine.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldRoutineOwnershipMap.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
- `docs/planning/banks/Bank19_20-runtime-representation.md`

This gave the project a durable answer to: “where does each Bank19_20 responsibility live right now?”

### 4. First real host implementation

We moved from a bank map into actual host behavior with:

- `OnFieldPlayCoordinator`
- `OnFieldGameState`
- the first runtime enums for phase/play/kick/pass/touchdown ownership
- focused supporting services for assignment, pre-snap control, targeting, stats, injury/cutscene, presentation, task coordination, CPU decisions, and skill hydration

The first implementation slice covered:

- entry into on-field gameplay
- opening kickoff routing
- play-selection/load routing
- regular vs special-teams startup boundaries

### 5. Incremental behavior conversion passes

From there, the bank was extended in bounded slices rather than one giant rewrite.
The major completed runtime slices were:

- regular-play start and play-over routing
- run/pass/special-teams branching
- pass lifecycle and sack/no-throw handling
- punt flow
- field-goal / extra-point flow
- interception return routing
- touchdown aftermath routing
- onside and loose-ball recovery routing
- turnover possession-change/reset handling
- dead-ball finalization and next-sequence queueing
- quarter-end and special restart cleanup

### 6. Audit and parity passes

After the main runtime coverage existed, we ran explicit review passes instead of assuming correctness:

- assembly ↔ manifest presence verification
- coordinator ownership coverage audit
- code-quality review
- football-rules review with flow chart
- follow-up parity fixes for the highest-risk issues

The review artifacts added during that phase were:

- `docs/reviews/Bank19_20-code-quality-review-2026-06-02.md`
- `docs/reviews/Bank19_20-football-rules-review-2026-06-02.md`

## Main implementation outcomes

### Strong outcomes

The Bank19_20 slice now has real strengths:

- full section inventory with no “mystery area” left unrepresented
- a clear split between conversion inventory and runtime code
- explicit ownership boundaries instead of a hidden god-module migration
- explicit Bank19_20 ↔ Bank21_22 bridge tracking
- a host coordinator that now covers the major live-flow branches
- durable review docs that already identified parity and design risks

### Important fixes that landed

The strongest follow-up fixes during this bank were:

- turnover returns now route before pass-loop handling
- kickoff/punt dead-ball flow waits for actual resolution before series setup
- blocked punts stay in live loose-ball flow
- blocked field goals gained explicit loose-ball/recovery routing
- missed field goals now carry a source-directed next-snap spot adjustment
- safety restart flow now queues a free kick by the safetied team while preserving scoring-side possession
- XP exit flow no longer fabricates an ordinary possession-change path
- dead-ball teardown and next-play dispatch are less fragmented

## Architecture shape we ended up with

At the end of this bank, the modern structure is:

### Conversion layer
Preserves Bank19_20 as source-faithful inventory and ownership metadata.

### Runtime host layer
`OnFieldPlayCoordinator` owns match-phase routing, possession changes, and high-level play resolution.

### Supporting service layer
Focused helpers own narrower Bank19_20 responsibility families:

- `PlayAssignmentService`
- `PreSnapControlService`
- `PassTargetingService`
- `StatAccountingService`
- `InjuryCutsceneService`
- `OnFieldPresentationService`
- `TaskCoordinationService`
- `CpuPlayDecisionService`
- `PlayerSkillHydrationService`

### Bank21_22 bridge layer
`CommandRuntimeBoundaryHoldingArea` keeps the command-runtime handoff set visible so later work does not lose the host/interpreter boundary responsibilities.

## Current risks / limits

Bank19_20 is much healthier than it was at the start, but it is not “done forever.”
The most important remaining concerns are:

- `OnFieldPlayCoordinator` is still large and trends toward god-class scope.
- Several services still behave more like explicit boundary placeholders than full invariant-enforcing runtime systems.
- Some runtime state is still stringly typed (`formation`, `banner`, `song`, cutscene-style keys).
- The Bank21_22 command-runtime conversion still needs to consume the carry-forward bridge set cleanly.
- There is still a difference between “represented faithfully” and “fully gameplay-complete.”

## Bottom line

Bank19_20 went from:

- raw assembly
- to durable architecture notes
- to a source-faithful extracted inventory
- to typed conversion models
- to a full runtime ownership map
- to an actual host coordinator/service implementation
- to post-implementation audit and parity-fix passes

That is a solid full-bank conversion pass, not just a planning packet.

The next best follow-on work is not more broad Bank19_20 expansion for its own sake; it is tightening the runtime contracts around the current types and making sure the later Bank21_22 command-runtime work honors the Bank19_20 bridge assumptions cleanly.
