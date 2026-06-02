# Bank19_20 inventory and responsibility map

## Purpose of this note

This note inventories the major sections in `Bank19_20_on_field_gameplay_loop.asm` and classifies them by modern ownership:

1. **On-field controller** (`OnFieldPlayCoordinator`-style host responsibilities)
2. **Supporting services/handlers** that the controller should call rather than absorb
3. **Neighbor systems** that should live outside the controller entirely
4. **Areas that need extra design attention** because the assembly bank mixes several responsibilities together

The main takeaway is that `Bank19_20` is **not** one clean modern class. In the NES version it acts like a large on-field host bank that owns play-flow orchestration, script assignment/reassignment, pre-snap control, special-teams transitions, play-end adjudication, and a grab bag of rendering/audio/stats/cutscene/injury helpers.

That is exactly why the assembly looks more complicated than the likely MonoGame design.

---

## High-level classification

### Should belong to the future on-field controller
These are the sections that directly decide **what phase of the play is active**, **which script families should be loaded**, **when to transition to another on-field phase**, and **when the play is over**.

- game-play entry / kickoff-side routing
- play select and initial play load
- run/pass/punt/FG/XP/onside/interception/fumble/return flow control
- possession-change transitions
- first-down / touchdown / safety / touchback / play-over adjudication
- quarter-over checks that affect live on-field flow
- pre-snap defender-selection / snap gating
- script assignment and script retargeting calls
- CPU play-call decisions at the on-field host layer

### Should probably be supporting services called by the controller
These are important, but they are narrower than the host itself.

- offensive/defensive play-data loaders
- formation loaders
- player-script assignment and retargeting helpers
- player-skill loading helpers
- pass-target / nearby-defender prioritization
- scoreboard/banner update helpers
- field-scroll and line-of-scrimmage marker helpers
- cutscene selection / cutscene sequence helpers
- injury-check / injury-animation helpers

### Should live outside the on-field controller entirely
These are neighboring systems that the controller should invoke, not own.

- per-player script execution (`Bank21_22` / `PlayerScriptRunner`)
- low-level player movement/physics/collision tasks
- rendering task execution
- audio playback
- stat persistence / stat calculators
- animation/cutscene presentation
- clock and game-status primitives

---

## Inventory by bank section

## 1) Entry and top-level play-phase routing

### `_GAME_PLAY_START_CHECK_FOR_KICK_TEAM`
**Role:** decides who starts with kickoff-side handling / entry path.

**Modern ownership:** controller

**Why:** this is pure on-field flow entry logic.

### `_P2_KICKOFF`, `_P1_KICKOFF`
**Role:** kickoff-specific live-play hosting for each side.

**Modern ownership:** controller

**Likely MonoGame shape:** kickoff phase/state inside `OnFieldPlayCoordinator`, with delegated helpers for setup and script assignment.

### `_P1_PLAY_SELECT_AND_PLAY_LOAD`, `_P2_PLAY_SELECT_AND_PLAY_LOAD`
**Role:** takes selected play information and initializes the on-field live-play setup.

**Modern ownership:** controller + supporting play-load service

**Why mixed:** the host decides *when* a play is loaded, but the detailed table/script loading can move into services.

### `_P1_RUN_PLAY`, `_P2_RUN_PLAY`
### `_P1_PASS_PLAY`, `_P2_PASS_PLAY`
### `_P1_PUNT_PLAY`, `_P2_PUNT_PLAY`
### `_P1_FG_PLAY`, `_P2_FG_PLAY`
### `_P1_ONSIDES_RETURN`, `_P2_ONSIDES_RETURN`
### `_P1_INTERCEPTED`, `_P2_INTERCEPTED`
### `_P1_SACK_OR_SCRAMBLE`, `_P2_SACK_OR_SCRAMBLE`
### `_P1_PASS_TIPPED_RESULT`, `_P2_PASS_TIPPED_RESULT`
### `_P1_PLAY_OVER_NORMAL`, `_P2_PLAY_OVER_NORMAL`
### `_P1_SAFETIED`, `_P2_SAFETIED`
### `_P1_TD`, `_P2_TD`
### `_P1_TO_P2_POSSESSION_CHANGE`, `_P2_TO_P1_POSSESSION_CHANGE`
**Role:** these sections represent the main on-field phase family for each side and special result transitions.

