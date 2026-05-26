# Coding Standards

Updated: 2026-05-26

These standards combine:
- MonoGame's C# style guidance
- the Doom 3 coding-style discipline, adapted for modern C#

When the two sources conflict, this project prefers **idiomatic C# / MonoGame conventions first**, then borrows Doom-style rules where they improve clarity, consistency, and maintainability.

## Goals

- Keep the codebase readable under steady long-term iteration
- Make parity-first conversion work easy to review against the source banks
- Prefer explicitness over cleverness
- Keep data semantics and gameplay behavior easy to trace

## Core style rules

### Indentation and whitespace
- Use **4 spaces** for indentation.
- Do **not** use tabs.
- Put a single space after commas.
- Do not put spaces just inside parentheses, method-call parentheses, or index brackets.
- Use a single space around binary operators.
- Do not put a space between a unary operator and its operand.

### Braces and control flow
- Use **Allman braces** in normal C# style.
- Braces open on the next line after the statement.
- `else`, `catch`, and `finally` begin on a new line.
- Single-line control-flow bodies may omit braces only when the body is truly trivial and the result is still obviously readable.
- If either branch of an `if`/`else` uses braces, prefer braces for both.

Example:

```csharp
if (isEligible)
{
    RunPlay();
}
else
{
    ShowBlockedReason();
}
```

## Naming

### General naming
- Use **PascalCase** for classes, structs, records, enums, methods, properties, and events.
- Use **camelCase** for parameters and local variables.
- Use **_camelCase** for private fields.
- Prefix interfaces with `I`.
- Do not use Hungarian notation.
- Avoid abbreviations unless they are domain-standard (`Cpu`, `Nes`, `Ppu`, `Apu`, `Rom`).

### Project prefix rule
The Doom style used an `id` company prefix for many class names. We do **not** apply that mechanically in C#.

For this project:
- use **plain PascalCase** by default for most types
- reserve **`Fb`** for project-wide engine/game abstractions where a project prefix materially helps clarity

Good examples:
- `TeamData`
- `PlayCommand`
- `FieldState`
- `FbGame`
- `FbSpriteBatchExtensions`
- `FbContentIds`

Avoid pointless prefixing like:
- `FbTeamData`
- `FbQuarter`
- `FbYardLine`

### Method naming
- Use verbs or verb phrases for methods.
- Prefer explicit names over overloaded ambiguity.
- If two operations differ semantically, name them differently instead of relying on overloads.

Prefer:
- `GetAnimByIndex()`
- `GetAnimByName()`
- `TryGetRosterEntry()`

Over:
- `GetAnim(int index)`
- `GetAnim(string name)`

### Enum naming
- Use PascalCase for enum type names.
- Use PascalCase for enum members.
- Do not prefix every enum member with the enum name.

Example:

```csharp
public enum PlayPhase
{
    PreSnap,
    LiveBall,
    DeadBall,
}
```

## File and type organization

- One public type per file.
- The file name should match the public type name.
- Group related small internal helper types in the same file only when that clearly improves locality.
- Organize folders to reflect the code's conceptual area, not temporary implementation history.
- Keep data-model types, runtime systems, rendering helpers, and source-bank reference tooling clearly separated.

Suggested broad structure:
- `Data/`
- `Gameplay/`
- `Rendering/`
- `Conversion/`
- `Content/`
- `Diagnostics/`

## Class layout

Within a class, prefer this order:
1. constants
2. static fields/properties
3. private fields
4. constructors
5. public properties
6. public methods
7. protected methods
8. private methods
8. nested types

Keep the public surface near the top.
Group members logically.
Use `#region` sparingly and only when it genuinely improves navigation in long files.

## Comments and documentation

- Comments should explain **intent, source behavior, invariants, or non-obvious decisions**.
- Do not write comments that just restate the code.
- Prefer comments above the code, not trailing comments.
- Public types and public methods should have XML docs when their contract is not completely obvious.
- Internal/private members do not need boilerplate docs; use focused comments where they add real value.

For conversion work, leave high-value source references like:

```csharp
// Source: Bank5_6_off_def_play_data.asm
// Mirrors the original pointer-table ordering for offensive play families.
```

