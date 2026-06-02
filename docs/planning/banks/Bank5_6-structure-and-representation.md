# Bank5_6 — structure and representation

Updated: 2026-06-02

## Purpose

This note describes the full-bank structure of `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank5_6_off_def_play_data.asm` and sets the representation rules for converting it into maintainable artifacts without losing parity-critical behavior.

Unlike `Bank1_2`, `Bank3`, and `Bank4`, this bank is not primarily tables of identities, pointers, or layouts. It is a large corpus of **player behavior scripts** for offense and defense.

## Top-level bank organization

`Bank5_6_off_def_play_data.asm` is organized as two major sections in order:

1. **Offensive play-data scripts**
   - `OFFENSE_PLAY_DATA:`
   - many `OFFENSE_PLAYER_REACTION_xxx` labeled script entry points
   - local jump labels and loop labels embedded through the section

2. **Defensive play-data scripts**
   - `DEFENSE_PLAY_DATA:`
   - many `DEFENSE_PLAYER_REACTION_xxx` labeled script entry points
   - local jump labels and loop labels embedded through the section

This is a script graph, not a set of fixed-width pointer tables.

## What is NES/assembly-driven vs what is real game structure

### Mostly assembly/NES representation details
- banked ROM access and cross-bank dispatch
- raw script addresses stored in per-player RAM
- bytecode packing optimized around opcode ranges and nibble arguments
- assembly trampolines and manual command-length bookkeeping
- local labels expressed as ROM addresses instead of symbolic graph nodes

### Real game/domain structure that must survive conversion
- the distinction between offensive and defensive reaction-script families
- the canonical set of reaction entry points and their source ordering
- the command stream contained by each reaction script
- the control-flow graph formed by jumps and loops within the bank
- the shared player-slot vocabulary used by many commands
- multi-frame blocking behavior such as waiting for snap, pass timing, handoff/pitch exchange, and special-teams timing
- commands that coordinate more than one actor, such as handoffs, pitches, post-catch transitions, and pass-target registration
- control-mode-sensitive behavior such as manual-vs-CPU branches and CPU-boost/juice branches

## Structural invariants that must remain intact

The conversion must preserve:
- the top-level split between `OFFENSE_PLAY_DATA` and `DEFENSE_PLAY_DATA`
- the canonical set of offensive reaction entry labels
- the canonical set of defensive reaction entry labels
- the exact source-ordered command sequence inside each reaction block
- exact jump/loop targets and branch structure within the bank
- the opcode-level distinction between command families where that distinction affects semantics
- the shared 0x00-0x0A player-slot/nibble vocabulary used by targeted commands
- the fact that some commands are cross-player and may redirect another player's next behavior step
- the fact that some commands are blocking/multi-frame rather than one-shot mutations

## Important interpretation rule

This bank should be treated as a **behavior-script source bank**, not as a bag of passive tables.

The runtime behavior traced so far shows:
- another gameplay bank maintains per-player execution state
- each player carries a current script pointer/program counter in RAM
- a dispatcher decodes the command at that location
- handlers may mutate player state, ball state, control state, timing state, and even another player's command state

So the real structure to preserve is:
- script entry points
- instruction sequences
- branch graph
- command semantics
- cross-frame execution behavior

## Representation split

### 1. Source-faithful extracted layer
This layer should preserve the source structure as directly as practical.

For Bank5_6, that means:
- a complete catalog of offensive reaction-script entry points in source order
- a complete catalog of defensive reaction-script entry points in source order
- each reaction script represented as an ordered sequence of source-faithful commands
- symbolic preservation of jump/loop targets between commands and labels
- exact command arguments, including:
  - targeted player-slot nibble values
  - timing bytes
  - movement coordinates
  - branch/jump destinations
  - control-mode and CPU-boost thresholds
- a stable representation of the shared player-slot vocabulary used by the scripts

This extracted layer should remain close enough to the disassembly that we can validate individual reaction scripts and targets against source labels without reintroducing raw ROM traversal as the consumer API.

