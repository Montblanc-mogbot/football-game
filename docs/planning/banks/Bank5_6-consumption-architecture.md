# Bank5_6 — runtime consumption architecture across Banks 17_18, 19_20, and 21_22

Updated: 2026-06-02

## Purpose

This note answers the architecture question more directly:

**Who consumes Bank5_6, and in what manner?**

The answer is not "Bank5_6 is the gameplay architecture."
Bank5_6 is the behavior-script content layer, and it is consumed through a runtime stack spread mainly across:
- `Bank17_18_main_game_loop.asm`
- `Bank19_20_on_field_gameplay_loop.asm`
- `Bank21_22_play_commands_on_field_logic.asm`

## Short version

The current working stack looks like this:

### Bank17_18
Owns the **match/session orchestration layer**.
It decides when a live on-field gameplay sequence begins and creates the gameplay task that enters Bank19/20.

### Bank19_20
Owns the **on-field host layer**.
It sets possession/play context, loads formations and play pointer families, copies the chosen Bank5_6 reaction-script addresses into player RAM, starts the live player-update task flow, and during turnovers/special-teams/post-catch events it retargets players to new script families.

### Bank21_22
Owns the **per-player command runtime**.
It takes a player's current `PLAY_CODE_ADDR`, reads the next Bank5_6 command, advances the pointer, and executes the command semantics. Many commands yield and resume across frames.

### Bank5_6
Owns the **script content**.
It provides the reaction-script graph that the Bank19/20 host assigns and that the Bank21/22 runtime steps through.

## The layered consumption picture

A useful mental model is:

1. **Bank17_18 starts a match phase**
2. **Bank19_20 configures the on-field situation and assigns scripts**
3. **Bank21_22 steps each player's assigned script**
4. **Bank5_6 supplies the actual instructions being stepped**

So the question is not simply "how is Bank5_6 read?"
It is:
- who chooses which script a player gets
- who stores that choice into live player state
- who ticks the current command each frame
- who redirects scripts when possession/state changes

Those responsibilities are split across the runtime stack.

## What Bank17_18 contributes

From the architecture review already captured, Bank17_18 is the **coarse game-flow controller**.

Relevant role in Bank5_6 consumption:
- starts the on-field gameplay task
- owns quarter/halftime/overtime/match flow
- decides when a live play or special-teams phase should begin
- hands control into the on-field layer rather than directly executing play scripts

Important clue:
- Bank17_18 creates the gameplay task pointing into `BANK_JUMP_ON_FIELD_GAMEPLAY_START`, which lands in Bank19/20.

### Implication
Bank17_18 does **not** directly consume Bank5_6 reaction commands.
It consumes them only indirectly by launching and sequencing the larger on-field runtime that uses them.

## What Bank19_20 contributes

Bank19_20 is where the Bank5_6 consumption story becomes concrete.

This bank appears to be the **live on-field orchestration host**. It sits between high-level match flow and low-level per-player command execution.

### Core responsibilities relevant to Bank5_6

#### 1. Entering the on-field gameplay phase
`BANK_JUMP_ON_FIELD_GAMEPLAY_START` / `ON_FIELD_GAMEPLAY_START` form the entry point from Bank17_18 into live field logic.

#### 2. Setting play context
Across kickoff, regular play, punt, field goal, extra point, interception return, punt return, onside recovery, and fumble/turnover transitions, Bank19_20 decides:
- which side has possession
- which formation is active
- which special-teams or turnover situation is active
- which scroll/UI/ball/field context applies

#### 3. Loading script pointer families into player RAM
This is the most direct Bank5_6-consumption responsibility.

Important routines include:
- `LOAD_P1_DEFENSE_PLAY_CODE_ADDRESSES`
- `LOAD_P2_DEFENSE_PLAY_CODE_ADDRESSES`
- `LOAD_PLAYER_SCRIPT_ADDR_INTO_PLAYER_RAM`
- `UPDATE_ALL_P1_PLAY_CODE_ADRR`
- `UPDATE_ALL_P1_PLAY_CODE_ADRR_EXCEPT_MAN`
- `UPDATE_ALL_P2_PLAY_CODE_ADRR`
- `UPDATE_ALL_P2_PLAY_CODE_ADRR_EXCEPT_MAN`

These routines show that Bank19_20 is the layer that:
- chooses a play-pointer table or script-pointer family
- iterates over players
- copies the chosen script address into each player's `PLAY_CODE_ADDR`
- primes command execution state such as `COMMAND_COUNTER`
- points players back at `JUMP_DO_NEXT_PLAYER_COMMAND`

That is the actual handoff from selected play context to executable Bank5_6 script state.

#### 4. Retargeting scripts during live play
Bank19_20 does not only assign scripts once at snap.
It also reassigns script pointers when play state changes.

Examples visible in the bank:
- interception return reassignment
- punt coverage vs punt return reassignment
- onside kick recovery reassignment
- fumble recovery / lost-fumble defense reassignment
- celebration / cry / chase-ball-carrier / recovery play-pointer family reassignment

This is a major architectural clue.

### Implication
Bank19_20 is not just a generic scene loop.
It is the **play-phase host that decides which Bank5_6 script family each player should be running right now**.

