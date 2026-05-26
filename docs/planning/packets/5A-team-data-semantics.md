# Packet 5A — team/player data semantics from `Bank1_2_team_data.asm`

## Packet scope
- **Packet id:** `5A`
- **Primary source bank for this note:** `Bank1_2_team_data.asm`
- **Dependent packet:** `5A` in `Bank5_6_off_def_play_data.asm`
- **Primary category:** data semantics
- **Old-code policy:** did **not** consult the older MonoGame repo

## Why packet 5A depends on Bank1_2 team data
The bounded `5A` family note for `OFFENSE_PLAYER_REACTION_091` names player-command targets rather than concrete roster rows:
- `PLAYER_COMMAND_DATA_RB2` at `x8351`
- `PLAYER_COMMAND_DATA_RB1` at `x8355`

Those symbols only become meaningful once they are tied to the offense-side player-slot ordering and the per-team roster/attribute data stored in `Bank1_2_team_data.asm`.

## Exact structures/enums packet 5A depends on

### 1. Player command slot enum from `macros/play_data_macros.asm`
Exact enum-like constants:
- `PLAYER_COMMAND_DATA_QB1 = $00`
- `PLAYER_COMMAND_DATA_RB1 = $01`
- `PLAYER_COMMAND_DATA_RB2 = $02`
- `PLAYER_COMMAND_DATA_WR1 = $03`
- `PLAYER_COMMAND_DATA_WR2 = $04`
- `PLAYER_COMMAND_DATA_TE1 = $05`
- `PLAYER_COMMAND_DATA_C = $06`
- `PLAYER_COMMAND_DATA_LG = $07`
- `PLAYER_COMMAND_DATA_RG = $08`
- `PLAYER_COMMAND_DATA_LT = $09`
- `PLAYER_COMMAND_DATA_RT = $0A`

Source traceability:
- `reference/Tecmo_Super_Bowl_NES_Disassembly/macros/play_data_macros.asm:3-13`

For packet `5A`, the relevant members are:
- `RB1` = offensive backfield slot index `$01`
- `RB2` = offensive backfield slot index `$02`

This is the direct semantic bridge between the play-script commands in bank `5_6` and the roster data in bank `1_2`.

### 2. Offensive roster slot order inside each team player-pointer list
Each team has a fixed player-pointer list whose early entries match the offensive slot names used by packet `5A`:
- `QB1`
- `QB2`
- `RB1`
- `RB2`
- `RB3`
- `RB4`
- `WR1`
- `WR2`
- `WR3`
- `WR4`
- `TE1`
- `TE2`
- `C`
- `LG`
- `RG`
- `LT`
- `RT`
- ...followed by defensive/special-teams entries

Source traceability:
- team list table root: `STARTING_ADDR_FOR_TEAM_PLAYER_NAMES_PTR_TABLE` at `Bank1_2_team_data.asm:35`
- first concrete layout example: `BUFFALO_LIST` at `Bank1_2_team_data.asm:47-76`

For packet `5A`, this means:
- `PLAYER_COMMAND_DATA_RB1` resolves to the team list's third offensive skill-position entry (`RB1`)
- `PLAYER_COMMAND_DATA_RB2` resolves to the next offensive skill-position entry (`RB2`)
- these are **slot references**, not hard-coded individual players across the whole game

### 3. Per-team player identity records for those slots
Each pointer-list entry resolves to a named player record. Example from Buffalo:
- `BUFFALO_QB1` at `Bank1_2_team_data.asm:949`
- `BUFFALO_RB1` at `Bank1_2_team_data.asm:951`
- `BUFFALO_RB2` at `Bank1_2_team_data.asm:952`

These records at minimum encode:
- jersey number byte
- display-name string payload

For packet `5A`, this establishes that `RB1` and `RB2` can map to different concrete players per team while the play script still targets the same abstract offensive slots.

### 4. Per-team ability blocks aligned to the same slot order
Each team also has an ability block with comments and byte groups ordered by roster slot:
- first concrete block: `BUFFALO_BILLS_ABILITIES` at `Bank1_2_team_data.asm:1891`
- next block begins at `INDIANAPOLIS_COLTS_ABILITIES` at `Bank1_2_team_data.asm:2040`

Within a team ability block, the relevant entries for packet `5A` are explicitly labeled:
- `QB1 Attributes`
- `RB1 Attributes`
- `RB2 Attributes`

For `RB1` and `RB2`, the current source comments identify these packed semantics:
- Rushing Power
- Running Speed
- Maximum Speed
- Hitting Power
- Face Identifier
- Ball Control
- Receptions

For `QB1`, the block similarly exposes the quarterback attributes that matter for `takeSnapUnderCenter` and any later QB-targeted transfer commands:
- Rushing Power
- Running Speed
- Maximum Speed
- Hitting Power
- Face Identifier
- Passing Speed
- Pass Control
- Accuracy of Passing
- Avoid Pass Block

## What packet 5A can safely assume from this dependency
Packet `5A` can safely model `FakeHandoffTarget` and `PitchTarget` as references to a compact offensive slot enum, not as direct player ids.

A parity-friendly modern interpretation is:
- play script chooses an offensive slot (`RB1`, `RB2`, etc.)
- active team data resolves that slot to the current team's player identity and ability row
- runtime behavior then uses the resolved player's ratings for movement/ball-control/catch-related consequences outside this note's bounded scope

## Unresolved semantics to keep explicit
- This note does **not** prove whether packet `5A` consumes only the slot identity, or also immediately consults `RB1`/`RB2` ratings during the fake-handoff/pitch sequence; that linkage likely lives in gameplay-execution code outside `Bank1_2_team_data.asm`.
- `Bank1_2_team_data.asm` clearly shows the roster slot order and labeled attribute groups, but this note does **not** yet decode the exact packed byte layout into final numeric attribute scales.
- `QB2`, `RB3`, and `RB4` exist in team data, but packet `5A` family `OFFENSE_PLAYER_REACTION_091` only depends directly on `QB1`, `RB1`, and `RB2`.
- Defensive aliases in `play_data_macros.asm` (`NT = RB1`, `LE = RB2`, etc.) show that the nibble values are formation-side slot enums reused across offense/defense roles; this note keeps its interpretation bounded to the offensive meaning required by packet `5A`.

## Validation / source-to-note mapping
Checked directly against source:
- `OFFENSE_PLAYER_REACTION_091` uses `PLAYER_COMMAND_DATA_RB2` and `PLAYER_COMMAND_DATA_RB1` in `Bank5_6_off_def_play_data.asm:474-476`
- slot constants come from `macros/play_data_macros.asm:3-13`
- team player-pointer ordering comes from `Bank1_2_team_data.asm:35` and `47-76`
- concrete player labels for the first team example come from `Bank1_2_team_data.asm:949-952`
- aligned ability-block labels come from `Bank1_2_team_data.asm:1891-1913`

## Packet-scope conclusion
Within packet `5A` scope, the exact `Bank1_2` data semantics dependency is:
- a shared offensive slot enum (`QB1`, `RB1`, `RB2`, ...)
- each team's roster pointer list in that same slot order
- each team's ability rows in that same slot order

That is enough to carry `5A`'s target-player references forward without guessing at unrelated team-data systems.
