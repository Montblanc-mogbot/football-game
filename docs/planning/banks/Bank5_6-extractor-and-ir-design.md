# Bank5_6 — extractor and intermediate-representation design

Updated: 2026-06-02

## Purpose

This note defines the next practical step for converting `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank5_6_off_def_play_data.asm`:

- how to extract the bank into source-faithful artifacts
- what the intermediate representation should preserve
- what should be decoded semantically
- what runtime-owned state should **not** leak into the extracted asset model

This is deliberately a design note, not an implementation note.

## Design goal

The Bank5_6 conversion should produce artifacts that are:
- traceable back to the disassembly
- complete enough to represent the full script graph
- explicit enough to validate jumps/loops/targets
- decoupled from ROM-pointer mechanics
- still neutral about the final MonoGame execution architecture

The extractor should not try to solve gameplay runtime design in the same step.

## Proposed conversion pipeline

### Stage 1: source parsing
Input:
- `Bank5_6_off_def_play_data.asm`
- `macros/play_data_macros.asm`

Output:
- a parsed bank model with:
  - top-level offense/defense section boundaries
  - reaction entry labels
  - local labels
  - ordered macro/instruction lines under each label
  - raw macro arguments as written in source

This stage should be as literal as practical.

### Stage 2: source-faithful script extraction
Output:
- generated JSON artifact(s) describing the full script corpus
- one generated summary artifact for counts/invariants

This stage should resolve enough structure to answer:
- what are the reaction entry points?
- what commands belong to each reaction script?
- what local labels exist inside each reaction script?
- which commands jump or branch to which labels?

### Stage 3: semantic instruction decoding
Output:
- typed semantic models in C# or a companion decoded JSON layer

This stage should map raw opcode/macro families into named semantic instructions while preserving:
- exact command identity
- operand meaning
- explicit control-flow targets
- offense/defense script-bank split

### Stage 4: runtime-consumption layer
Not part of the extractor itself.

This later layer can decide whether the gameplay runtime executes the scripts via:
- an interpreter
- coroutine-style command runners
- compiled command objects
- another equivalent MonoGame-friendly model

The extractor should feed this layer, not bake it in.

## Proposed generated artifacts

### 1. Source-faithful master artifact
Suggested path:
- `content/game-data/play-scripts/generated/bank5_6-play-scripts.json`

Suggested top-level shape:
- `sourceFile`
- `offenseReactions`
- `defenseReactions`
- `sharedPlayerSlotVocabulary`
- `commandSetVersion` or equivalent extractor metadata

Each reaction record should include:
- `reactionId` (example: `OFFENSE_PLAYER_REACTION_091`)
- `kind` (`offense` / `defense`)
- `sourceOrder`
- `entryLabel`
- `labels`
- `instructions`
- `controlFlowEdges`

### 2. Summary artifact
Suggested path:
- `content/game-data/bank5_6/generated/summary.json`

Suggested summary contents:
- offense reaction count
- defense reaction count
- total local label count
- total instructions by command family
- count of branch/jump instructions
- count of cross-player-targeting instructions
- count of CPU-conditional instructions
- count of special-teams instructions

This gives a quick parity snapshot similar to the earlier bank summaries.

## Reaction-script extraction shape

A reaction should be treated as the bounded unit of extraction.

Suggested shape:

```json
{
  "reactionId": "OFFENSE_PLAYER_REACTION_091",
  "kind": "offense",
  "sourceOrder": 91,
  "entryLabel": "OFFENSE_PLAYER_REACTION_091",
  "labels": [
    {
      "label": "OFFENSE_PLAYER_REACTION_091",
      "kind": "entry",
      "instructionIndex": 0
    },
    {
      "label": "@loop",
      "kind": "local",
      "instructionIndex": 5
    }
  ],
  "instructions": [
    {
      "index": 0,
      "sourceMacro": "takeSnapUnderCenter",
      "family": "single",
      "opcode": "D4",
      "rawArgs": []
    }
  ],
  "controlFlowEdges": [
    {
      "fromInstructionIndex": 8,
      "kind": "jump",
      "targetLabel": "@loop"
    }
  ]
}
```

The exact JSON does not matter yet. The structural obligations do.

## Labels and control flow

This is the heart of the Bank5_6 extraction.

### What must be preserved
- entry labels for every reaction
- local labels inside reactions
- whether a control-flow instruction is:
  - unconditional jump
  - conditional COM/CPU jump
  - random jump
  - branch with relative/local target
- exact target label identity
- exact target reaction when a target crosses a reaction boundary, if that occurs

### What should not remain the primary representation
- raw ROM addresses
- implicit control flow only derivable from byte offsets
- consumer-facing dependence on opcode-length arithmetic

### Recommended rule
For extracted artifacts, **labels are the canonical jump targets**.

If the extractor also records raw source addresses for validation, that is fine as auxiliary metadata, but labels should drive the consumer-facing model.

## Instruction model: source-faithful layer

Each instruction in the extracted artifact should keep enough detail to round-trip source meaning.

Suggested fields:
- `index`
- `sourceMacro`
- `family` (`group` / `single`)
- `opcode`
- `rawArgs`
- `resolvedArgs`
- `targetLabel` when applicable
- `notes` only when a source oddity must be called out

### Why both raw and resolved args?
Because some commands mix:
- nibble-packed player slots
- timing bytes
- coordinates
- jump labels
- probability/juice thresholds