### 2. Decoded semantic layer
This layer removes NES pointer mechanics while preserving meaning.

For Bank5_6, that likely means typed concepts such as:
- `PlayScriptBank`
- `ReactionScript`
- `ReactionScriptKind` (`Offense` / `Defense`)
- `ScriptCommand`
- `ScriptJumpTarget`
- `PlayerSlotReference`
- `PassTargetPriority`
- `SnapVariant`
- `ControlModeCondition`
- `CpuBoostCondition`
- command-specific records such as:
  - `HandoffCommand`
  - `PitchCommand`
  - `PassTimingCommand`
  - `WaitForSnapCommand`
  - `MoveRelativeCommand`
  - `SetFormationPositionCommand`

This layer should preserve script semantics without forcing future consumers to know about ROM addresses, bank numbers, or opcode-length tables.

### 3. Runtime-consumption layer
This should stay separate and come later.

Examples of later runtime-consumption concerns:
- deciding whether Bank5_6 is executed by an interpreter, compiled state objects, coroutines, or another equivalent MonoGame-friendly behavior system
- integrating script commands with the main gameplay loop, player objects, ball state, collision systems, and animation systems
- deciding how cross-player command redirection should be modeled in the new runtime
- deciding whether some source-faithful commands should compile into richer runtime actions

This note intentionally does **not** lock in that final runtime architecture.

## Relationship to prior banks

Bank5_6 depends conceptually on the earlier converted banks but has a different shape.

- **Bank1_2** supplies the canonical roster-slot and team vocabulary that later consumers will map onto these script-level player-slot references.
- **Bank3** supplies offensive pointer-table structure that selects reaction scripts for offensive contexts.
- **Bank4** supplies defensive pointer-table structure that selects reaction scripts for defensive contexts and special cases.

In other words:
- Banks 3 and 4 tell the runtime **which reaction scripts to assign**
- Bank5_6 defines **what those assigned reactions actually do**

## What not to preserve mechanically in C#

We should **not** preserve raw command-pointer chasing as the gameplay-facing model.

Do preserve:
- reaction-entry identity
- script ordering
- jump/loop graph structure
- command semantics
- player-slot references
- timing and threshold arguments
- offense/defense separation
- multi-frame/blocking behavior as a semantic contract

Do not preserve:
- bank swapping as a consumer concern
- raw 16-bit ROM addresses as runtime-facing references
- opcode-length tables as the primary gameplay abstraction
- direct assembly-era command-counter plumbing in player RAM
- the requirement that commands be consumed only via address arithmetic

## Exact source anchors

Use these anchors when validating the conversion:
- `OFFENSE_PLAY_DATA:`
- `OFFENSE_PLAYER_REACTION_001`
- `OFFENSE_PLAYER_REACTION_091`
- `OFFENSE_PLAYER_REACTION_314`
- `OFFENSE_PLAYER_REACTION_446`
- `DEFENSE_PLAY_DATA:`
- `DEFENSE_PLAYER_REACTION_001`
- `DEFENSE_PLAYER_REACTION_255`
- `macros/play_data_macros.asm`
- `DO_NEXT_PLAYER_COMMAND` in `Bank21_22_play_commands_on_field_logic.asm`
- `LOAD_P1_DEFENSE_PLAY_CODE_ADDRESSES` / `LOAD_P2_DEFENSE_PLAY_CODE_ADDRESSES` and the play-code-address setup flow in `Bank19_20_on_field_gameplay_loop.asm`

## Conversion direction implied by this note

The safest full-bank conversion path for Bank5_6 is:
1. extract the complete source-faithful script graph
2. preserve command identity and arguments exactly
3. assign symbolic labels/targets instead of raw pointer mechanics in the semantic layer
4. defer the final runtime architecture decision until the main gameplay loop and broader object model are in place

That approach preserves parity-critical behavior while avoiding premature commitment to an assembly-shaped MonoGame design.
