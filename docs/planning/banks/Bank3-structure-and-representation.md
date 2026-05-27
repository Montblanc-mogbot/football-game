# Bank3 — structure and representation

Updated: 2026-05-27

## Purpose

This note describes the full-bank structure of `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank3_formation_metatile_data.asm` and sets the representation rules for converting it into maintainable artifacts without losing parity-critical structure.

## Top-level bank organization

`Bank3_formation_metatile_data.asm` is organized as five major layers in order:

1. **Offensive formation pointer families**
   - `_F{_OFFENSIVE_FORMATION_POINTERS`
   - `KICKOFF_FORMATION_POINTERS` through `ONEBACK_2_FORMATION_POINTERS`
   - 22 formation families, each with exactly 11 ordered reaction pointers

2. **Normal offensive execution tables**
   - `_F{_OFFENSIVE_PLAY_POINTERS`
   - `OFFENSIVE_EXECUTION_1` through `OFFENSIVE_EXECUTION_92`
   - 92 execution tables, each with exactly 11 ordered reaction pointers

3. **Special offensive-play pointer tables**
   - `_F{_SPECIAL_OFFENSIVE_PLAY_POINTERS`
   - `TD_CELEBRATION_OFF_PLAY_POINTERS` through `UNUSED_7_OFF_PLAY_POINTERS`
   - 16 special tables, each with exactly 12 ordered reaction pointers

4. **Metatile pointer table**
   - `_F{_METATILE_DATA_POINTERS`
   - `METATILE_DATA_POINTERS`
   - 76 pointer entries in canonical source order
   - one intentional duplicate pointer target exists: `DEFAULT_HELMET_SHELL_METATILE_DATA` is referenced by both indices `0x3F` and `0x40`

5. **Metatile layout records**
   - `_F{_METATILE_DATA`
   - `DOUBLE_DECKER_STADIUM_WITH_SCOREBOARD_METATILE_DATA` through `BENGALS_HELMET_SHELL_P2_MATCHUP_METATILE_DATA`
   - 75 unique records because one pointer target is shared twice
   - every record has a seven-byte header plus a metatile grid body

## What is NES/assembly-driven vs what is real game structure

### Mostly assembly/NES representation details
- ROM pointer tables instead of typed references
- CHR-bank bytes and bank-offset bytes as cartridge-facing delivery details
- hand-authored `.HEX` payload rows for packed metatile indices

### Real game/domain structure that must survive conversion
- canonical formation-family order
- canonical player-reaction ordering inside each formation/execution/special table
- the distinction between ordinary offensive execution tables and special-play tables
- metatile layout header semantics: CHR bank pair, tile-bank offset, palette-set index, dimensions, and starting screen location
- row/column ordering of the metatile grids
- pointer-table aliases where multiple indices intentionally share the same layout record

## Structural invariants that must remain intact

The conversion must preserve:
- the 22-formation canonical order from the formation pointer section
- the fixed 11-entry width of formation families
- the fixed 11-entry width of the 92 normal offensive execution tables
- the fixed 12-entry width of the 16 special offensive-play tables
- the 76-entry metatile pointer-table order
- the duplicate aliasing of `DEFAULT_HELMET_SHELL_METATILE_DATA`
- the exact metatile layout dimensions implied by each seven-byte header
- row-major ordering of the metatile body bytes

## Representation split

### 1. Source-faithful extracted layer
This layer should preserve the source structure as directly as practical.

For Bank3, that means:
- formation-family ordering
- exact reaction labels in source order
- exact offensive execution table ordering
- exact special-play table ordering
- exact metatile pointer indices and targets
- exact metatile header bytes decoded into stable fields
- exact row/column metatile index layout

### 2. Decoded semantic layer
This layer removes pointer mechanics while preserving structure and meaning.

For Bank3, that means typed concepts like:
- `FormationId`
- `FormationFamilyRecord`
- `OffensiveExecutionRecord`
- `SpecialOffensivePlayRecord`
- `MetatileLayoutHeader`
- `MetatileLayoutRecord`

### 3. Runtime-consumption layer
This should stay separate and come later.

Examples:
- formation selection used by playcall/game-state code
- runtime mapping from execution tables into future Bank5/Bank21 command consumers
- background scene composition using modern MonoGame rendering instead of NES transfer mechanics

## What not to preserve mechanically in C#

We should **not** preserve raw ROM pointer chasing as the runtime API.

Do preserve:
- canonical ordering
- alias relationships
- header semantics
- row-major metatile layout structure

Do not preserve:
- bank-switching assumptions
- raw CHR-transfer plumbing as a gameplay-facing concept
- byte-pointer navigation as the main domain model

## Exact source anchors

Use these anchors when validating the conversion:
- `_F{_OFFENSIVE_FORMATION_POINTERS`
- `KICKOFF_FORMATION_POINTERS`
- `ONEBACK_2_FORMATION_POINTERS`
- `_F{_OFFENSIVE_PLAY_POINTERS`
- `OFFENSIVE_EXECUTION_1`
- `OFFENSIVE_EXECUTION_92`
- `_F{_SPECIAL_OFFENSIVE_PLAY_POINTERS`
- `TD_CELEBRATION_OFF_PLAY_POINTERS`
- `UNUSED_7_OFF_PLAY_POINTERS`
- `_F{_METATILE_DATA_POINTERS`
- `METATILE_DATA_POINTERS`
- `_F{_METATILE_DATA`
- `DOUBLE_DECKER_STADIUM_WITH_SCOREBOARD_METATILE_DATA`
- `DEFAULT_HELMET_SHELL_METATILE_DATA`
- `BENGALS_HELMET_SHELL_P2_MATCHUP_METATILE_DATA`
