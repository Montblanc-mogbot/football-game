# Packet 5A — offensive script model extraction

## Packet scope
- **Packet id:** `5A`
- **Source bank:** `Bank5_6_off_def_play_data.asm`
- **Primary category:** behavioral reimplementation through explicit data semantics
- **Old-code policy:** did **not** consult the older MonoGame repo

## Bounded offensive family selected
I modeled `OFFENSE_PLAYER_REACTION_091` as a first explicit offensive play family.

Why this family:
- it stays small enough for a first packet
- it clearly reads as a distinct backfield sequence
- it exposes several important offensive-script semantics without forcing a full command vocabulary yet:
  - snap style
  - ordered ball-placement deltas
  - fake handoff target
  - pitch target
  - jump/exit continuation

## Source slice used
Exact source label:
- `OFFENSE_PLAYER_REACTION_091` at `x834a` through `x8359`

Assembly excerpt summary:
1. `takeSnapUnderCenter`
2. move ball placement by `$F8, $E0`
3. move ball placement by `$F8, $DC`
4. `fakeHandoffToPlayer PLAYER_COMMAND_DATA_RB2`
5. move ball placement by `$F8, $DA`
6. `pitchToPlayer PLAYER_COMMAND_DATA_RB1`
7. move ball placement by `$F0, $D0`
8. `jumpTo OFFENSE_PLAYER_REACTION_314`

## Modern artifact added
- `src/FootballGame/Conversion/PlayScripts/OffensivePitchPlayFamily.cs`

The model deliberately stays narrow:
- `OffensivePitchPlayFamily`
- `SnapStyle`
- `BallPlacementStep`

This is not yet a general decoder or full play-command system.
It is only a packet-sized data model for one offensive family.

## Proposed instance mapping
A future decoder/fixture for this family would map the source like this:

- `FamilyId` → `PitchFakeToRb2ThenPitchRb1`
- `SourceLabel` → `OFFENSE_PLAYER_REACTION_091`
- `SnapStyle` → `UnderCenter`
- `FakeHandoffTarget` → `PLAYER_COMMAND_DATA_RB2`
- `PitchTarget` → `PLAYER_COMMAND_DATA_RB1`
- `ExitReactionLabel` → `OFFENSE_PLAYER_REACTION_314`
- `BallPlacementSteps` → ordered list preserving the four source movement commands:
  1. `x834b`: vertical `-8`, horizontal `-32` (`$F8, $E0`)
  2. `x834e`: vertical `-8`, horizontal `-36` (`$F8, $DC`)
  3. `x8352`: vertical `-8`, horizontal `-38` (`$F8, $DA`)
  4. `x8356`: vertical `-16`, horizontal `-48` (`$F0, $D0`)

## Mapping back to source-bank structure
This packet leaves the following traceability decisions explicit:

- `OFFENSE_PLAYER_REACTION_091` is treated as a single offensive-family script entry.
- The family is represented as an **ordered sequence** rather than flattened properties only, because the ball-placement commands occur between possession events and their order materially matters.
- `fakeHandoffToPlayer` and `pitchToPlayer` are preserved as separate semantics instead of being collapsed into a generic "ball transfer" field.
- The terminal `jumpTo` is kept as `ExitReactionLabel` because the source-bank structure clearly chains reactions by label rather than ending as a self-contained play object.

## Unknowns / deferred follow-up
Deferred to later packets:
- exact runtime meaning of `PLAYER_COMMAND_DATA_RB1` and `PLAYER_COMMAND_DATA_RB2` symbol-to-roster-slot mapping
- a shared command vocabulary for all offensive/defensive scripts (`5C`)
- whether ball-placement deltas should later be normalized into field-space units rather than preserved as raw script bytes
- the exact behavior of `OFFENSE_PLAYER_REACTION_314`, which is outside this packet's bounded scope

## Validation notes
Validation for this packet is source-to-model traceability rather than runtime execution.

Checked manually:
- the selected source label is present in `Bank5_6_off_def_play_data.asm`
- the C# model contains explicit fields for every non-trivial semantic in `OFFENSE_PLAYER_REACTION_091`
- the ordered ball-placement list preserves all four movement instructions and their original byte values as signed deltas
- the packet scope remains bounded to one offensive family and one modern data artifact

## Assumptions changed for the next packet
This packet suggests a useful next-step assumption for `5C`:
- the eventual shared command vocabulary should likely distinguish between:
  - stance/snap commands
  - ordered positional deltas
  - possession-transfer events
  - reaction chaining/jumps
