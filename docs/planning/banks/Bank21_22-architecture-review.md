# Bank21_22 — architecture review

Updated: 2026-06-02

## Purpose

This note reviews `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm` from an architecture perspective.

The goal is to identify what this bank actually contributes to gameplay structure, especially relative to:
- `Bank5_6_off_def_play_data.asm` as behavior-script content
- `Bank17_18_main_game_loop.asm` as high-level game/match flow
- `Bank19_20_on_field_gameplay_loop.asm` as the broader live-play host

## Short version

Bank21_22 is the game's **player-command runtime**.

It is responsible for:
- decoding the current opcode for a player from that player's script pointer
- advancing the player's script pointer according to command length
- dispatching to the matching command handler
- mutating player/ball/runtime state according to that command
- yielding across frames for commands that wait, animate, move, or poll conditions
- supporting script control flow such as branches, jumps, and CPU-only conditional behavior

So if Bank5_6 is the **content** of the behavior language, Bank21_22 is the **interpreter/executor** for that language.

## Top-level architectural role

`DO_NEXT_PLAYER_COMMAND` is the key anchor.

At a high level it does this:
1. determine whether the current player should read offense or defense script data
2. bank in the appropriate Bank5 or Bank6 script data
3. load the player's current `PLAY_CODE_ADDR`
4. read the current opcode and up to three operand bytes
5. classify the opcode as group-command vs single-command
6. use a command-length table to advance the player's script pointer
7. switch to the command-logic bank
8. jump through a command dispatch table into the handler

That is classic VM/interpreter structure, even if it is expressed in banked 6502 style.

## What this bank is not

Bank21_22 is not mainly:
- the match-phase controller
- the quarter/halftime/overtime rules host
- the persistent season-state layer
- the script content bank itself

Those responsibilities live elsewhere.

Bank21_22 is much more local: it is about **what one player does next**, using scripted commands plus current world state.

## Core runtime model

### Per-player instruction pointer
Each player has a current play/script address in RAM.

Bank21_22 repeatedly:
- reads from that address
- decodes one command
- updates the player's next address
- runs the command logic

That means the core abstraction is not "run the whole play at once."
It is closer to:
- each player owns a script cursor
- each update resumes that player from wherever their current command left them

### Frame-yielding command execution
Many handlers do not finish in one atomic step.

Instead they:
- perform setup
- wait for a condition or number of frames
- resume later
- only then continue to the next command

Examples include:
- snap receive commands waiting for ball-snap / ball-collision conditions
- block / coverage / movement commands that loop until state changes
- stance / wait / turn commands that intentionally consume time
- passing and kicking sequences that stage ball animation and then resume

### Architecture implication
A modern rewrite should not model these as plain one-shot methods that instantly complete.

Bank21_22 strongly suggests a runtime where commands can be:
- entered
- ticked over time
- completed
- then advance to the next instruction

That could be done with:
- explicit command-state objects
- a coroutine-like system
- a small VM stepper with per-command continuation state

But some notion of **multi-frame resumable execution** looks mandatory.

## Decode/dispatch structure

`DO_NEXT_PLAYER_COMMAND` splits opcodes into two families:

### Group commands
These are the compact commands below `$C0`.
They use nibble-packed or multi-byte formats and dispatch through `GROUP_COMMAND_TABLE`.

Examples include:
- man coverage
- random branch
- block / chop block
- handoff / fake handoff / pitch
- pre-snap motion mirroring
- CPU pass target/timing setup
- kickoff position setup

### Single commands
These are `$C0`-`$FF` and dispatch through `SINGLE_COMMAND_TABLE`.

Examples include:
- dropback
- pass-wait timing
- CPU-only conditional jumps
- pull-block movement variants
- snap/hike commands
- movement commands
- chase / mirror / pursuit behavior
- speed/power mutation commands
- kicking / punting / FG / XP
- stance / turn / wait commands
- collision/block masks
- branch / jump

