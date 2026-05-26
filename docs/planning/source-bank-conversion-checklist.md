# Source-bank conversion checklist

Updated: 2026-05-26

Use this checklist before starting and before closing any conversion packet.

## Before starting a packet
- Identify the **packet id** and **source bank** explicitly.
- Read the relevant source `.asm` section directly.
- Re-read these repo docs:
  - `docs/coding-standards.md`
  - `docs/planning/conversion-inventory.md`
  - `docs/planning/critical-bank-conversion-packets.md`
  - the packet-specific planning note, if one already exists
- State whether old MonoGame code may be consulted.
- Decide whether the task is primarily:
  - data semantics
  - gameplay behavior
  - rendering/presentation
  - platform/plumbing replacement

## While implementing
- Keep the code traceable to the named bank responsibility.
- Prefer explicit, small data models and helper types.
- Do not introduce broad architecture before the source responsibility is understood.
- Separate decoded source data from runtime behavior that consumes it.
- Before choosing JSON/YAML/or another format, identify which source structures must remain intact: ordering, slot counts, fixed-width rows, pointer families, packed record shapes, or grouped tables.
- Do not reshape assembly-derived data into a convenience format if that loses meaningful source structure.
- Leave comments only where they add source intent, invariants, or parity reasoning.
- Use names that are explicit and C#-idiomatic.
- Follow `docs/coding-standards.md` for file/type layout and naming.

## Before marking a packet complete
- Confirm the artifact is still bounded to the packet scope.
- Write down the exact source labels/tables/routines used.
- Record unknowns, assumptions, and deferred follow-up items.
- Leave validation evidence:
  - tests, or
  - comparison notes, or
  - a source-to-model mapping note
- State whether the packet changed any assumptions for the next packet.

## If reuse from old code is proposed
- Stop and make it explicit.
- Justify the reuse against the named source-bank responsibility.
- Add or update a task/note documenting the reuse decision before importing code.
