# Critical bank conversion packets

Updated: 2026-05-26

This note breaks the six critical Tecmo Super Bowl banks into bounded packet-sized units for a **fresh** MonoGame conversion.

It intentionally avoids assuming any prior implementation exists.
If future work decides to reuse old code from another repository, that should be an explicit choice made later.

## Core rule

Each packet should be executed as a source-bank-first conversion task:
- inspect the original assembly bank
- describe the bounded responsibility being ported
- define the MonoGame-side replacement/reimplementation boundary
- leave behind a validation note appropriate to the packet

## Bank5_6_off_def_play_data.asm

### Packet 5A — offensive script model extraction
- Responsibility: identify one offensive play family and model its command structure in a modern data form.
- Goal: prove we can represent one bounded offensive script family faithfully without inventing broader architecture first.

### Packet 5B — defensive script model extraction
- Responsibility: identify one defensive play family and model its command structure in a modern data form.
- Goal: prove one bounded defensive family can be expressed clearly enough for later runtime work.

### Packet 5C — shared play-command vocabulary
- Responsibility: define the smallest useful command vocabulary needed by packets 5A and 5B.
- Goal: create a minimal modern representation for original play-command semantics.

## Bank12_13_sim_update_stats.asm

### Packet 12A — clock and quarter bookkeeping slice
- Responsibility: isolate one bounded timing/bookkeeping responsibility from the original bank.
- Goal: describe what must be reimplemented faithfully versus what is just storage/plumbing.

### Packet 12B — per-play stat bookkeeping slice
- Responsibility: isolate one bounded stat family from the original bank.
- Goal: define the original state transitions and outputs for later runtime implementation.

### Packet 12C — season/meta bookkeeping slice
- Responsibility: isolate one bounded season or meta bookkeeping responsibility.
- Goal: avoid treating the whole season layer as one giant task.

## Bank17_18_main_game_loop.asm

### Packet 17A — front-end state sequence slice
- Responsibility: map one bounded front-end/game-entry transition chain.
- Goal: identify the real original state machine rather than inventing a fresh flow too early.

### Packet 17B — post-play orchestration slice
- Responsibility: isolate one bounded next-play/post-play orchestration responsibility.
- Goal: understand where the original game loop owns transitions.

### Packet 17C — interruption-state slice
- Responsibility: isolate one interruption flow such as pause, timeout, halftime gating, or equivalent.
- Goal: keep main-loop responsibilities decomposed into understandable units.

## Bank19_20_on_field_gameplay_loop.asm

### Packet 19A — snap-to-live-ball phase slice
- Responsibility: isolate one early play-phase transition in the on-field loop.
- Goal: make the on-field lifecycle explicit before writing new runtime code.

### Packet 19B — whistle/dead-ball phase slice
- Responsibility: isolate one late play-phase transition in the on-field loop.
- Goal: understand how live play becomes authoritative next-state setup.

### Packet 19C — control handoff slice
- Responsibility: isolate one player/ball/control ownership transition during a play.
- Goal: keep control semantics grounded in the original program.

## Bank20_playcall.asm

### Packet 20A — offense playcall flow slice
- Responsibility: map one bounded offense-facing playcall flow.
- Goal: preserve original play selection semantics without dragging in final UI concerns.

### Packet 20B — defense/CPU selection slice
- Responsibility: map one bounded defensive or CPU selection rule.
- Goal: keep playcall logic grounded in source behavior instead of generic menu design.

### Packet 20C — special-teams selection slice
- Responsibility: map one bounded special-teams playcall path.
- Goal: prevent special-teams choice flow from becoming an afterthought.

## Bank21_22_play_commands_on_field_logic.asm

### Packet 21A — offensive command semantics slice
- Responsibility: isolate one offensive command family and explain its intended runtime meaning.
- Goal: anchor future runtime code in source-bank semantics.

### Packet 21B — defensive reaction semantics slice
- Responsibility: isolate one defensive reaction family and explain its intended runtime meaning.
- Goal: avoid replacing source behavior with generic pursuit/coverage assumptions.

### Packet 21C — interaction edge-case semantics slice
- Responsibility: isolate one command-driven interaction edge case.
- Goal: capture important source rules before they get blurred into generic collision code.

## Suggested first packet order

1. `5A` — offensive script model extraction
2. `5C` — shared play-command vocabulary
3. `21A` — offensive command semantics slice
4. `17A` — front-end state sequence slice
5. `19A` — snap-to-live-ball phase slice
6. `12A` — clock and quarter bookkeeping slice

## Planning rule for future tasks

A future task should name the packet it is working on and keep its scope tight.
Avoid broad asks like "start the game conversion" when the real unit of work is one source-bank responsibility.
