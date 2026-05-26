# Bank1_2 conversion validation

Updated: 2026-05-26

## Scope

This note validates the first full-bank conversion artifacts for `Bank1_2_team_data.asm`.

## Artifacts covered

- `docs/planning/banks/Bank1_2-structure-and-representation.md`
- `content/reference/bank12/team-roster-ordering.yaml`
- `content/reference/bank12/ability-layout.yaml`
- `src/FootballGame/Conversion/Bank12/Models/*.cs`

## Validation checks

### Team-order and roster-order structure
Checked against:
- `STARTING_ADDR_FOR_TEAM_PLAYER_NAMES_PTR_TABLE`
- `BUFFALO_LIST`
- representative later team list labels through `ATLANTA_LIST`

Validated:
- 28-team canonical order is preserved in the extracted roster artifact
- 30-slot canonical roster order is preserved explicitly, not implied
- slot names remain stable and reviewable

### Player identity semantics
Checked against:
- `_F{_PLAYER_NUMBERS_AND_NAMES`
- representative labels such as `BUFFALO_QB1`, `INDIANAPOLIS_RB1`, `NEW_YORK_JETS_QB1`

Validated:
- jersey byte remains explicit
- source-name payload remains exact, not normalized
- source label remains explicit
- placeholder-QB semantics are not flattened away

### Ability-layer semantics
Checked against:
- `_F{_PLAYER_ABILITIES`
- `.ENUM $00`
- `ATTRIBUTE_6` through `ATTRIBUTE_100`
- `BUFFALO_BILLS_ABILITIES`

Validated:
- per-position widths remain explicit
- nibble-grade scale remains explicit
- face byte remains separate from packed nibble fields
- role-specific tail fields remain distinct in the decoded model

## Important non-goals of this pass

This pass does **not** yet provide:
- a full automatic parser for the assembly file
- runtime loading/gameplay integration
- full behavior-side consumers of the ability data
- display-name normalization beyond preserving source payloads

## Outcome

The first Bank1_2 conversion pass preserves the bank’s meaningful structure while removing pointer mechanics from the semantic C# layer.
