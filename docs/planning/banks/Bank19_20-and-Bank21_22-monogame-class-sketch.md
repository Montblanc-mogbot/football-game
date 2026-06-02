# Bank19_20 and Bank21_22 — MonoGame class sketch checked against coding standards

Updated: 2026-06-02

## Purpose

This note checks the emerging runtime design against `docs/coding-standards.md` before locking in architecture too early.

The immediate question is:

- if Bank19_20 is the on-field host and Bank21_22 is the per-player command runtime,
- what would that roughly look like in MonoGame,
- and does that shape fit the project standards?

## Short answer

Yes — the rough design is compatible with the coding standards **if** we keep three boundaries explicit:

1. **source-faithful script assets**
2. **decoded semantic model**
3. **runtime-consumption model**

That rule is directly reinforced by the standards.

The standards also push us away from:
- giant convenience classes
- premature flattening of script semantics
- broad architecture before the source-bank responsibility is understood
- mixing decoded data with runtime state

So the likely MonoGame shape should be **layered and explicit**, not inheritance-heavy and not over-abstracted.

## What the coding standards say that matters here

The most relevant architectural guardrails from `docs/coding-standards.md` are:

### 1. Separate decoded source data from runtime behavior
The standards explicitly say to keep:
- decoded source data
- runtime systems that consume it
- rendering/presentation concerns
- platform/plumbing

...clearly separated.

### Implication here
That strongly supports:
- Bank5_6 extracted data staying as data assets
- Bank21_22-style execution living in runtime classes
- Bank19_20-style orchestration living in host/controller classes

It argues against one giant `PlayLogicManager` that owns everything.

## 2. Preserve source-bank responsibility boundaries
The standards repeatedly emphasize:
- keep code traceable to the source bank responsibility
- do not introduce architecture before the source is understood
- structure the code so boundaries remain visible

### Implication here
That means the rewrite should preserve the distinction between:
- **script assignment/reassignment**
- **script execution**

So if Bank19_20 and Bank21_22 are separate source responsibilities, they should probably become separate modern runtime responsibilities too.

## 3. Keep the three-layer pipeline visible
The standards explicitly recommend separating:
1. raw/source-faithful extracted structure
2. decoded semantic model
3. runtime-consumption model

### Implication here
This is probably the single most important rule for Bank5_6/19_20/21_22.

It means we should not go directly from:
- extracted Bank5_6 JSON

to:
- ad hoc gameplay objects that blur assets, execution, and state.

## 4. Prefer explicit code over clever code
The standards prefer:
- explicitness
- small verifiable slices
- short methods grouped by responsibility
- guard clauses and loud failures on bad assumptions

### Implication here
This argues for:
- explicit `PlayerScriptRunner`
- explicit `OnFieldPlayCoordinator`
- explicit instruction handlers or instruction execution services
- explicit runtime context objects

It argues against:
- deep inheritance trees
- opaque coroutine magic without clear state ownership
- giant switchboards that also own field orchestration and asset lookup

## Architectural conclusion from the standards

The coding standards do **not** block the Bank19_20 / Bank21_22 split.

They actually support it, provided we keep it in a clear layered form.

The safest high-level shape is:

- **Bank5_6 asset layer**
- **Bank19_20 host/coordinator layer**
- **Bank21_22 per-player execution layer**
- shared gameplay-state/services underneath those runtime layers

## Recommended pattern mix

This does not need one pure Gang-of-Four pattern name.

It looks more like a practical combination:

### Interpreter
Because Bank21_22 is fundamentally interpreting script instructions.

### State machine
Because:
- a play has host-level state transitions
- a player command can span multiple frames
- execution resumes over time

### Strategy/handler dispatch
Because different instructions have different semantics and should not all live in one giant method forever.

### Coordinator / application-service style orchestration
Because Bank19_20 is really orchestrating on-field flow and script assignment rather than representing a single gameplay entity.

