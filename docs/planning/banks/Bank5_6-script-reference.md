# Bank5_6 — script reference

Updated: 2026-06-02

## Purpose

This note is a working reference for `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank5_6_off_def_play_data.asm`.

It is meant to answer a narrow question:

- what is stored in Bank5_6
- what command vocabulary does it use
- how is that script language consumed at runtime

This note is intentionally **descriptive, not prescriptive**. It documents the NES-side script semantics so later MonoGame architecture can preserve behavior without copying the assembly-era implementation shape.

## High-level mental model

Bank5_6 is not a bank of passive lookup data.

It is a large **reaction-script corpus** for on-field player behavior:
- offensive reaction scripts
- defensive reaction scripts
- local branches/jumps/loops inside those scripts
- bytecode-like commands that drive movement, snap logic, blocking, ball exchange, pass timing, celebrations, and other per-player behaviors

The runtime state machine itself lives outside Bank5_6.

### Practical split

- **Gameplay/runtime banks** maintain per-player execution state, frame progression, ball state, and command dispatch.
- **Bank5_6** supplies the script programs that the runtime executes for each player.

A good modern analogy is:
- engine/interpreter elsewhere
- behavior-script assets here

## Top-level structure of `Bank5_6_off_def_play_data.asm`

The bank is divided into two major sections:

1. `OFFENSE_PLAY_DATA:`
   - offensive player reaction scripts
2. `DEFENSE_PLAY_DATA:`
   - defensive player reaction scripts

Within those sections, the source is organized primarily as:
- `OFFENSE_PLAYER_REACTION_xxx`
- `DEFENSE_PLAYER_REACTION_xxx`
- local jump labels
- local loop labels

These are not simple flat tables. They form a graph of script entry points plus intra-bank control flow.

## What the runtime appears to do with this bank

Current understanding from `Bank19_20_on_field_gameplay_loop.asm` and `Bank21_22_play_commands_on_field_logic.asm`:

1. Play setup selects one reaction pointer per player.
2. That pointer is copied into player RAM as the player's current `PLAY_CODE_ADDR`.
3. The per-player update/task machinery is primed so the player will enter `DO_NEXT_PLAYER_COMMAND`.
4. `DO_NEXT_PLAYER_COMMAND`:
   - selects offense or defense script bank
   - reads the command byte at the current script address
   - decodes command type and arguments
   - advances the stored script pointer by command length
   - dispatches to the matching command handler
5. Many handlers then:
   - do some work immediately
   - wait or animate for one or more frames
   - sometimes rewrite another player's command/script state
   - return to process the next command later

So each player effectively has a small runtime state containing at least:
- current script/program counter
- command scheduling state
- normal player state such as movement/collision/ball-carrier state

## Important consequence for later conversion

The important thing to preserve is the **behavioral contract**, not the NES plumbing.

Preserve:
- script ordering
- command semantics
- branch/jump structure
- cross-player coordination behavior
- wait/blocking behavior across frames

Do not assume we should preserve literally:
- bank switching
- raw ROM pointers
- assembly trampolines
- command-address byte packing in player RAM

## Player-slot vocabulary used by the scripts

The command macros define a shared 0x00-0x0A slot vocabulary:

### Offensive slot nibbles
- `QB1 = $00`
- `RB1 = $01`
- `RB2 = $02`
- `WR1 = $03`
- `WR2 = $04`
- `TE1 = $05`
- `C = $06`
- `LG = $07`
- `RG = $08`
- `LT = $09`
- `RT = $0A`

### Defensive aliases over the same nibble space
- `RE = QB1`
- `NT = RB1`
- `LE = RB2`
- `ROLB = WR1`
- `RILB = WR2`
- `LILB = TE1`
- `LOLB = C`
- `RCB = LG`
- `LCB = RG`
- `FS = LT`
- `SS = RT`

This shared nibble mapping is important because many Bank5_6 commands target "player slot X" rather than a concrete roster identity.

## Command-language shape

The command set is defined by `reference/Tecmo_Super_Bowl_NES_Disassembly/macros/play_data_macros.asm`.

There are two broad encoding families:

### 1. Group commands (`<$C0`)
These use opcode ranges and often pack a player nibble or a small count in the low bits.

### 2. Single commands (`>=$C0`)
These use distinct opcodes and fixed or table-driven lengths.

## Command reference

Below is the current command reference organized by opcode family and macro name.

### `0x00-0x1F` — man-coverage assignment
- `manCoverageTight playerNibble, time`
- `manCoverageLoose playerNibble, time`

Meaning:
- assign a defender to cover a target player slot
- save the target slot and coverage time in player state
- begin/pass through man-coverage defensive logic

