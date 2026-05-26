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

## Active tasks
- [ ] Decide the default policy for consulting or reusing code from the old MonoGame repo. Acceptance: one short note states whether the default is no reuse, selective packet-by-packet reuse, or broader reuse only with explicit source-bank justification.
- [ ] Turn the assembly-first planning docs into a clean restart sequence. Acceptance: one short durable note orders the first banks/packets to tackle from scratch.
- [ ] Choose the first bounded conversion packet for actual implementation. Acceptance: the task names the packet id, the source bank, the expected output/artifact, and whether old code may be consulted.

## Notes
- This task file intentionally avoids carrying forward old completed tasks, old validation campaigns, or old architecture milestones.
- Future tasks should be source-bank-first, not old-code-first.
