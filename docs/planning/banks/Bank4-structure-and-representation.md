# Bank4 — structure and representation

Updated: 2026-05-27

## Purpose

This note describes the full-bank structure of `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank4_def_spec_play_pointers_data.asm` and sets the representation rules for converting it into maintainable artifacts without losing parity-critical structure.

## Top-level bank organization

`Bank4_def_spec_play_pointers_data.asm` is organized as two major layers in order:

1. **Defensive execution pointer tables**
   - `_F{_DEFENSE_PLAY_POINTERS`
   - `DEFENSIVE_EXECUTION_1` through `DEFENSIVE_EXECUTION_255`
   - 255 defensive execution tables, each with exactly 11 ordered reaction pointers

2. **Special defense-play pointer tables**
   - `_F{_DEFENSE_SPECIAL_PLAY_POINTERS`
   - `DEFENSIVE_PLAYERS_CRY_PLAY_POINTERS` through `UNUSED_7_DEFENSE_SPECIAL_PLAY_POINTERS`
   - 16 special tables, each with exactly 12 ordered reaction pointers

## What is NES/assembly-driven vs what is real game structure

### Mostly assembly/NES representation details
- ROM pointer tables instead of typed references
- contiguous table packing in bank order
- label-driven organization rather than explicit typed records

### Real game/domain structure that must survive conversion
- canonical defensive execution table order
- canonical player-reaction ordering inside each 11-entry defensive table
- distinction between normal defensive execution tables and special defense-play tables
- canonical 12-entry ordering inside each special table

## Structural invariants that must remain intact

The conversion must preserve:
- the 255-table canonical order of `DEFENSIVE_EXECUTION_1` through `DEFENSIVE_EXECUTION_255`
- the fixed 11-entry width of normal defensive execution tables
- the 16-table canonical order of the special defense-play block
- the fixed 12-entry width of special defense-play tables
- exact reaction-label ordering within each table

## Representation split

### 1. Source-faithful extracted layer
This layer should preserve the source structure as directly as practical.

For Bank4, that means:
- exact defensive execution table ordering
- exact reaction labels in source order for all 255 execution tables
- exact special defense-play table ordering
- exact reaction labels in source order for all 16 special tables

### 2. Decoded semantic layer
This layer removes pointer mechanics while preserving structure and meaning.

For Bank4, that means typed concepts like:
- `DefensiveReactionPointer`
- `DefensiveExecutionRecord`
- `SpecialDefensePlayRecord`
- `DefensePlayTableSet`

This layer is intentionally aligned with earlier work:
- **Bank1_2** supplies the canonical roster-slot and team vocabulary that later consumers will use when these reaction pointers are mapped to actual player identities and ratings.
- **Bank3** already established the offensive-side formation/execution table pattern, so Bank4 mirrors that structure on the defensive side instead of inventing a different abstraction shape.

### 3. Runtime-consumption layer
This should stay separate and come later.

Examples:
- mapping defensive table entries onto canonical Bank1_2 roster-slot identities
- joining Bank4 pointer families to later Bank5/21 behavior-command semantics
- choosing the correct defensive table from playcall/game-state code

## What not to preserve mechanically in C#

We should **not** preserve raw ROM pointer chasing as the runtime API.

Do preserve:
- canonical ordering
- source label identity
- fixed table widths
- exact reaction-label sequencing

Do not preserve:
- byte-pointer traversal as the gameplay-facing model
- ROM-local packing assumptions once the semantic layer exists

## Exact source anchors

Use these anchors when validating the conversion:
- `_F{_DEFENSE_PLAY_POINTERS`
- `DEFENSE_PLAY_POINTERS`
- `DEFENSIVE_EXECUTION_1`
- `DEFENSIVE_EXECUTION_255`
- `_F{_DEFENSE_SPECIAL_PLAY_POINTERS`
- `DEFENSE_SPECIAL_PLAY_POINTERS`
- `DEFENSIVE_PLAYERS_CRY_PLAY_POINTERS`
- `UNUSED_7_DEFENSE_SPECIAL_PLAY_POINTERS`