## The rough MonoGame class picture

Below is the rough class set I would expect from the Bank19_20 + Bank21_22 split.

## Asset / decoded model side

These are not the runtime classes yet.
They are the data/semantic layers beneath runtime consumption.

### `PlayScriptBankAsset`
Represents the full Bank5_6 script corpus.

Owns:
- offense and defense reaction collections
- shared player-slot vocabulary
- lookup by reaction id / label

### `ReactionScriptAsset`
Represents one reaction script.

Owns:
- reaction id
- offense/defense kind
- ordered instructions
- labels
- control-flow edges

### `ScriptInstruction`
Represents one decoded instruction.

Owns:
- instruction kind/opcode identity
- typed operands
- optional jump/branch target metadata

### `ScriptLabel`
Represents one script label.

### `ScriptJumpTarget`
Represents a resolved label target.

These types should stay mostly immutable and runtime-neutral.

## Bank19_20-shaped runtime classes

These are the classes that best match the on-field host/orchestration layer.

### `OnFieldPlayCoordinator`
This is the closest single class to the Bank19_20 role.

Owns responsibilities such as:
- starting a live play phase
- setting possession and play context
- selecting formation/play/script pointer families
- assigning initial reaction scripts to players
- reassigning scripts during turnovers, returns, and special-teams transitions
- deciding when play-level state transitions occur

This class should **not** decode individual Bank5_6 instructions.

### `PlayAssignmentService`
A narrower helper owned by or used by `OnFieldPlayCoordinator`.

Owns:
- mapping play call + possession context into assigned reaction scripts
- assigning initial script cursors to players
- bulk reassignment for events like interception return or punt return

This is a likely modern home for the logic that Bank19_20 expresses through pointer-table loads and `UPDATE_ALL_*_PLAY_CODE_ADRR` routines.

### `PlayPhaseState`
A host-level state model for the live play phase.

Could include:
- `PreSnap`
- `LiveBall`
- `KickInFlight`
- `Return`
- `TurnoverTransition`
- `PlayOver`

This is not the same thing as individual script instructions.
It is the play-host state above them.

### `OnFieldGameContext`
A runtime context object owned by the coordinator layer.

Holds references to:
- field state
- possession state
- ball state
- player collection
- control state
- current play call / formation context

This is the modern equivalent of the live game state Bank19_20 keeps coordinating around.

## Bank21_22-shaped runtime classes

These are the classes that best match the per-player command runtime.

### `PlayerScriptRunner`
This is the key Bank21_22-shaped class.

Owns:
- current script cursor for one player
- active in-progress instruction state
- stepping the current instruction
- advancing to next instruction when complete
- handling jumps/branches

This is the clearest modern equivalent of `DO_NEXT_PLAYER_COMMAND` plus the player's live execution state.

### `ScriptCursor`
Represents where a player currently is in a reaction script.

Owns:
- current reaction id
- current instruction index
- maybe current label target metadata for debugging/validation

This is the modern replacement for raw `PLAY_CODE_ADDR` as a gameplay-facing concept.

### `ActiveInstructionState`
Represents multi-frame execution state for the current instruction.

Possible contents:
- instruction start frame/time
- wait timers
- target-player references
- temporary command-local values
- any continuation metadata needed to resume

This lets commands span frames without turning the whole runtime into a soup of flags.

### `InstructionExecutionContext`
A service/context bundle passed into instruction execution.

Likely references:
- current player
- other players/query helpers
- ball state
- field/LOS/hash state
- control ownership state
- play status flags
- audio/animation hooks if needed

This keeps instruction handlers from directly depending on one giant global object.

### `ScriptInstructionDispatcher`
Maps instruction kinds to the matching execution handler.

Could be implemented with:
- a registry of handlers
- a switch for the first slice
- later promoted to typed handlers if the instruction surface gets large

This should stay local to the execution layer, not the play-host layer.

