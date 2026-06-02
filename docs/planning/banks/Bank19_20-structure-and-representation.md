# Bank19_20 — structure and representation

Updated: 2026-06-02

## Purpose

This note defines the full-bank conversion shape for `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm`.

Unlike `Bank1_2`, `Bank3`, or `Bank4`, this bank is not primarily table data.
It is a large **behavior/orchestration bank** that mixes:

- on-field host/controller flow
- play/script assignment and retargeting helpers
- pre-snap control logic
- play-outcome adjudication
- special-teams transitions
- pass-targeting helpers
- stats/injury/cutscene/presentation support
- explicit cross-bank handoffs into Bank17_18, Bank21_22, Bank23, and Bank27

So the conversion has to preserve the bank's **section structure, ownership boundaries, and cross-bank handoff points**, not just extract a data table.

## Source anchors covered

This conversion slice is grounded in:

- the top-level bank entrypoints:
  - `BANK_JUMP_ON_FIELD_GAMEPLAY_START`
  - `BANK_JUMP_SKP_VS_SKP_INJURY_START`
- the special play-pointer family constants near the top of the bank
- every `_F{ ... }` / `_F} ...` section in the source file, including the nested fumble-recovery subsections
- the Bank21_22 bridge constants used by Bank19_20:
  - `JUMP_DO_NEXT_PLAYER_COMMAND`
  - `JUMP_WR_JUMP_DIVE_CHECK_PASS`
  - `JUMP_DEF_JUMP_DIVE_CHECK_PASS`

## Representation layers

### 1. Source-faithful extracted layer
Artifacts:
- `development-tools/bank19_20/extract_bank19_20.py`
- `content/game-data/on-field/generated/bank19_20-section-map.json`
- `content/game-data/bank19_20/generated/summary.json`

This layer preserves:
- the two explicit bank entrypoints
- the named special script-pointer families and their addresses
- every `_F{...}` section, including nested recovery sub-sections
- each section's start/end source span
- global labels found inside each section
- modern ownership classification per section
- explicit cross-bank dependency hits per section
- explicit "carry forward into Bank21_22 notes" tags where Bank19_20 primes or hands off to the command runtime

This is the Bank19_20 equivalent of the earlier source-faithful bank artifacts.
The important preserved shape here is **behavioral structure**, not byte-table layout.

### 2. Decoded semantic model layer
C# types:
- `src/FootballGame/Conversion/OnField/Bank19OnFieldGameplayInventory.cs`
- `src/FootballGame/Conversion/OnField/Bank19EntryPointRecord.cs`
- `src/FootballGame/Conversion/OnField/Bank19ScriptPointerFamilyRecord.cs`
- `src/FootballGame/Conversion/OnField/Bank19ExternalJumpConstantRecord.cs`
- `src/FootballGame/Conversion/OnField/Bank19CrossBankDependencyRecord.cs`
- `src/FootballGame/Conversion/OnField/Bank19SectionRecord.cs`
- `src/FootballGame/Conversion/OnField/Bank19SectionLabelRecord.cs`
- `src/FootballGame/Conversion/OnField/Bank19ModernOwner.cs`
- `src/FootballGame/Conversion/OnField/Bank19ResponsibilityGroup.cs`
- `src/FootballGame/Conversion/OnField/Bank19OnFieldGameplayInventoryJsonLoader.cs`
- `docs/planning/banks/Bank19_20-loader-layer.md`

These types intentionally model the bank as a **conversion inventory / responsibility map**, not as final gameplay runtime classes.

That is important because this bank is still being used to define architecture boundaries.
We do not want to flatten the bank straight into a guessed final MonoGame runtime.

### 3. Runtime-consumption layer
This pass now includes a first explicit runtime-facing representation for the full bank:
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/Services/*.cs`
- `src/FootballGame/Gameplay/OnField/Bank19RuntimeRepresentation.cs`
- `src/FootballGame/Gameplay/OnField/Bank21Bridge/Bank19ToBank21BoundaryHoldingArea.cs`
- `docs/planning/banks/Bank19_20-runtime-representation.md`

The current runtime split is:
- `OnFieldPlayCoordinator` for the Bank19_20 host/orchestration role
- dedicated services for script assignment, pre-snap control, pass targeting, play outcomes, stats, injury, task coordination, CPU support, player-skill hydration, and presentation
- `Bank19ToBank21BoundaryHoldingArea` for the explicit Bank19_20-to-Bank21_22 carry-forward bridge
- `PlayerScriptRunner` still deferred to the later Bank21_22 command-runtime pass

## What this conversion preserves on purpose

### Bank-scale structure
The bank remains visible as:
- entrypoints
- special pointer families
- top-level sections
- grouped responsibility families

### Controller vs service boundary
The conversion explicitly records which sections are best treated as:
- **controller** material
- **supporting service** material

That lets us represent all of Bank19_20 content without collapsing it into one giant coordinator class.

### Cross-bank handoff boundaries
This pass keeps the important handoffs explicit instead of hiding them:

- **Bank21_22**
  - per-player command runtime re-entry
  - pass jump/dive contest handlers
- **Bank23**
  - field draw, banner, collision/presentation task entrypoints
- **Bank27**
  - player skill loading
- **Bank17_18**
  - injury/scoreboard/game-loop re-entry points

### Script-pointer families
The top-level special pointer families are preserved because they are the clearest source-level evidence that Bank19_20 owns script assignment/reassignment during:
- interceptions
- fumble recovery
- punt return / coverage
- onside recovery
- cheers / cry / chase-ball-carrier outcomes

## What this conversion does not preserve literally

This pass does **not** treat the following as primary modern representations:

- MMC3 bank-swap mechanics
- raw stack/task tricks as architectural concepts
- exact zero-page temp-variable layout
- RTS/jump-table implementation style
- raw numeric jump-entry addresses as the main semantic representation

Those can still be documented for parity review, but they are not the shape the MonoGame code should mirror.

## Bank21_22 carry-forward set

Some Bank19_20 content should stay represented here **and** be explicitly carried into later Bank21_22 work because it creates the bridge between host flow and command runtime.

This pass marks these sections for carry-forward:

- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
  - seeds `COMMAND_COUNTER`
  - seeds `JUMP_DO_NEXT_PLAYER_COMMAND`
  - bulk assigns player script cursors
- `DEFENDER_CHANGE_BEFORE_HIKE`
  - primes the active player for snap-time control handoff
  - seeds command return state after snap
- `CHECK_SNAP_PUNT`
  - keeps punt snap gating separate from the interpreter while still defining the handoff moment
- `SET_PLAYERS_CLOSE_TO_PASS`
  - primes `JUMP_WR_JUMP_DIVE_CHECK_PASS`
  - primes `JUMP_DEF_JUMP_DIVE_CHECK_PASS`

These are not moved out of Bank19_20.
They are tagged so the Bank21_22 conversion does not miss the boundary behavior that Bank19_20 depends on.

## Practical result

Bank19_20 now has the same broad conversion ingredients as the earlier bank passes:

- durable bank note
- extractor
- generated source-faithful artifacts
- typed semantic model layer
- validation note

The difference is that for this bank the "source-faithful artifact" is a **section/responsibility/cross-bank map**, because that is the real semantic structure this bank contributes.

## Deferred follow-up

The most useful next sub-slices remain:

1. a tighter script-pointer/reassignment matrix for Bank19_20
2. a pre-snap control deep dive
3. later, the paired Bank21_22 runtime conversion that consumes the Bank19_20 carry-forward set
