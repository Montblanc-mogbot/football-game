# Bank1_2 — structure and representation

Updated: 2026-05-26

## Purpose

This note describes the full-bank structure of `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank1_2_team_data.asm` and sets the representation rules for converting it into maintainable C# artifacts without losing parity-critical structure.

## Top-level bank organization

`Bank1_2_team_data.asm` is organized as four major layers in order:

1. **Team-order pointer table**
   - `_F{TEAM_PLAYER_NAMES_TEAM_PTR_TABLE`
   - `STARTING_ADDR_FOR_TEAM_PLAYER_NAMES_PTR_TABLE`
   - 28 teams in fixed canonical order

2. **Per-team roster-slot pointer lists**
   - `_F{_PLAYER_NAME_POINTERS`
   - labels such as `BUFFALO_LIST`, `INDIANAPOLIS_LIST`, ..., `ATLANTA_LIST`
   - each team list contains exactly 30 player pointers in a fixed slot order

3. **Player identity records**
   - `_F{_PLAYER_NUMBERS_AND_NAMES`
   - records such as `BUFFALO_QB1`, `INDIANAPOLIS_RB1`, etc.
   - each record stores a jersey byte plus an exact source-name payload
   - `PLAYER_LIST_END` marks the end of the identity-record block

4. **Per-team packed ability data**
   - `_F{_PLAYER_ABILITIES`
   - begins with the `.ENUM $00` slot-layout offsets
   - defines the 16-step nibble attribute scale (`ATTRIBUTE_6` .. `ATTRIBUTE_100`)
   - defines packing macros (`ADD_NIBBLES_AS_BYTE`, `ADD_FACE_IDENTIFIER`)
   - stores one aligned ability blob per team (`BUFFALO_BILLS_ABILITIES` .. `ATLANTA_FALCONS_ABILITIES`)

## What is NES/assembly-driven vs what is real game structure

### Mostly assembly/NES representation details
- pointers instead of typed references
- contiguous ROM layout
- packed nibble fields inside bytes
- hand-authored offset layout via `.ENUM`
- macros used to emulate typed records

### Real game/domain structure that must survive conversion
- canonical team order
- canonical 30-slot roster order
- stable alignment between team roster order and team ability order
- position-group-specific ability schemas
- exact source-name payloads and placeholder QB conventions
- the 16-step attribute grade scale

## Structural invariants that must remain intact

The conversion must preserve:
- the **28-team canonical order** from `STARTING_ADDR_FOR_TEAM_PLAYER_NAMES_PTR_TABLE`
- the **30-slot canonical roster order** inside every team list
- the distinction between:
  - team table
  - team roster-slot lists
  - player identity records
  - team ability blobs
- exact per-position ability widths:
  - QB = 5 bytes
  - RB/WR/TE/defenders/K/P = 4 bytes
  - OL = 3 bytes
- the nibble-grade semantics of ability values
- exact source-name payloads without lossy normalization
- placeholder/team-QB records where they exist

## Representation split

### 1. Source-faithful extracted layer
This layer should preserve the source structure as directly as practical.

For Bank1_2, that means:
- canonical team order
- exact roster-slot order per team
- exact player labels
- jersey byte values
- exact source-name payloads
- per-team ability blobs in slot order
- raw nibble values and face bytes

This layer should be reviewable against the assembly without needing to mentally reconstruct the original layout.

### 2. Decoded semantic layer
This layer should remove pointer mechanics while preserving structure and meaning.

For Bank1_2, that means typed concepts like:
- `TeamId`
- `RosterSlot`
- `PlayerIdentityRecord`
- `TeamRosterRecord`
- `QuarterbackAbilityRecord`
- `SkillPositionAbilityRecord`
- `OffensiveLineAbilityRecord`
- `DefenderAbilityRecord`
- `KickerAbilityRecord`
- `PunterAbilityRecord`
- `AttributeGrade`

This layer should be the main dependency for later Bank3/Bank4/Bank5_6 work.

### 3. Runtime-consumption layer
This should stay separate and come later.

Examples:
- gameplay-facing roster selection
- team-loading/runtime lookup helpers
- ability queries used by play execution

Do not force runtime concerns back into the source-faithful layer.

## What not to preserve mechanically in C#

We should **not** preserve raw pointer management as an implementation technique.

Do preserve:
- pointer-table meaning
- ordering semantics
- slot identity
- aligned table families

Do not preserve:
- manual pointer chasing
- raw ROM-like navigation as the main domain API
- convenience-free byte-centric runtime code once the semantic layer exists

## Format guidance

JSON or YAML are acceptable only if they preserve the bank’s meaningful structure cleanly.

For Bank1_2 specifically:
- ordered collections are mandatory
- per-slot identity must remain explicit
- role-specific ability schemas must not be flattened away into one generic record
- raw source-name payloads should remain byte-faithful strings
- raw nibble grades should remain available for parity validation

If a format makes the bank look simpler than it really is, it is the wrong format.

## Pitfalls to avoid

- flattening rosters into unordered dictionaries
- normalizing names into guessed display fields too early
- erasing placeholder QB records
- collapsing all ability records into one generic stat bag
- decoding nibble grades only into integers and discarding source ordinals
- mixing source extraction and runtime/gameplay behavior in one model

## Exact source anchors

Use these anchors when validating the conversion:
- `_F{TEAM_PLAYER_NAMES_TEAM_PTR_TABLE`
- `STARTING_ADDR_FOR_TEAM_PLAYER_NAMES_PTR_TABLE`
- `_F{_PLAYER_NAME_POINTERS`
- `BUFFALO_LIST` through `ATLANTA_LIST`
- `_F{_PLAYER_NUMBERS_AND_NAMES`
- `PLAYER_LIST_END`
- `_F{_PLAYER_ABILITIES`
- `.ENUM $00`
- `ATTRIBUTE_6` through `ATTRIBUTE_100`
- `ADD_NIBBLES_AS_BYTE`
- `ADD_FACE_IDENTIFIER`
- `BUFFALO_BILLS_ABILITIES` through `ATLANTA_FALCONS_ABILITIES`