Keeping raw arguments plus resolved typed meaning will make validation and future debugging much easier.

## Semantic intermediate representation

On top of the extracted layer, the decoded IR should normalize meaning without flattening behavior.

Suggested semantic types:
- `PlayScriptBankAsset`
- `ReactionScriptAsset`
- `ReactionScriptKind`
- `ScriptInstruction`
- `ScriptLabel`
- `ScriptEdge`
- `PlayerSlotReference`
- `ScriptJumpTarget`
- `ControlModeCondition`
- `CpuBoostThreshold`
- `InstructionTimingWindow`

Suggested instruction categories:
- snap / ball-exchange
- movement / formation positioning
- block / coverage / pursuit
- pass-decision and target-order registration
- control transfer / man-vs-CPU logic
- collision/block-permission mutation
- celebration / post-play reactions
- special-teams actions
- control-flow

### Important rule
The semantic layer should preserve **instruction identity**, not just effect category.

Example:
- `takeSnapUnderCenter`
- `takeSnapFromShotgun`
- `takeSnapForFGXP`

These are related, but they should not be collapsed into one generic "receive snap" instruction unless variant data remains explicit.

## Player-slot and target references

The extractor should carry the shared slot vocabulary as data, not as scattered comments.

Suggested approach:
- emit one shared vocabulary table in the generated artifact
- store every slot-targeting operand both as:
  - raw nibble value
  - resolved symbolic slot id

Example:
- raw: `0x02`
- symbolic offense meaning: `RB2`
- symbolic defense alias: `LE`

This matters because later consumers may need either the offensive vocabulary, the defensive alias, or a neutral internal slot enum.

## Runtime-owned state that should stay out of extracted assets

This boundary matters a lot.

The extracted Bank5_6 asset should **not** contain runtime state such as:
- current player RAM addresses
- command counters
- per-frame wait counters in live execution state
- current ball position, velocity, or animation task state
- current pass target selection result
- current man-controlled player pointer
- current collision flags
- current possession state
- current CPU boost value
- current match clock / quarter state

Those belong to the runtime/execution context, not the script asset.

The extracted asset may reference those concepts semantically, but should not embed live state slots for them.

## Cross-player behavior boundaries

Some commands in Bank5_6 clearly affect another player:
- handoff / fake handoff
- pitch
- target-player redirection
- some coverage/block behaviors
- commands that rewrite another player's next command address when valid

The extractor should preserve **that a command targets another player slot or another player's control-flow path**.

But it should not attempt to precompute the live runtime consequences.

Recommended extraction rule:
- preserve targeted slot / target label / target-instruction metadata
- leave actual player-object lookup and state mutation to the runtime layer

## Suggested validation invariants

The extractor should validate at least:
- offense and defense reaction counts match the source review
- every instruction belongs to exactly one reaction script
- every label used as a jump/branch target resolves successfully
- no instruction index is skipped within a reaction
- group vs single command decoding matches the documented opcode families
- all slot-targeting operands resolve through the shared vocabulary

Helpful parity metrics to record:
- total offense reactions
- total defense reactions
- total labels
- total instructions
- total branch/jump edges
- total commands by opcode/family

## What the extractor does not need to decide yet

It does **not** need to decide:
- final gameplay object model
- whether commands become classes, coroutines, or tables at runtime
- whether a future VM uses one step per frame or cached continuations
- how Bank19/21 runtime services are finally carved into MonoGame subsystems

Those are downstream architecture decisions.

## Naming direction

To stay aligned with the earlier bank work, prefer:
- source-faithful names in generated artifacts
- C#-idiomatic names in the semantic model
- explicit use of `ReactionScript`, `Instruction`, `Label`, `JumpTarget`, and `PlayerSlot`

Avoid making the extracted layer sound more abstract than it is.
It is still an assembly-derived script corpus.

## Recommended next implementation slice

The safest first implementation slice is:
1. build a parser/extractor that emits only the source-faithful JSON plus summary
2. validate reaction counts, labels, and control-flow targets
3. add the semantic IR types after the JSON extraction is stable
4. defer runtime execution code until the extracted graph is trustworthy

That keeps the work bank-by-bank and prevents the Bank5_6 runtime from being guessed into existence too early.

## Source anchors for this design

Primary source anchors:
- `OFFENSE_PLAY_DATA:`
- `DEFENSE_PLAY_DATA:`
- representative entries like `OFFENSE_PLAYER_REACTION_091` and `DEFENSE_PLAYER_REACTION_001`
- `DO_NEXT_PLAYER_COMMAND` in `Bank21_22_play_commands_on_field_logic.asm`
- `GROUP_COMMAND_TABLE`
- `SINGLE_COMMAND_TABLE`
- `UPDATE_LOCAL_PLAYER_COMMAND_ADDR_IF_VALID`
- `JUMP_COMMAND_START`
- `BRANCH_COMMAND_START`
- `DO_ACTION_IF_COM_COMMAND_START`
- `COM_JUMP_BASED_ON_JUICE_COMMAND_START`
- `macros/play_data_macros.asm`

## Current recommendation

Treat Bank5_6 extraction as a **script-graph extraction problem** first.

If we preserve:
- reaction identities
- ordered instructions
- label graph
- typed operands
- cross-player targeting metadata

...then we will have a strong foundation for both validation and later runtime design without dragging NES pointer mechanics into the consumer model. kupo.
