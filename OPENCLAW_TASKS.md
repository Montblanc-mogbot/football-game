# OPENCLAW_TASKS.md

## Project
Football Game (fresh-start MonoGame conversion of Tecmo Super Bowl NES)

## Branch / remote
- Branch: `main`
- Remote: `origin`

## Reset rule
This repo starts fresh.
It should contain only the assembly snapshot and the planning/categorization work created on 2026-05-26.
Do not treat prior MonoGame implementation work as part of this repository.

## Source of truth
- Original disassembly snapshot: `reference/Tecmo_Super_Bowl_NES_Disassembly/`
- Planning docs:
  - `docs/planning/conversion-inventory.md`
  - `docs/planning/bank-parity-status-matrix.md`
  - `docs/planning/critical-bank-conversion-packets.md`

## Active tasks
- [ ] Decide the default reuse policy for the older MonoGame repo. Acceptance: one short note states whether this fresh repo should initially avoid reuse, selectively import packet-by-packet, or allow broader reuse only with explicit parity validation.
- [ ] Turn the planning docs into a first clean restart sequence. Acceptance: one short task note orders the first banks/packets to attack from scratch.
- [ ] Choose the first bounded conversion packet to implement in this repo. Acceptance: the task names the source bank, exact responsibility, expected evidence, and whether any old code may be consulted or imported.