**Modern ownership:** controller

**Why:** this is the strongest evidence that `Bank19_20` is the host/orchestrator bank. These routines own the large-grain transitions: normal live play, sacks/scrambles, interceptions, returns, safeties, touchdowns, and possession flips.

**Design note:** these should likely become explicit play-phase states or clearly named coordinator methods, not dozens of monolithic methods in one class.

---

## 2) Play-end adjudication and state progression

### `_CHECK_FOR_FIRST_DOWN_OR_TOD`
### `_UPDATE_HASHMARK_FOR_NEXT_SNAP`
### `_CHECK_FOR_TD`
### `_CHECK_FOR_TOUCHBACK`
### `_CHECK_FOR_SAFETY`
### `_CHECK_FOR_PLAY_OVER`
### `_CHECK_FOR_FUMBLES_TOSS_AND_NORMAL`
### `_ONSIDE_AND_FUMBLE_RECOVERY_LOGIC`
### `_P1_RECOVERS_FUMBLE`, `_P2_RECOVERS_FUMBLE`
### `_CHECK_FOR_QTR_OVER`
### `_CLEAR_VARIABLES_FOR_XP_KICKOFF`
**Role:** determine whether the current live play changes phase, ends, changes possession, or advances the broader game state.

**Modern ownership:** split between controller and rules/result services

**Recommended split:**
- controller owns the **when do I ask these questions?** part
- rules/result services own the **how do I determine the outcome?** part

**Possible service names:**
- `PlayOutcomeEvaluator`
- `PossessionTransitionService`
- `SpotAndHashService`
- `SpecialTeamsRecoveryService`

**Needs extra consideration:** the assembly likely interleaves raw state mutation, rules evaluation, and transition decisions inside the same routines.

---

## 3) Support for task/game-status coordination

### `_END_SPECIFIC_TASKS`
### `_SET_GAME_STATUS_ON_FIELD_START_PLAYER_TASK`
**Role:** task orchestration and game-status setup around on-field activity.

**Modern ownership:** mostly outside the controller, but invoked by it

**Modern shape:** this looks like platform/runtime plumbing. In MonoGame this is more likely handled through gameplay-session state, update subscriptions, or subsystem activation rather than explicit NES task-bank manipulation.

---

## 4) Pre-snap control and snap gating

### `_DEFENDER_CHANGE_BEFORE_HIKE`
### `_CHECK_SNAP_PUNT`
**Role:** manage pre-snap defender switching, man/CPU control branches, snap timing, and punt snap timing.

**Modern ownership:** controller + input/control services

**Why important:** this is one of the clearest “assembly is doing too much in one place” areas. It mixes:
- pre-snap phase ownership
- control-mode branching (man/com vs man/com)
- active defender selection
- icon/UI updates
- snap timing rules
- immediate script-pointer priming when the snap happens

**Likely split:**
- `OnFieldPlayCoordinator` owns the pre-snap phase
- `PreSnapControlService` handles defender cycling and snap eligibility/timing
- `ControlAssignmentService` or `UserControlService` handles current man-controlled player updates
- `PlayAssignmentService` or `ScriptAssignmentService` handles the script-pointer swap at snap time

**Needs extra consideration:** this area likely deserves its own follow-up note because it is a compact example of why the class-based version can be simpler than the assembly.

---

## 5) Play-data and script-loading family

### `_LOAD_P1_OR_P2_OFF_PLAY_INFO`
### `_LOAD_OFF_FORMATIONS`
### `_LOAD_DEF_PLAY_INFO`
### `_LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
**Role:** load offensive play metadata, formation pointers, defensive play pointers, and then copy/update per-player script addresses in RAM.

**Modern ownership:** service layer, called by controller