### `IScriptInstructionHandler`
Optional but likely helpful.

Rough shape:

```csharp
public interface IScriptInstructionHandler
{
    InstructionStatus StartOrResume(
        ScriptInstruction instruction,
        InstructionExecutionContext context,
        ActiveInstructionState? activeState,
        float deltaTimeSeconds);
}
```

Whether this becomes a formal interface or a simpler dispatch service can stay open for now.
The important part is the **responsibility split**, not the exact type mechanism.

## Shared gameplay-state classes beneath both layers

Both Bank19_20-style orchestration and Bank21_22-style execution will need shared state/services.

Likely classes include:
- `PlayerState`
- `BallState`
- `FieldState`
- `PossessionState`
- `ControlState`
- `PlayStatusState`

These should not be Bank-specific classes.
They are domain state used by both layers.

## How the classes fit together

A rough runtime flow in MonoGame would look like this:

### Start of play
`OnFieldPlayCoordinator`
- decides the play context
- asks `PlayAssignmentService` for initial reaction-script assignments
- gives each player a `ScriptCursor`
- resets/starts each player's `PlayerScriptRunner`

### Per-frame update
For each player:
- `PlayerScriptRunner.Tick(...)`
- fetch instruction from `ReactionScriptAsset`
- `ScriptInstructionDispatcher` finds the right handler
- handler reads/writes through `InstructionExecutionContext`
- if complete, runner advances cursor
- if not complete, runner keeps `ActiveInstructionState` and resumes next frame

### Mid-play reassignment
If the host layer detects interception / punt return / turnover / special-teams transition:
- `OnFieldPlayCoordinator` asks `PlayAssignmentService` for the new script family
- runners receive updated cursors/scripts
- execution continues from the new reaction assignments

That is the modern equivalent of the Bank19_20 + Bank21_22 relationship.

## Patterns to avoid

The coding standards make me pretty confident we should avoid these shapes:

### 1. Giant inheritance tree of play behaviors
Examples of what to avoid:
- `QuarterbackPitchLeftBehavior : BasePlayBehavior`
- `KickReturnBehavior : BasePlayBehavior`
- dozens or hundreds of per-reaction subclasses

Why avoid it:
- loses source traceability
- explodes class count for script content that should remain data-driven
- mixes asset definition with runtime behavior

### 2. One giant `PlayEngine` class
A single class that:
- assigns scripts
- decodes instructions
- owns play host state
- mutates every player
- handles ball logic
- handles special teams

...would violate the standards' responsibility and explicitness guidance pretty quickly.

### 3. Over-abstracted ECS-first guesswork
An ECS could maybe host parts of this later, but using it as the first architecture decision here would be premature.

The standards explicitly warn against introducing architecture before the source responsibility is understood.

## Recommended first concrete runtime-facing types

If I had to name the first modern classes to sketch without overcommitting, I would start with:

- `OnFieldPlayCoordinator`
- `PlayAssignmentService`
- `PlayerScriptRunner`
- `ScriptCursor`
- `ActiveInstructionState`
- `InstructionExecutionContext`
- `ScriptInstructionDispatcher`
- `PlayScriptBankAsset`
- `ReactionScriptAsset`
- `ScriptInstruction`

That is enough to express the Bank19_20 / Bank21_22 split clearly.

## Current recommendation

We are **not** violating the coding standards by thinking in terms of:
- a coordinator for Bank19_20-like responsibilities
- a per-player interpreter/runtime for Bank21_22-like responsibilities
- handlers/dispatch for instruction semantics

In fact, that shape is probably the safest way to satisfy the standards because it:
- preserves source-bank responsibility boundaries
- keeps data separate from runtime behavior
- avoids giant monoliths
- keeps the runtime explicit and traceable
- supports small verifiable slices

So the next useful step is not to finalize every class signature, but to treat this as the likely **responsibility map** for the eventual MonoGame runtime.