### Architecture implication
The future script representation should preserve:
- opcode family distinctions where useful
- command identity
- command arguments
- exact control-flow behavior

But it does **not** need to preserve the literal 6502 table-jump shape or bank-switch sequence.

## Bank21_22 as semantics host for the Bank5_6 language

This bank confirms that Bank5_6's meaning is not just "movement routes."
The script language can:
- control ball exchange sequencing
- change player control ownership
- alter speed/rushing/hitting parameters
- set movement goals in different coordinate spaces
- wait on world conditions
- branch based on CPU/team-control state
- branch based on juice/randomness
- manipulate collision/block permissions
- initiate animation-heavy actions like kickoff/punt/FG/XP

So Bank5_6 should be treated as a **behavior scripting language**, and Bank21_22 is where much of that behavior vocabulary gets defined.

## Important architectural responsibility groups

### 1. Script instruction decoding and pointer advancement
This is the cleanest "VM" responsibility in the bank.

Important features:
- read current opcode
- parse arguments
- use command-length tables
- advance `PLAY_CODE_ADDR`
- dispatch to handler

### What matters for parity
- correct byte lengths
- correct argument interpretation
- correct offense/defense script-bank selection
- correct next-instruction behavior for jumps/branches

### What does not need literal preservation
- bank swaps
- RTS-based computed jumps
- zero-page temp variable layout

## 2. Command semantics live here, not in the content bank
The command handlers define what script opcodes actually mean.

Examples:
- `DO_ACTION_IF_COM_COMMAND_START`
- `COM_JUMP_BASED_ON_JUICE_COMMAND_START`
- `RECEIVE_SNAP_CENTER_COMMAND_START`
- `RECEIVE_SNAP_SHOTGUN_COMMAND_START`
- `MOVE_RELATIVE_COMMAND_START`
- `MAN_TAKE_CONTROL_COMMAND_START`
- `BRANCH_COMMAND_START`
- `JUMP_COMMAND_START`

### Architecture implication
The rewrite likely needs a separation between:
- serialized/deserialized script assets
- runtime command interpreter
- world/gameplay services the commands can call into

That is much healthier than burying script semantics directly inside raw content structures.

## 3. Commands depend heavily on gameplay services
Many handlers call out into broader gameplay systems rather than doing everything locally.

Examples visible in this bank include services for:
- player movement / facing / sprite update
- ball animation and ball ownership
- player collision state
- man-controlled player pointer updates
- displayed-name / UI marker updates
- play-status checks
- timing/yield helpers
- target-player lookup and addressing helpers

### Architecture implication
A modern interpreter probably should not directly own all gameplay logic.
It should orchestrate against subsystem interfaces such as:
- player-state service
- ball-state service
- control-ownership service
- animation/sprite service
- timing/wait service
- targeting/query service

That would match the actual dependency shape better than one monolithic script class.

## 4. Control-flow commands are parity-critical
`BRANCH_COMMAND_START` and `JUMP_COMMAND_START` confirm that script control flow is first-class.

Together with CPU-conditional commands like:
- `DO_ACTION_IF_COM_COMMAND_START`
- `COM_JUMP_BASED_ON_JUICE_COMMAND_START`
- `IF_COM_JUMP_COMMAND_START`

...this means the language is not just a linear list of actions.
It is a graph with conditional edges.

### Architecture implication
Any extracted representation for Bank5_6 should keep control flow explicit.
Possible modern shapes:
- raw instruction lists with jump targets
- label-aware instruction graphs
- structured semantic instructions that still retain exact branch destinations

What would be risky is flattening these into pre-resolved "state classes" too early.

## 5. Human vs CPU control is part of script semantics
Several commands explicitly care whether a player/team is human-controlled or CPU-controlled.

This is not just UI or input routing. It changes script execution.

Examples:
- CPU-only jump commands
- CPU-juice-gated branches
- commands that hand off control to man-controlled play
- commands that update the currently controlled player and displayed player-name marker

