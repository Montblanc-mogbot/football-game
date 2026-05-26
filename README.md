# Football Game

Fresh-start MonoGame conversion workspace for **Tecmo Super Bowl NES**.

This repository contains:
- a local snapshot of the original disassembly
- planning/categorization work for the bank-by-bank conversion
- converted game-facing data/code as it is carried into the new game
- separate development tools used to extract or reshape source data

It does **not** carry forward the prior MonoGame implementation.
That older work remains in its separate repository/history.

## Source of truth
- Original disassembly snapshot: `reference/Tecmo_Super_Bowl_NES_Disassembly/`

## Main repo areas
- `src/FootballGame/` — game-facing converted code
- `content/game-data/` — game-facing converted data artifacts
- `development-tools/` — extraction/conversion tooling, intentionally kept separate from the game
- `docs/planning/` — conversion notes, validation, and bank-by-bank planning

## Planning docs
- `docs/planning/conversion-inventory.md`
- `docs/planning/bank-parity-status-matrix.md`
- `docs/planning/critical-bank-conversion-packets.md`
