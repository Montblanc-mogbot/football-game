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
- `src/FootballGame/Gameplay/OnField/OnFieldRoutineOwnershipMap.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/Services/*.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
- `docs/planning/banks/Bank19_20-runtime-representation.md`

Validated:
- every extracted Bank19_20 routine is assigned to either the coordinator or one Bank19_20 service in `OnFieldRoutineOwnershipMap`
- the runtime-facing classes expose covered-routine lists so the ownership remains source-traceable
- the four Bank19_20-to-Bank21_22 bridge routines are mirrored into an explicit holding area for later command-runtime work

### First coordinator logic slice
Checked against:
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldGameState.cs`
- `src/FootballGame/Gameplay/OnField/Services/PlayAssignmentService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PlayerSkillHydrationService.cs`
- `src/FootballGame/Gameplay/OnField/Services/TaskCoordinationService.cs`
- `src/FootballGame/Gameplay/OnField/Services/OnFieldPresentationService.cs`
- `src/FootballGame/Gameplay/OnField/Services/CpuPlayDecisionService.cs`

Validated:
- the coordinator now contains real host logic for `GAME_PLAY_START_CHECK_FOR_KICK_TEAM`, `P1_KICKOFF`, `P2_KICKOFF`, the P1/P2 play-select/load entry paths, the immediate transition into regular-play vs special-teams host routing, and the first play-over/possession-change outcome paths
- kickoff setup now makes explicit service calls for play assignment, skill hydration, task startup, presentation setup, and CPU kickoff strategy evaluation
- regular-play setup now makes explicit service calls for pre-snap control and presentation setup before entering run/pass host flow
- play-over resolution now makes explicit service calls for stat accounting and injury/cutscene handling before re-entering play selection or kickoff flow
- pass-play progression now makes explicit service calls for pass-target indicator updates, pass-collision ordering, incomplete-pass presentation, cutscene resets, and interception-return script reassignment
- special-teams progression now makes explicit service calls for special-teams skill overrides, punt coverage/return script reassignment, punt-return presentation, field-goal/XP presentation, and blocked-kick presentation
- turnover/score progression now makes explicit coordinator/service calls for interception-return assignment, touchdown celebration/presentation, onside recovery handling, loose-ball recovery routing, turnover series reset, and post-turnover spot/hash updates
- kickoff progression is now wired into the live coordinator loop with explicit kickoff-state reset, kickoff/onside routing, and normalized punt/interception possession-change handling instead of leaving those newer host methods partly disconnected
- kickoff/punt return routing now waits for dead-ball resolution rather than ending immediately on catch, and extra-point made/missed/blocked outcomes now correctly exit into kickoff routing instead of dead-ending the host flow
- blocked field-goal routing now enters an explicit loose-ball / recovery path instead of stopping at a placeholder event, and onside return touchdowns are classified as special-teams returns instead of generic defensive returns
- dead-ball transition teardown is now less fragmented: next-sequence dispatch flows through a shared finalization step that ends Bank19_20 tasks, records quarter-over checks, clears transient dead-ball flags, and then routes into play-select or kickoff setup
- the naming of the runtime-facing ownership map and routine ids has been cleaned up into gameplay-facing names instead of generic `Bank19...Section...` names

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
- a complete `OnFieldPlayCoordinator` implementation for every Bank19_20 path
- a Bank21_22 command interpreter implementation
- a packet-level Bank19_20 runtime slice such as `19A`, `19B`, or `19C`

This is still an incremental conversion/representation pass, but it now includes a real first coordinator logic slice in addition to the inventory, loader, and ownership map layers.

## Outcome

Bank19_20 now has a reviewable full-bank conversion layer that preserves:
- bank entrypoints
- special script-pointer families
- section-level structure
- controller/service boundaries
- explicit cross-bank dependencies
- Bank21_22 carry-forward bridge points

That is the right source-faithful representation for this bank at the current architecture stage.