### Architecture implication
The future runtime likely needs control-state access as part of the script execution context.

That means command execution depends not only on physics/position, but also on:
- control mode
- active human-controlled player
- possession side
- CPU boost/juice context

## 6. Command execution is world-state-sensitive
Many commands block until the world reaches a condition.

Examples:
- wait until ball snapped
- wait until ball collides with QB/holder
- wait until nearby players can collide
- wait/turn/stance for a timed duration
- chase/mirror logic that loops until position/state changes

### Architecture implication
This is another reason the rewrite needs a step/tick runtime, not just precomputed results.

A useful mental model is:
- Bank5_6 supplies instructions
- Bank21_22 executes them against the evolving world each frame
- completion of one instruction may depend on arbitrary runtime events

## 7. Address-to-player safety and runtime guards
Helpers like `UPDATE_LOCAL_PLAYER_COMMAND_ADDR_IF_VALID` and `UPDATE_PLAYER_COMMAND_ADDR_NOT_COL_JUMP_ON_GROUND` show that the runtime contains safety/guard logic before retargeting another player's command pointer.

That matters because some commands affect:
- other players
- targeted defenders/receivers/blockers
- players whose state may be invalid for the requested redirect

### Architecture implication
The modern runtime should preserve these semantic guards even if the representation changes.

In other words: if a command only retargets another player when that player is valid, upright, and not already colliding, that behavior is parity-relevant even if the exact RAM-address checks are not.

## What looks mostly NES/plumbing here
These should not dominate the MonoGame design directly:
- bank swapping between script data and command logic banks
- RTS-based dispatch tricks
- zero-page temp storage conventions
- sprite tile update details as assembly mechanism
- raw RAM-address range tests as literal implementation details

They are important clues, but not the design target.

## What looks structurally important for the rewrite

### A. Per-player script cursor/runtime context
Each player clearly needs runtime state for:
- current instruction pointer
- active command / continuation state
- command-local timing or wait counters
- command-target references where needed

### B. Resumable multi-frame command execution
This is a hard requirement.

### C. Explicit command semantics layer
Do not treat commands as generic data only. Many opcodes have rich semantics.

### D. World/query/service boundary
Commands need controlled access to ball/player/control/timing systems.

### E. Control-flow-preserving script asset model
Jump/branch destinations must remain explicit.

### F. Human/CPU control context inside the runtime
This affects command behavior directly.

## Likely modern architecture pressure from this bank
This bank suggests a future design along lines such as:
- `PlayerScriptRuntime` or `PlayerBehaviorInterpreter`
- `ScriptInstructionDecoder`
- `ScriptCommandRegistry` / handler dispatch
- `PlayerScriptExecutionContext`
- gameplay service interfaces for player, ball, control, and timing systems
- extracted Bank5_6 asset models that preserve jump targets and operands

Again, the exact class names are not the point. The separation of roles is.

## Relationship to the broader bank picture
Current working model now looks stronger:

- **Bank5_6** = offensive/defensive behavior-script content
- **Bank21_22** = interpreter/semantic runtime for those commands
- **Bank19_20** = broader live-play/on-field orchestration host
- **Bank17_18** = high-level match/session/season flow

That layered model is useful because it keeps us from forcing everything into one gameplay class.

## Current recommendation
When Bank5_6 conversion starts for real, target at least three layers:

1. **Extracted source-faithful script assets**
   - preserve command sequence data
   - preserve labels / jump targets / control-flow edges

2. **Semantic instruction model**
   - map opcodes to named behaviors and typed operands
   - keep parity-critical distinctions like CPU-only branches and wait semantics

3. **Runtime interpreter/execution layer**
   - one per-player execution context
   - multi-frame resumable command handling
   - service-based access into player/ball/control state

That feels much more source-faithful than flattening Bank5_6 into ad hoc OOP behaviors, and much more maintainable than copying the exact NES implementation shape.