## Conversion traceability rules

Because this is a parity-first conversion project:
- important gameplay/data code should be traceable to source-bank responsibilities
- avoid "magic cleanup" refactors that hide the original semantics too early
- when introducing a modern abstraction, document what original responsibility it replaces
- keep source-bank references in comments, docs, or adjacent notes when they materially help parity review

## C# API and language guidance

### Nullability
- Treat nullable annotations as part of the contract.
- Do not add `?` just to silence warnings.
- Keep runtime guard clauses where they improve failures or enforce invariants.
- Use the null-forgiving operator `!` sparingly.

### Immutability and state
- Prefer `readonly` fields when practical.
- Prefer immutable data objects for decoded source data unless mutation is part of the model.
- Keep mutable gameplay state explicit and localized.
- Avoid hidden cross-system mutation.

### Const/readonly adaptation from Doom guidance
Doom emphasizes `const` discipline. In C#, adapt that as:
- use `const` only for true compile-time constants
- use `readonly` for stable fields after construction
- mark methods as non-mutating by design and keep side effects narrow
- prefer read-only interfaces/views where mutation is not intended

### Overloading
- Avoid overloads that differ only by parameter type when that makes call sites ambiguous.
- Overloads are fine when they are conventional and obvious.
- Prefer `Try...` methods for lookup/parse patterns that can fail without exceptional behavior.

### Floating-point literals
- Use explicit `f` suffixes for `float` literals.

Example:

```csharp
float movementScale = 0.5f;
float snapWindowSeconds = 1.0f;
```

## Data and behavior boundaries

- Keep decoded source data separate from runtime systems that consume it.
- Avoid mixing rendering concerns into gameplay/state types.
- Avoid mixing MonoGame plumbing into source-data decoding layers.
- When porting a bank, identify whether the code belongs primarily to:
  - data semantics
  - gameplay behavior
  - rendering/presentation
  - platform/plumbing replacement

Structure the code so those boundaries remain visible.

## Source-data representation policy

For assembly-derived data, prefer **structure preservation first** over convenience-first reshaping.

- Do not flatten or normalize source data just because JSON/YAML makes it tempting.
- Preserve original grouping, ordering, fixed-width tables, slot counts, pointer families, and position-specific record shapes whenever those structures carry meaning.
- Treat the original assembly layout as a semantic contract, not just raw bytes to be reformatted.
- If JSON or YAML can express the source structure cleanly without hiding that contract, it is acceptable.
- If JSON or YAML would force awkward reshaping, loss of ordering meaning, or premature abstraction, use a different representation.
- Prefer a representation that makes source-to-artifact comparison easy during parity review.
- Separate three layers clearly when possible:
  1. raw/source-faithful extracted structure
  2. decoded semantic model
  3. runtime-consumption model
- Do not skip directly from assembly bytes to a convenience runtime model when the intermediate semantic structure is important.
- For behavior-touching banks, document what structural properties must remain stable before choosing a serialization format.

Practical rule: the data format serves the source structure — the source structure does not get bent to fit the data format.

## Error handling

- Fail loudly on invalid internal assumptions.
- Use guard clauses early.
- Do not swallow exceptions without a specific recovery reason.
- Prefer `Try...` APIs for expected lookup failure.
- Prefer explicit validation errors over silent fallback when decoding source-derived data.

## Testing and verification mindset

- Favor small, verifiable slices.
- Each conversion packet should leave behind evidence: tests, data snapshots, comparison notes, or runnable validation.
- Prefer deterministic helpers for data decoding and behavior rules where possible.
- When a behavior is source-sensitive, document how it was verified.

## Practical maintainability rules

- Prefer explicit code over clever compact code.
- Keep methods short enough to scan comfortably.
- Split large methods by responsibility, not just by line count.
- Do not introduce architecture layers before the source-bank responsibility is understood.
- Reuse old code only when a task explicitly authorizes it and parity review justifies it.

## Default tie-breakers

If a style question comes up and this document does not answer it:
1. prefer normal modern C#/.NET conventions
2. prefer MonoGame-style consistency
3. prefer the more explicit/readable option
4. prefer the option that keeps source-bank intent easiest to trace