## How Bank19_20 appears to assign Bank5_6 scripts

The recurring pattern looks like this:

1. choose offense/defense/special-teams/turnover context
2. load formation or play data pointers
3. select the correct pointer table for each side
4. copy those script addresses into each player's RAM `PLAY_CODE_ADDR`
5. initialize or refresh command execution counters
6. let the player-update/task loop drive Bank21_22 execution

This means Bank19_20 is the main **script assignment and reassignment authority**.

## What Bank21_22 contributes

Bank21_22 is the **interpreter/execution layer** for the assigned scripts.

The central routine is `DO_NEXT_PLAYER_COMMAND`.

### What it does
For the current player, it:
- decides offense bank vs defense bank for script fetch
- reads the current `PLAY_CODE_ADDR`
- loads the next command byte and arguments
- advances the stored pointer according to command length
- dispatches to the handler for that opcode

### Why that matters
Bank21_22 is the layer that turns a Bank5_6 address into actual behavior.

Bank19_20 says:
- "this player should now run reaction script X"

Bank21_22 says:
- "okay, I will execute the next command inside reaction script X"

### Multi-frame execution
Many handlers do not complete instantly.
They:
- wait on snap/ball/collision/control conditions
- animate over several frames
- sometimes redirect another player's command address
- then resume later

So Bank21_22 is best thought of as a **resumable player-script runtime**, not merely a parser.

## The exact consumption chain

A cleaner end-to-end view:

### Step A — match/game flow (Bank17_18)
Bank17_18 determines that the game should enter a live playable phase and launches the on-field gameplay task.

### Step B — play-context setup (Bank19_20)
Bank19_20 sets possession, formation, play type, player skills, ball/field state, and other live-play context.

### Step C — script assignment (Bank19_20)
Bank19_20 chooses play-pointer families and copies Bank5_6 reaction-script addresses into each player's RAM.

### Step D — command stepping (Bank21_22)
Each player eventually enters `DO_NEXT_PLAYER_COMMAND`, which decodes and executes the assigned Bank5_6 script commands.

### Step E — reentry / retargeting (Bank19_20 + Bank21_22)
When world state changes—snap, reception, interception, fumble, punt return, turnover, play-over cutscene, etc.—Bank19_20 may assign a new script family, and Bank21_22 resumes stepping from the updated `PLAY_CODE_ADDR`.

## The most important architecture distinction

This review sharpens a critical boundary:

### Bank19_20 is not the same thing as Bank21_22
- **Bank19_20** = live play host, state transitions, script assignment/reassignment
- **Bank21_22** = per-player script stepping/execution

That distinction matters a lot for the rewrite.

If we collapse those together too early, we lose the actual source architecture shape.

## What this means for MonoGame architecture

If we translate the pressure of these banks into a modern design, the likely layers are:

### 1. Match/session flow layer
Bank17_18 territory.
- quarter flow
- halftime/overtime
- entering/exiting live play
- postgame transitions

### 2. On-field play host / play-phase controller
Bank19_20 territory.
- possession/play-context setup
- formation/play assignment
- special-teams and turnover transitions
- choosing which script family each player should receive
- reassigning scripts as live state changes

### 3. Per-player script runtime
Bank21_22 territory.
- current instruction pointer
- opcode decode/dispatch
- resumable multi-frame command execution
- command-local waiting/continuation logic

### 4. Script asset layer
Bank5_6 territory.
- reaction scripts
- instruction sequences
- labels/jumps/loops
- typed operands and player-slot references

## Why Bank19_20 matters before over-designing Bank5_6

This is the part I drifted past too quickly before.

Bank5_6 alone can tell us:
- what commands exist
- what local behavior scripts look like

But Bank19_20 tells us:
- **when** those scripts are assigned
- **why** they change during a play
- **which higher-level play states** force a script-family transition
- **how much of the live gameplay runtime is script-driven vs host-driven**

That makes Bank19_20 central to the real architecture picture.

## Provisional ownership model

Here is the cleanest current ownership model:

### Bank17_18 owns
- match lifecycle
- period transitions
- scoreboards/halftime/overtime/postgame
- starting the on-field gameplay task

### Bank19_20 owns
- live play hosting
- play setup for regular and special-teams phases
- assigning reaction scripts to players
- reassigning scripts on state changes like catches, picks, fumbles, returns, and blocked kicks
- possession/field/ball-flow integration around script execution

### Bank21_22 owns
- executing the next command for a player
- advancing per-player script pointers
- handling the semantics of script opcodes
- yielding/resuming command execution across frames

### Bank5_6 owns
- the source behavior programs those layers consume

## Current recommendation

The next architecture work should stay focused on **Bank19_20 as the Bank5_6 host layer**, not immediately on richer Bank5_6 semantic models.

Specifically, the next useful questions are:
- what are the main script-pointer families used by Bank19_20?
- which transitions reassign scripts mid-play?
- which parts of live play are driven by host-state transitions versus player-script commands?
- how should a future MonoGame play host hand scripts to a player runtime without copying assembly-era RAM plumbing?

That will give a much better architecture picture than continuing Bank5_6 asset work in isolation.