**This is the main Bank19_20 service-family already visible in assembly:**
- `LOAD_PLAYER_SCRIPT_ADDR_INTO_PLAYER_RAM`
- `UPDATE_ALL_P1_PLAY_CODE_ADRR`
- `UPDATE_ALL_P1_PLAY_CODE_ADRR_EXCEPT_MAN`
- `UPDATE_ALL_P2_PLAY_CODE_ADRR`
- `UPDATE_ALL_P2_PLAY_CODE_ADRR_EXCEPT_MAN`
- `SET_ALL_PLAYER_SCRIPT_ADRR_EXCEPT_MAN`

**Recommended modern ownership:**
- `PlayAssignmentService`
- maybe a narrower `PlayerScriptAssignmentService`

**Why not the controller itself:** these routines are not deciding overall play flow; they are implementing a specific responsibility: assigning and retargeting script cursors for player groups.

**Important connection:** this is the strongest evidence that Bank19_20 contains service-like handlers embedded in the host bank.

---

## 6) Roster/skills loading

### `_LOAD_SKILLS`
**Role:** populate player RAM with skill data.

**Modern ownership:** separate roster/player-data service

**Possible names:**
- `PlayerSkillLoader`
- `RosterSnapshotBuilder`
- `PlayerAttributeService`

**Why outside controller:** this is setup/data hydration, not phase orchestration.

---

## 7) Audio/banner/UI/field-presentation helpers

### `_STOP_CURRENT_SONG`
### `_SET_ONFIELD_SONG`
### `_CHECK_FOR_UPDATE_BANNER`
### `_UPDATE_SCORE_FUNCTIONS`
### `_SIDE_CHANGE_BANNER_AND_SONG`
### `_UPDATE_SCROLL_LIMITS`
### `_START_DRAW_GAME_FIELD`
### `_UPDATE_LOS_MARKERS`
**Role:** on-field presentation coordination.

**Modern ownership:** presentation services outside core controller

**Controller relationship:** controller should trigger these, but not own their implementation.

**Possible services:**
- `OnFieldPresentationService`
- `BannerService`
- `FieldCameraService`
- `ScoreboardService`
- `OnFieldAudioDirector`

**Needs extra consideration:** `UPDATE_SCORE_FUNCTIONS` sounds broader than pure UI and may also mix score-state mutation with presentation.

---

## 8) CPU decision helpers

### `_CPU_PLAY_LOGIC`
**Role:** CPU kickoff/special teams play-choice logic.

**Modern ownership:** AI or play-selection service, called by controller

**Possible names:**
- `CpuPlaySelectionService`
- `SpecialTeamsDecisionService`

**Why:** this is decision support for the host, not host flow ownership itself.

---

## 9) Ball-targeting / pass-contest support

### `_SET_PLAYERS_CLOSE_TO_PASS`
### `_UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS`
**Role:** identify closest defenders, rank pass targets, retarget indicators, and prime collision/script behavior around pass outcomes.

**Modern ownership:** supporting gameplay services, not the main controller

**Possible split:**
- `PassTargetingService`
- `PassContestResolutionSetup`
- `ReceiverSelectionService`

**Why this matters:** these routines are substantial and gameplay-critical, but they are still subordinate to the live-play host. They should not bloat `OnFieldPlayCoordinator`.

---

## 10) Stats, distance, and post-play accounting

### `_UPDATE_STATS`
### `_CALCULATE_PLAY_DISTANCE`
**Role:** in-game stat updates, distance calculations, kick/pass/rush/interception accounting, and related stat persistence.

**Modern ownership:** stat/accounting layer outside controller

**Possible services:**
- `PlayStatRecorder`
- `PlayDistanceCalculator`
- `GameStatUpdateService`

**Why outside controller:** these are post-event accounting responsibilities.

**Needs extra consideration:** the assembly version likely uses live game state directly; a modern version should try to feed these services richer event/result objects instead.

---

## 11) Injury and cutscene systems

### `_INJURY_CHECK_NORMAL_AND_SKIP`
### `_CHECK_IF_PLAYER_CAN_BE_INJURED`
### `_PLAYER_CHANGE_INJURY`
### `_INJURY_ANIMATION`
### `_CUTSCENE`
### `_GENERATE_CUTSCENE_RANDOM`
### `_CUTSCENE_SEQUENCE_PTRS_AND_SEQUENCES`
### `_DRAW_RECOVER`
**Role:** injury determination, injury substitution/change flow, cutscene selection, cutscene sequence lookup, and recovery-draw presentation.

