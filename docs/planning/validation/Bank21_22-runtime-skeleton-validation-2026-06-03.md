# Bank21_22 runtime skeleton validation — 2026-06-03

## Scope

This validation covers the first production-facing Bank21_22 runtime skeleton added under `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/` for the task:

- `Implement the first Bank21_22 production-facing runtime skeleton`

The goal of this slice was **not** to wire live gameplay yet.
It was to add the smallest coherent `PlayerCommand*` execution-layer skeleton that keeps the Bank19_20 host/runtime seam explicit.

## Files added

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntimeBoundary.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandPointer.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandDefinition.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntimeHostRequest.cs`

## Files updated

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
- `OPENCLAW_TASKS.md`

## Direct source/code references behind the slice

### Bank21_22 player-command stepper anchor

Source note already captured in:
- `docs/planning/banks/Bank21_22-runtime-representation.md`

Primary source anchor:
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:240` (`DO_NEXT_PLAYER_COMMAND`)

Why it matters here:
- `PlayerCommandRuntime.StepPlayerCommand(...)` is intentionally shaped as a **single per-player step** rather than as a host-flow coordinator.
- `PlayerCommandExecutionContext` keeps one player's pointer/continuation ownership local, matching the documented Bank21_22 role as a per-player command runtime.

### Bank19_20 host/runtime bridge anchors kept explicit

Source notes already captured in:
- `docs/planning/banks/Bank21_22-runtime-representation.md`

Bridge routines:
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `DEFENDER_CHANGE_BEFORE_HIKE`
- `CHECK_SNAP_PUNT`
- `SET_PLAYERS_CLOSE_TO_PASS`

Current host-side code owners remain unchanged:
- `src/FootballGame/Gameplay/OnField/Services/PlayAssignmentService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`

Why it matters here:
- `CommandRuntimeBoundaryHoldingArea.CreateHostRequests(...)` now turns those same four bridge routines into explicit `PlayerCommandRuntimeHostRequest` records instead of letting the new `PlayerCommand*` runtime absorb host responsibilities.
- `PlayerCommandRuntimeBoundary` is only a seam wrapper; it primes execution contexts and routes one bounded step into `PlayerCommandRuntime`.

## What the skeleton deliberately does

- represents one per-player command pointer via `PlayerCommandPointer`
- represents one per-player mutable runtime state carrier via `PlayerCommandExecutionContext`
- represents one bounded decoded command step via `PlayerCommandDefinition`
- records one bounded step result via `PlayerCommandStepResult`
- keeps Bank19_20-originated handoff reasons explicit via `PlayerCommandRuntimeHostRequest`
- keeps the seam explicit via `PlayerCommandRuntimeBoundary`

## What the skeleton deliberately does not do yet

- no live wiring into `OnFieldPlayCoordinator`
- no reuse of older repo command-runtime code
- no broad decode/dispatch table implementation yet
- no attempt to absorb `PlayAssignmentService`, `PreSnapControlService`, or `PassTargetingService` into the Bank21_22 runtime layer

That keeps this slice aligned with the task's requirement to stay small and keep the Bank19_20/Bank21_22 ownership seam visible.

## Bounded verification

Validation run:
- `git diff --check`

Targeted code-inspection assertions run:
- verified the new `PlayerCommandRuntime` type exposes a per-player `StepPlayerCommand(...)` entrypoint
- verified `CommandRuntimeBoundaryHoldingArea.CreateHostRequests(...)` still names the four documented Bank19_20 bridge routines
- verified the host-side owners (`PlayAssignmentService`, `PreSnapControlService`, `PassTargetingService`) remain the same files/types after the skeleton landed

## Result

The repo now has a first production-facing `PlayerCommand*` runtime skeleton for per-player command stepping, while the Bank19_20 host/runtime seam remains explicit and source-traceable.
