# OPENCLAW_TASKS.md

## Project
Football Game (fresh-start MonoGame conversion of Tecmo Super Bowl NES)

## Branch / remote
- Branch: `main`
- Remote: `origin`

## Reset rule
This repo starts fresh.
It contains only:
- the assembly snapshot under `reference/`
- the assembly-first planning/categorization notes created on 2026-05-26

Do not treat prior MonoGame implementation work as part of this repository.
If old code is ever consulted or imported from a separate repo, that must be an explicit future decision.

## Source of truth
- Original disassembly snapshot: `reference/Tecmo_Super_Bowl_NES_Disassembly/`
- Planning docs:
  - `docs/planning/conversion-inventory.md`
  - `docs/planning/bank-parity-status-matrix.md`
  - `docs/planning/critical-bank-conversion-packets.md`
  - `docs/planning/source-bank-conversion-checklist.md`
- Coding standards:
  - `docs/coding-standards.md`

## Active tasks
- [x] Decide the default policy for consulting or reusing code from the old MonoGame repo. Acceptance: one short note states whether the default is no reuse, selective packet-by-packet reuse, or broader reuse only with explicit source-bank justification.
  - Evidence: added `docs/planning/old-code-reuse-policy.md`, which sets the default to **no reuse by default** and requires any later reuse to be justified packet-by-packet against a named source-bank responsibility.
- [x] Turn the assembly-first planning docs into a clean restart sequence. Acceptance: one short durable note orders the first banks/packets to tackle from scratch.
  - Evidence: added `docs/planning/restart-sequence.md`, which orders the fresh-start conversion through a Phase 1 gameplay-parity backbone (`Bank1_2`, `5A`, `5C`, `21A`, `17A`, `19A`, `12A`) before widening into playcall, post-play, stats, collision, and later meta-state slices.
- [x] Choose the first bounded conversion packet for actual implementation. Acceptance: the task names the packet id, the source bank, the expected output/artifact, and whether old code may be consulted.
  - Evidence: added `docs/planning/first-conversion-packet.md`, which selects packet `5A` from `Bank5_6_off_def_play_data.asm`, defines the expected packet-analysis note + initial modern data artifact + validation note, and explicitly says old MonoGame code should **not** be consulted for this first packet.

- [ ] Implement packet `5A` from `Bank5_6_off_def_play_data.asm`. Acceptance: create `docs/planning/packets/5A-offensive-script-model.md`, add one small explicit C# data model representing one offensive play family, and leave validation notes showing the mapping back to the source-bank structure. Old-code policy: do **not** consult the older MonoGame repo.
- [ ] Capture `Bank1_2_team_data.asm` data semantics needed by packet `5A`. Acceptance: leave one durable note naming the exact team/player data structures or enums that `5A` depends on, with any unresolved semantics called out explicitly.

## Notes
- This task file intentionally avoids carrying forward old completed tasks, old validation campaigns, or old architecture milestones.
- Future tasks should be source-bank-first, not old-code-first.
- All implementation/refactor work should follow `docs/coding-standards.md` and the packet flow in `docs/planning/source-bank-conversion-checklist.md`.
