# Bank19_20 conversion validation

Updated: 2026-06-02

## Scope

This note validates the current Bank19_20 full-bank conversion artifacts for `Bank19_20_on_field_gameplay_loop.asm`.

## Artifacts covered

- `docs/planning/banks/Bank19_20-structure-and-representation.md`
- `docs/planning/banks/Bank19_20-inventory-and-responsibility-map.md`
- `development-tools/bank19_20/extract_bank19_20.py`
- `content/game-data/on-field/generated/bank19_20-section-map.json`
- `content/game-data/bank19_20/generated/summary.json`
- `src/FootballGame/Conversion/OnField/*.cs`
- `docs/planning/banks/Bank19_20-loader-layer.md`
- `docs/planning/banks/Bank21_22-architecture-review.md` (carry-forward bridge additions)

## Validation checks

### Section inventory
Checked against:
- every `_F{...}` block in `Bank19_20_on_field_gameplay_loop.asm`

Validated:
- the extractor records every `_F{...}` section in source order, including nested recovery sub-sections
- each extracted section preserves start/end markers and source line span
- each extracted section preserves its global labels
- the generated summary reports `sectionCount = 75`

### Entry points and pointer-family preamble
Checked against:
- `BANK_JUMP_ON_FIELD_GAMEPLAY_START`
- `BANK_JUMP_SKP_VS_SKP_INJURY_START`
- the special play-pointer-family constants near the top of the bank

Validated:
- both explicit bank entrypoints are preserved in the generated artifact
- the generated summary reports `entryPointCount = 2`
- the special pointer-family constants used for interception/fumble/punt/onside/cheer/cry/chase contexts are preserved with source addresses and side/purpose tags
- the generated summary reports `scriptPointerFamilyCount = 19`

### Modern ownership mapping
Checked against:
- the section-by-section ownership analysis in `docs/planning/banks/Bank19_20-inventory-and-responsibility-map.md`

Validated:
- each section is classified as either controller-owned or supporting-service-owned
- responsibility groups remain explicit instead of flattening the whole bank into one coordinator class

### Loader/semantic bridge
Checked against:
- `content/game-data/on-field/generated/bank19_20-section-map.json`
- `src/FootballGame/Conversion/OnField/Bank19OnFieldGameplayInventoryJsonLoader.cs`
- `docs/planning/banks/Bank19_20-loader-layer.md`

Validated:
- the typed semantic layer now includes the `externalJumpConstants` slice from the generated artifact
- the typed semantic layer preserves section labels as records with source lines rather than flattening them away
- the loader maps JSON string ownership/responsibility values into explicit enums and returns one `Bank19OnFieldGameplayInventory` aggregate for later runtime-facing work

### Runtime-facing coordinator/service coverage
Checked against:
- `src/FootballGame/Gameplay/OnField/Bank19RuntimeRepresentation.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/Services/*.cs`
- `src/FootballGame/Gameplay/OnField/Bank21Bridge/Bank19ToBank21BoundaryHoldingArea.cs`
- `docs/planning/banks/Bank19_20-runtime-representation.md`

Validated:
- every extracted Bank19_20 section is assigned to either the coordinator or one Bank19_20 service in `Bank19RuntimeRepresentation`
- the runtime-facing classes expose covered-section lists so the section ownership remains source-traceable
- the four Bank19_20-to-Bank21_22 bridge sections are mirrored into an explicit holding area for later command-runtime work

### Bank21_22 carry-forward boundary
Checked against representative Bank19_20 sections that prime or hand off to Bank21_22:
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `DEFENDER_CHANGE_BEFORE_HIKE`
- `CHECK_SNAP_PUNT`
- `SET_PLAYERS_CLOSE_TO_PASS`

Validated:
- each of the above sections is tagged for carry-forward into later Bank21_22 work
- the generated summary reports `carryForwardToBank21_22SectionCount = 4`
- the Bank21_22 architecture note now explicitly names these Bank19_20-originated bridge areas so they do not get lost when that bank conversion resumes

## Important non-goals of this pass

This pass does **not** yet provide:
- a finished MonoGame gameplay runtime implementation for the full on-field loop
- a final `OnFieldPlayCoordinator` class implementation
- a Bank21_22 command interpreter implementation
- a packet-level Bank19_20 runtime slice such as `19A`, `19B`, or `19C`

This is still a conversion/inventory/representation pass, but it is now complete enough to keep the whole Bank19_20 content represented and reviewable.

## Outcome

Bank19_20 now has a reviewable full-bank conversion layer that preserves:
- bank entrypoints
- special script-pointer families
- section-level structure
- controller/service boundaries
- explicit cross-bank dependencies
- Bank21_22 carry-forward bridge points

That is the right source-faithful representation for this bank at the current architecture stage.
