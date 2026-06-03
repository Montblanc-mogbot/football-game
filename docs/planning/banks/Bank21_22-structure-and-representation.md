# Bank21_22 — structure and representation

Updated: 2026-06-03

## Purpose

This note defines the source-faithful representation shape for `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm`.

Bank21_22 is not a bank to flatten into bank-numbered production runtime names.
It is the per-player command-runtime bank that sits between:
- `Bank5_6_off_def_play_data.asm` as behavior-script content
- `Bank19_20_on_field_gameplay_loop.asm` as the on-field host/coordinator
- future MonoGame command-runtime code that should use gameplay-facing names rather than source bank numbers

## Source anchors covered

This conversion slice is grounded in:
- the top-of-bank gameplay/runtime constant block (`_PLAYER_ON_FIELD_CONSTANTS`)
- the exported bridge jumps at the start of the bank:
  - `BANK_JUMP_DO_NEXT_PLAYER_COMMAND`
  - `BANK_JUMP_DO_MOVEMENT_COLL_LOGIC`
  - `BANK_JUMP_WR_JUMP_DIVE_CHECK_PASS`
  - `BANK_JUMP_DEF_JUMP_DIVE_CHECK_PASS`
- every top-level `_F{ ... }` / `_F} ...` section in the bank
- the decode-and-dispatch core in `_PLAYER_COMMAND_PROCESSING`
- the command-length tables used to advance each player's script cursor

## Representation layers

### 1. Source-faithful extracted layer
Artifacts:
- `development-tools/bank21_22/extract_bank21_22.py`
- `content/game-data/bank21_22/generated/section-map.json`
- `content/game-data/bank21_22/generated/summary.json`

This layer preserves:
- all 164 top-level sections in source order
- each section's source span and labels
- the top-of-bank named constants that shape collisions, timing, throws, kicking, and thresholds
- the command-dispatch split between group commands and single commands
- the bridge-jump exports that Bank19_20 and other banks use to re-enter Bank21_22 behavior

The important preserved structure here is the bank's runtime organization, not literal 6502 bank-switch mechanics.

### 2. Decoded semantic model layer
C# types:
- `src/FootballGame/Conversion/PlayCommands/Bank21_22PlayCommandInventory.cs`
- `src/FootballGame/Conversion/PlayCommands/Bank21_22SectionRecord.cs`
- `src/FootballGame/Conversion/PlayCommands/Bank21_22LabelRecord.cs`
- `src/FootballGame/Conversion/PlayCommands/Bank21_22ConstantRecord.cs`
- `src/FootballGame/Conversion/PlayCommands/Bank21_22CommandDispatcherRecord.cs`
- `src/FootballGame/Conversion/PlayCommands/Bank21_22PlayCommandInventoryJsonLoader.cs`

These models intentionally stop at the conversion/runtime-boundary layer.
They preserve source-facing runtime structure without forcing the future gameplay runtime to expose names like `Bank21` or `Bank22` in production code.

### 3. Future runtime-consumption layer
This slice does **not** edit live gameplay runtime files yet.
Instead it leaves a clean handoff for later gameplay-facing names such as:
- `PlayerCommandRuntime`
- `PlayerCommandExecutionContext`
- `PlayCommandDispatcher`
- `PlayCommandCatalog`
- `PlayerCollisionResolver`
- `KickCommandResolver`

Those names should be chosen by responsibility, not by source bank number.

## Source-facing responsibility groups

Bank21_22 content falls into a few durable groups:
- **dispatch and decoding**
  - `DO_NEXT_PLAYER_COMMAND`
  - command-length tables
  - group/single command dispatch tables
- **command semantics**
  - man coverage, random branches, blocks, handoffs, pitches, motion, pass setup, dropbacks, branches/jumps, kick commands, tackle/fumble behaviors, return logic, and control handoff
- **gameplay helpers**
  - collision resolution, tackle/fumble adjudication, pass contests, direction/velocity updates, player-final-location checks
- **presentation and animation support**
  - sprite update helpers, tumble/flyback data, kick meter tile data, animation tables
- **lookup tables / thresholds**
  - hitting-power thresholds, distance/speed/gravity constants, catch/interception thresholds, player-skill tables

## Runtime naming rule

The goal of this conversion is to prove Bank21_22 can be represented without introducing bank-numbered runtime names into production gameplay code.

So this slice keeps bank-numbered names only in:
- source-faithful docs
- generated extraction artifacts
- conversion-layer models that explicitly describe source-bank structure

It does **not** require future runtime gameplay types to be named `Bank21Runtime`, `Bank22Dispatcher`, or similar.

## Relationship to Bank19_20

Bank19_20 still owns the on-field host/coordinator layer.
Bank21_22 owns the per-player command interpreter and command semantics.
The durable bridge remains:
- Bank19_20 assigns and retargets play-code/script cursors
- Bank19_20 primes pass-interaction and pre-snap/punt boundary state
- Bank21_22 decodes one player's next command, steps it, and mutates world/player/ball state until the command yields or completes

That boundary is why the Bank21_22 conversion layer should speak in command-runtime terms instead of bank-numbered production names.

## What this conversion does not preserve literally

This pass intentionally does not model the following as production architecture:
- MMC3 bank swapping
- RTS dispatch tricks
- zero-page temp layout
- raw RAM slot names as future C# field names
- numeric source-bank naming as the public runtime vocabulary

Those details remain valuable for parity review, but they are not the shape the MonoGame runtime should mirror.

## Practical result

Bank21_22 now has a parallel representation shape to the earlier full-bank passes:
- a durable structure note
- an extractor
- generated source-faithful artifacts
- typed conversion-layer models/loaders
- explicit validation evidence

The later gameplay runtime pass can now consume these artifacts while choosing coherent gameplay-facing names that describe command execution responsibilities instead of bank numbers.
