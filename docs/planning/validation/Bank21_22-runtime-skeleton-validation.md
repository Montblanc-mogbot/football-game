# Bank21_22 runtime skeleton validation

Updated: 2026-06-03

## Scope

This validation note covers the first production-facing `PlayerCommand*` runtime skeleton added for the Bank21_22 execution layer.

Task target:
- `OPENCLAW_TASKS.md` → "Implement the first Bank21_22 production-facing runtime skeleton"

## Source anchors preserved

### Bank21_22 execution entry/step source anchor
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:240`
  - `DO_NEXT_PLAYER_COMMAND`
  - This remains the direct source reference for the new production-facing stepper naming.

### Bank19_20 host/runtime seam anchors
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:3243-3436`
  - `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
  - Bank19_20 bulk script installation / reassignment seam
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:2718-2966`
  - `DEFENDER_CHANGE_BEFORE_HIKE`
  - pre-snap controlled-defender handoff seam
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:2968-3243`
  - `CHECK_SNAP_PUNT`
  - punt snap gate before Bank21_22 stepping resumes
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:3580-3856`
  - `SET_PLAYERS_CLOSE_TO_PASS`
  - pass-collision retargeting seam into Bank21_22 jump/dive handling

## Production-facing runtime skeleton added

### Runtime types
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
  - owns per-player execution contexts and bounded stepping
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntimeBoundary.cs`
  - keeps the Bank19_20 host/runtime seam explicit instead of letting services call execution state directly
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
  - represents one player's mutable command-stepping state
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandPointer.cs`
  - source-faithful per-player script cursor
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandDefinition.cs`
  - minimal decoded command identity for the first skeleton
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntimeHostRequest.cs`
  - host-owned handoff request preserving which Bank19_20 seam triggered the runtime entry
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`
  - bounded step result for one player-command advance

### Explicit seam retention
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
  - still carries the four deferred Bank19_20 bridge routines
  - now also exposes `CreateHostRequests(OnFieldGameState state)` so the seam is represented as concrete host-side handoff objects rather than only a note/list

## Why this satisfies the slice

This skeleton is intentionally small:
- it does **not** implement real Bank21_22 command semantics yet
- it **does** introduce the smallest coherent production-facing execution-layer vocabulary needed to represent:
  - per-player installed script pointers
  - per-player step advancement
  - pending/continuation state
  - host-triggered handoff reasons tied to the Bank19_20 seam

That keeps the repo on production-facing names (`PlayerCommand*`) without collapsing the still-important Bank19_20 responsibilities owned by:
- `PlayAssignmentService`
- `PreSnapControlService`
- `PassTargetingService`
- `OnFieldPlayCoordinator`

## Bounded verification

Verification run:
- `git diff --check`

Result:
- passed

Reason this gate is the smallest meaningful check here:
- the repo currently has no `.csproj` / `.sln`, so a build is not available as a local validation gate
- this slice is a bounded runtime-shape addition rather than a runnable integration path yet
- `git diff --check` verifies that the new runtime skeleton lands cleanly and is reviewable without whitespace/conflict issues
