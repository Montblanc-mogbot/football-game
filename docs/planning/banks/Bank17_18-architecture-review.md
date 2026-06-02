# Bank17_18 — architecture review

Updated: 2026-06-02

## Purpose

This note reviews `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank17_18_main_game_loop.asm` from an architecture perspective.

The goal is not to catalog every menu or helper routine. The goal is to identify which responsibilities in Bank17_18 are likely to shape the future MonoGame architecture, and which responsibilities are mostly NES/UI/plumbing details that should not control the design.

## Short version

Bank17_18 is the game's **high-level flow coordinator**.

It is responsible for:
- entering the game from menus or season modes
- loading the right teams/control modes/context
- moving data between season storage and in-game state
- starting gameplay and related tasks
- sequencing quarter/halftime/overtime flow
- updating the game clock
- transitioning into post-play / post-game scoreboards and season bookkeeping

So if Bank21_22 looks like the **per-player behavior runtime**, Bank17_18 looks like the **match/session orchestration layer** around it.

## Top-level architectural role

At a high level, Bank17_18 appears to sit above the on-field gameplay banks.

It does not define the detailed player command language or the on-field movement/collision logic. Instead, it:
- decides **when** a game starts
- decides **what kind** of game is being played
- prepares the in-game data context
- launches the on-field gameplay task in the correct context
- watches quarter/game progression
- routes to halftime, overtime, standings, season updates, and related metagame transitions

That makes it one of the clearest candidates for the future **game flow / game mode / match orchestration** layer in MonoGame.

## Major responsibility groups

### 1. Front-end flow and mode selection
The early part of the bank handles:
- intro
- start screen
- sound mode access
- main menu
- preseason menu
- season menu
- pro bowl menu
- team data screens
- team selection and control-type selection

### Architecture implication
This strongly suggests a modern split between:
- front-end screens / menus
- persistent campaign or season state
- match-start setup

### What is real game structure here
Real structure:
- the game has multiple top-level modes
- those modes feed different team/control/match contexts into gameplay
- season/pro bowl/preseason are not just visual skins; they change how state is loaded, saved, and advanced

Mostly NES/plumbing details:
- nametable management
- sprite positioning for arrows/logos
- IRQ split-screen setup
- explicit PPU buffering logic
- direct scene-bank draw orchestration

## 2. Match setup and handoff into gameplay
Key entry points include:
- `TRANSFER_TEAM_INFO_AND_START_GAME`
- `SEASON_GAME_START`
- `PRO_BOWL_GAME_START`
- `DO_GAME`

These routines do important setup such as:
- clearing in-game stats
- transferring season/team data into in-game state
- updating injury status before a game
- setting starters and playbooks
- deciding whether to run normal gameplay or skip/sim mode
- showing matchup / week / playoff context screens

### Architecture implication
MonoGame likely needs a **match bootstrap pipeline** that is distinct from both menus and live gameplay.

Something like:
- select game context
- construct match/session state
- load teams/playbooks/starters/conditions
- enter either playable mode or sim mode

That is a real architectural seam.

## 3. Season/in-game state transfer
One of the most important responsibilities here is the repeated transfer between long-lived season data and temporary in-game data.

Important routines include:
- `CLEAR_IN_GAME_STATS_TRANSFER_SEASON_INFO`
- `SAVE_TEMP_P1_P2_TEAM_INFO_TO_SEASON`
- `SET_CUR_STARTERS_PLAYBOOKS_TO_SEASON`
- `UPDATE_INJURY_STATUS`
- `ADD_GAME_STATS_TO_SEASON_STATS_UPDATE_GAME_INDEX`

### What this means semantically
Bank17_18 distinguishes between at least two important data lifetimes:

#### Persistent/season lifetime
- season playbooks
- starters
- injuries
- conditions
- standings / week progression
- playoff bracket state

#### Match/in-game lifetime
- current game stats
- current active starters and playbook state for this match
- temporary control/match context

### Architecture implication
This is a strong argument for separate modern objects such as:
- `SeasonState` or broader persistent league/campaign state
- `MatchContext` / `GameSessionState`
- explicit import/export between them

That separation looks fundamental, not accidental.

## 4. High-level quarter / halftime / overtime loop
The most architecture-bearing gameplay-side logic in this bank is the quarter flow.

Key areas include:
- `COIN_TOSS_START`
- `START_QUARTER_GAMEPLAY_UPDATE_CLOCK_TASK`
- halftime handling
- overtime handling
- playoff overtime special case
- end-of-quarter scoreboard flow
- end-of-game scoreboard/stats flow

### What the bank is doing
For a normal game it roughly does:
- coin toss
- initialize quarter state and timeouts
- run a gameplay/clock phase
- update player conditions between major phases
- show scoreboard transitions
- run halftime logic
- continue through later quarters
- branch to overtime or final stats depending on score

### Architecture implication
This strongly suggests a future **match state machine** with coarse phases like:
- pregame
- coin toss
- quarter start
- live play phase
- quarter break
- halftime
- overtime
- postgame summary
- postgame persistence/update

That is a much higher-level state machine than the per-player Bank21/22 runtime.