**Modern ownership:** separate injury/cutscene subsystems invoked by the controller

**Possible services:**
- `InjuryResolutionService`
- `RosterReplacementService`
- `CutsceneDirector`
- `CutsceneSequenceLibrary`

**Why outside controller:** these are specialized systems that happen because of play outcomes, not core on-field orchestration.

---

## Responsibility map summary

## Strong controller material
These feel like direct `OnFieldPlayCoordinator` ownership:

- gameplay entry routing
- play-type phase control
- possession-change and return-flow transitions
- play-over and scoring transition control
- pre-snap phase ownership
- deciding when to assign or retarget scripts
- deciding when to invoke result evaluators / presentation / stats / injury systems

## Strong service material already visible inside Bank19_20
These are the most obvious embedded service families:

- **Play/script assignment service**
  - `LOAD_PLAYER_SCRIPT_ADDR_INTO_PLAYER_RAM`
  - `UPDATE_ALL_P1_PLAY_CODE_ADRR*`
  - `UPDATE_ALL_P2_PLAY_CODE_ADRR*`
  - `SET_ALL_PLAYER_SCRIPT_ADRR_EXCEPT_MAN`

- **Pre-snap control service**
  - defender switching
  - snap gating
  - man/com branching around the snap

- **Play outcome / rules service**
  - first down / TD / touchback / safety / play-over / fumble / recovery checks

- **Pass targeting service**
  - closest-defender ordering
  - target priority
  - pass-target indicator updates

- **Stats/accounting service**
  - distance calculation
  - post-play stat recording

- **Presentation helpers**
  - banner, score, field draw, LOS markers, song changes

- **Injury/cutscene service family**
  - injury eligibility
  - injury replacement
  - cutscene randomization and sequence lookup

---

## What belongs elsewhere, even if Bank19_20 triggers it

### Bank21_22 / script runtime
The actual per-player command interpreter still belongs outside this controller inventory.

`Bank19_20` says things like:
- assign this script family
- retarget all defenders to recovery/chase behavior
- prime the active player for snap/control behavior

`Bank21_22` still owns:
- fetching the next instruction
- advancing the script cursor
- decoding and dispatching commands
- multi-frame command continuation

### Core simulation and rendering primitives
Any future MonoGame architecture should avoid making the controller own:
- low-level movement updates
- collision engine details
- actual draw-task execution
- audio playback implementation
- save/stat persistence implementation

---

## Design implication

Your intuition is right: the assembly version is more complex because it is a **bank-shaped host full of compressed responsibilities**, not because the modern design needs one giant class.

A likely cleaner MonoGame split is:

- `OnFieldPlayCoordinator`
  - owns play phase/state transitions
  - invokes the right services
  - decides when scripts are assigned or swapped

- `PlayAssignmentService`
  - loads initial reactions
  - retargets player groups after interceptions, fumbles, punts, onsides, etc.

- `PreSnapControlService`
  - defender selection
  - snap gating
  - control ownership before the snap

- `PlayOutcomeEvaluator`
  - touchdown / safety / touchback / first down / play-over / recovery results

- `PassTargetingService`
  - defender proximity ordering
  - target selection / target-indicator support

- `PlayStatRecorder`
  - distance and stat updates

- `InjuryResolutionService`
  - injury checks and roster changes

- `OnFieldPresentationService`
  - banner/audio/field/LOS helpers

- `PlayerScriptRunner`
  - the separate Bank21_22 interpreter/runtime layer

That structure is much easier to reason about than the bank, while still remaining faithful to its behavioral ownership.

---

## Recommended next follow-up

The most useful next Bank19_20-specific deep dive is probably:

1. **script-pointer families and reassignment triggers**
   - initial assignment
   - snap-time reassignment
   - interception/fumble/onside/punt-return retargeting
   - man-player exceptions

2. **pre-snap host responsibilities**
   - defender switching
   - snap gating
   - controller vs service boundary

Those two areas look like the cleanest bridge from assembly complexity to MonoGame classes.