### `0x20-0x2F` — random branch
- `randomJumpTo probabilityNibble, newLocation`

Meaning:
- probabilistic branch to another script location
- source-level representation is a local branch/jump in the reaction script graph

### `0x30-0x3F` — block target player
- `blockPlayer playerNibble`

Meaning:
- choose a target slot to block
- enter blocking behavior until conditions change

### `0x40-0x4F` — chop block target player
- `chopBlockPlayer playerNibble`

Meaning:
- targeted block variant with its own chase/engage behavior

### `0x50-0x5F` — handoff target
- `handoffToPlayer playerNibble`

Meaning:
- initiate handoff flow to another player slot
- this is not only a ball-transfer marker; the runtime may redirect the target player's command state into receive-handoff logic

### `0x60-0x6F` — fake handoff target
- `fakeHandoffToPlayer playerNibble`

Meaning:
- same targeting pattern as handoff, but with fake-handoff behavior/animation semantics

### `0x70-0x7F` — pitch target
- `pitchToPlayer playerNibble`

Meaning:
- initiate pitch/toss flow to another player slot
- runtime may redirect the target player's command state into receive-pitch logic

### `0x80-0x8F` — pre-snap motion mirror/follow
- `motionFollowingPlayer playerNibble`

Meaning:
- a defender mirrors/follows the specified offensive motion player before the snap

### `0x91-0x94` — pass-target selection block
- `passChance2ReceiversAndPostCatch`
- `passChance3ReceiversAndPostCatch`
- `passChance4ReceiversAndPostCatch`
- `passChance5ReceiversAndPostCatch`

Meaning:
- CPU pass-decision block
- encode a number of receiver candidates plus a post-catch script location
- runtime uses weighted receiver selection and stores a post-catch target script pointer for the eventual receiver

### `0xA0-0xAF` — pass-target priority registration
- `setRouteNumber routeNumber`

Meaning:
- despite the name, this appears to register the current player into the pass-target progression order
- runtime stores the player in `PASS_TARGETS[priority]`
- if the priority is first target, runtime also updates `CURRENT_PASS_TARGET`

This looks more like **target-order metadata** than route geometry.

### `0xB0`, `0xB1`, `0xB4` — kickoff positioning/movement
- `setPositionFromKickoffB0 y, x`
- `setPositionFromKickoffB1 y, x`
- `moveDuringKickoff y, x`

Meaning:
- special-teams kickoff placement/movement commands
- these appear tied to kickoff-specific positioning logic rather than generic play movement

### `0xC0` — QB dropback
- `dropback y, x`

Meaning:
- move QB according to encoded dropback coordinates relative to the scripted context

### `0xC1` — CPU/coach pass timing window
- `COACOMPassTiming startTime, endTime, takeSackChance`

Meaning:
- choose an actual wait duration within a timing window
- optionally allow early progression when pressure closes in
- acts as a CPU QB decision/timing gate, not the throw itself

### `0xC4` — celebrate
- `celebrate time`

### `0xC5` — cry
- `cry time`

Meaning:
- post-play or cutscene-style player reactions

### `0xC7` — COM-only branch
- `COMJumpTo newLocation`

Meaning:
- branch only when runtime control conditions say this is the COM side/path

### `0xC8` — CPU-juice conditional branch
- `basedOnJuiceCOMJumpTo juiceCompareValue, newLocation`

Meaning:
- conditional branch gated by CPU boost/juice state and control conditions
- current tracing suggests this is better understood as a CPU-state threshold branch than a pure random jump

### `0xCA` — coach/COM branch
- `COACOMJumpTo newLocation`

Meaning:
- branch when the relevant team/control mode is not manually controlled

### `0xCC` — generic block mode
- `block`

Meaning:
- enter pass-block / block-nearby-player behavior

### `0xCD-0xCF` — pull movement setup
- `pullRelative y, x`
- `pullBallPlacement y, x`
- `pullMiddleOfField y, x`

Meaning:
- line/pull style movement commands with different coordinate anchors

### `0xD0-0xD1` — set snap/start position
- `setPositionBallPlacement y, x`
- `setPositionMiddleOfField y, x`

Meaning:
- set player position relative to ball placement or field midpoint
- used for formation/start alignment

### `0xD2-0xD3` — initiate snap
- `hikeUnderCenter`
- `hikeFromShotgun`

Meaning:
- snap-trigger side of the exchange
- distinct from the QB-side receive-snap commands below

### `0xD4-0xD6` — receive snap
- `takeSnapUnderCenter`
- `takeSnapFromShotgun`
- `takeSnapForFGXP`