## 5. Task/thread orchestration
Bank17_18 does not run everything inline. It creates and resumes tasks such as:
- gameplay task
- banner task
- clock/update loop coordination

Examples:
- it creates the on-field gameplay task pointing into Bank19
- it creates the banner task
- during halftime it resumes gameplay rather than always rebuilding everything from scratch

### Architecture implication
The NES expresses this through task slots and banked jump addresses.

The modern equivalent should probably be something like:
- a main update loop coordinating subsystems
- or a scene/mode object that owns child systems
- or a scheduler for coarse gameplay services

What matters is not preserving the task-slot mechanics. What matters is preserving the separation of concerns:
- live on-field gameplay
- banner/UI updates
- clock progression
- metagame transitions

## 6. Clock authority and quarter-ending rules
`START_QUARTER_GAMEPLAY_UPDATE_CLOCK_TASK` is especially important architecturally.

It appears to own:
- quarter clock initialization
- stopped vs running vs play-select countdown modes
- different countdown rates
- quarter-over detection
- last-seconds ticking sound behavior
- two-minute warning behavior
- overtime tie-check handling
- punt-clock-aware display behavior

### Architecture implication
This suggests a dedicated **game clock / period controller** rather than scattering clock logic through general gameplay code.

A good modern split might be:
- `GameClockController`
- `MatchPhaseController`
- `RulesController` for period transitions / overtime rules

This bank makes it pretty clear that the game clock is not just HUD formatting. It is a rules-bearing subsystem.

## 7. Skip/sim mode as a parallel game path
`DO_SIM_MODE_GAME_LOOP` is architecturally meaningful.

This is not a tiny shortcut. It is a distinct flow that:
- detects skip-mode control state
- loads sim-relevant data
- runs a separate sim/stat calculation path
- writes resulting stats back into in-game / persistent structures
- still routes through scoreboard/postgame presentation

### Architecture implication
MonoGame likely needs a top-level distinction between:
- live playable match runtime
- simulated match runtime

They should probably share:
- rules/state inputs
- resulting stat outputs
- postgame persistence flow

But they do not need to share the exact same inner execution model.

## 8. Season and playoff orchestration
Bank17_18 owns a lot of season-layer logic, including:
- current week handling
- playoff matchup lookup
- playoff bracket presentation
- standings/rankings/NFL leaders screens
- advancing season results after games
- post-season seeding/sorting logic

### Architecture implication
This argues for a distinct **league/season domain layer** above the match layer.

Likely modern split:
- front-end/menu layer
- season/league state layer
- match orchestration layer
- on-field gameplay runtime layer

That separation seems much healthier than letting one giant gameplay class absorb everything.

## What looks mostly NES/platform/UI plumbing
These areas should not drive the MonoGame architecture directly:
- PPU address writes and tile buffering
- nametable/sprite clearing
- IRQ split data loading
- MMC3 bank swaps
- SRAM checksum and write-enable mechanics as hardware details
- direct scene-bank draw commands
- per-screen arrow sprite management

They matter for source understanding, but they are mostly delivery mechanisms.

## What looks structurally important for the rewrite
These responsibilities probably do need first-class architectural homes:

### A. Match bootstrap / game-start pipeline
Because Bank17_18 repeatedly prepares a match context before gameplay begins.

### B. Persistent season state vs transient in-game state
Because the bank explicitly transfers data between them.

### C. Match-phase state machine
Because the quarter/halftime/overtime/game-over flow is explicit and rules-bearing.

### D. Clock/period controller
Because the clock is an actual rules subsystem, not just UI.

### E. Mode routing between live play and sim/skip play
Because skip mode is a distinct execution path.

### F. Postgame persistence/update pipeline
Because results have to flow back into season/playoff structures.

## Likely modern architecture pressure from this bank
If I translate the pressure of this bank into modern design concerns, I get something like:

- `FrontEndMode` / `MenuFlowController`
- `SeasonState` / `LeagueState`
- `MatchSetupService`
- `MatchStateMachine`
- `GameClockController`
- `OnFieldGameplayRuntime` (probably hosted elsewhere, especially Bank19/21/22 territory)
- `SimulationRuntime` for skip/sim mode
- `PostGamePersistenceService`

I would not lock those names in yet, but the **layer boundaries** feel real.

## Relationship to Bank21_22
This review makes the Bank21_22 review even more important.

Working hypothesis now:
- **Bank17_18** = coarse match/session orchestration
- **Bank19_20** = on-field gameplay loop / live play sequencing
- **Bank21_22** = per-player command runtime and behavior execution
- **Bank5_6** = behavior-script content consumed by that runtime

If that holds, then the future MonoGame architecture will likely need at least two different kinds of state machine:
1. a **match-phase/game-flow state machine**
2. a **per-player behavior/script runtime**

## Current recommendation
Do not try to design the final architecture from Bank5_6 alone.

Bank17_18 shows that the real architectural center of gravity includes:
- game flow
- period/rules flow
- persistent vs transient state boundaries
- mode routing
- postgame update/persistence

So the rewrite should likely be organized around those boundaries first, then fit Bank21_22/Bank5_6 behavior execution inside that larger structure.