Meaning:
- blocking receive-ball commands
- wait until snap state is active
- update ball-carrier / possession state when the ball reaches the player
- then continue after a short delay

### `0xD7-0xD9` — generic movement commands
- `moveRelative y, x`
- `moveBallPlacement y, x`
- `moveMiddleOfField y, x`

Meaning:
- move to scripted locations using different coordinate anchors

### `0xDA` — run rush
- `runRush`

Meaning:
- enter rushing/chase behavior; exact semantics should be validated later against the broader movement runtime

### `0xDB` — vertically mirror ball carrier
- `verticallyMirrorBallCarrier`

Meaning:
- defensive/reactive mirroring behavior based on the ball carrier

### `0xDD` — pass rush
- `passRush`

Meaning:
- enter pass-rush behavior

### `0xDF` — computer takes control
- `computerTakesControl`

Meaning:
- return/force control to CPU logic for the player

### `0xE0-0xE3` — skill/stat tuning
- `setRS value`
- `setMS value`
- `boostRP boost`
- `boostRS boost`

Meaning:
- set or modify runtime skill/movement parameters for the player during the script

### `0xE4` — player takes control
- `playerTakesControl`

Meaning:
- switch active/manual control handling to this player
- used in some snap/return contexts

### `0xE5-0xE8` — special-teams/kick actions
- `kickoff`
- `punt`
- `fieldGoal`
- `extraPoint`

Meaning:
- enter the relevant kicking routine/state

### `0xEA`, `0xEC` — wait for snap stance
- `waitForSnap3PointStance`
- `waitForSnap2PointStance`

Meaning:
- pre-snap wait commands with stance/animation behavior

### `0xEB`, `0xED` — shift/motion pre-snap
- `shift time`
- `motion time`

Meaning:
- scripted pre-snap movement sequences

### `0xEE` — QB stance
- `qbStance`

Meaning:
- pre-snap QB pose/stance behavior

### `0xEF` — change player icon to returner
- `changePlayerIconToReturner`

Meaning:
- special-teams UI/control helper for the return player

### `0xF0` — face direction
- `faceDirection direction`

Meaning:
- explicit facing-direction update

### `0xF3-F5` — simple wait/pose timing
- `stand time`
- `turn time`
- `wait startTime, endTime`

Meaning:
- stand/turn animation waits and generic randomized wait windows

### `0xF6-F7` — HP tuning
- `setHP value`
- `boostHP boost`

Meaning:
- set or modify HP/toughness-related runtime parameter

### `0xF8` — infinite loop
- `infiniteLoop`

Meaning:
- explicit loop/hold state
- often likely used as a terminal or parked behavior

### `0xFA` — recover ball
- `recoverBall`

Meaning:
- special loose-ball recovery behavior

### `0xFC-0xFD` — set grapple/block bitmasks
- `setToGrapple byteTargetPlayerBitStringA, byteTargetPlayerBitStringB`
- `setToBlock byteTargetPlayerBitStringA, byteTargetPlayerBitStringB`

Meaning:
- multi-target selection masks for grapple/block logic

### `0xFE` — relative loop branch
- `loopTo newLocation`

Meaning:
- compact local loop branch using a relative offset

### `0xFF` — absolute jump
- `jumpTo newLocation`

Meaning:
- direct jump to another script location
- this is the basic absolute control-flow instruction used throughout the bank

## Behavioral patterns seen in the bank

Common patterns observed during review:
- pre-snap setup + wait-for-snap
- QB receives snap, then handoff/pitch/dropback logic
- CPU-only branching based on control mode or CPU boost state
- pass-target registration followed by pass timing and throw selection
- special-teams-specific scripts for kickoff, punt, FG, XP, and returns
- defensive mirroring / pass-rush / man-coverage reactions
- post-play celebration/cry/recovery flows

## What this note does not settle yet

This note does **not** attempt to lock in the final MonoGame architecture.

Open implementation questions that should wait for the main gameplay loop and broader runtime design:
- whether scripts become an interpreted bytecode layer, authored data assets, coroutines, explicit state objects, or a hybrid
- how much of Bank5_6 should remain source-shaped at runtime versus compiled into richer objects
- how generic the eventual per-player behavior system should be beyond parity needs

## Current best summary

Bank5_6 is best understood as:
- a large source-faithful library of offensive and defensive **behavior scripts**
- consumed by a separate gameplay runtime that already acts like a small per-player script interpreter/state machine
- rich enough to coordinate ball state, waits, jumps, control-mode branches, and cross-player handoff/pitch transitions

That makes it a critical parity source, but not a reason to copy the NES implementation structure literally.
