# Bank21_22 pass-contest edge-case handler validation — 2026-06-03

## Scope

This validation covers the next live Bank21_22 runtime slice after the initial packet-21B defensive reaction handler work.

Chosen family:
- packet `21C`
- source bank: `Bank21_22_play_commands_on_field_logic.asm`
- bounded production-facing family: the receiver-miss / defender-only interception-window path documented in packet 21C

## Files inspected/updated

### Runtime bridge / handlers
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PassContestCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IPassContestCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PassContestCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ReceiverMissedBallInterceptionWindowCommandHandler.cs`

### Host/runtime seam
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`

## Source references used

### Bank19_20 host-side priming
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:3580-3856`
- `SET_PLAYERS_CLOSE_TO_PASS`

### Bank21_22 receiver-side contest / miss window
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:4759-5119`
- `OFFENSE_JUMP_DIVE_CATCH_PASS_START`
- `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH`

### Bank21_22 defender-priority interception path
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:5552-5588`
- `PASS_CALCULATION_TARGET_NOT_CLOSE_TO_BALL`

## What changed

### 1. The live `SET_PLAYERS_CLOSE_TO_PASS` seam now enters packet 21C instead of reusing packet 21B chase logic

`OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now emits:
- `ReceiverMissedBallInterceptionWindowCommand`
- source label `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH`

That means the host-side pass-target ordering seam still starts in Bank19_20, but the first live step now lands in the Bank21_22 receiver-miss / ranked-defender interception window documented by packet 21C.

### 2. The runtime gained a separate pass-contest dispatcher/state family

The new production-facing runtime types are:
- `PassContestCommandDispatcher`
- `IPassContestCommandHandler`
- `ReceiverMissedBallInterceptionWindowCommandHandler`
- `PassContestCommandState`

This keeps the packet-21C slice separate from the already-ported defensive reaction family instead of overloading `DefensiveReactionCommandState` with unrelated pass-result semantics.

### 3. The source-visible bug remains explicit in runtime state and handler policy

`ReceiverMissedBallInterceptionWindowCommandHandler` records:
- an active defender-only interception window
- ranked defender window size = 3
- `PreserveSourceBugByPolicy = true`

That keeps the `PASS_CALCULATION_TARGET_NOT_CLOSE_TO_BALL` bug path explicit rather than silently normalizing it away.

## Wiring check against the documented split

### Still host-owned (Bank19_20 side)
- `PassTargetingService` still owns receiver/defender ranking and runtime-request priming through `SET_PLAYERS_CLOSE_TO_PASS`
- `CommandRuntimeBoundaryHoldingArea` still exposes `SET_PLAYERS_CLOSE_TO_PASS` plus the pass-contest bridge symbol set
- the host continues to queue the seam before the runtime step executes

### Newly runtime-owned (Bank21_22 side)
- `PlayerCommandRuntime` now routes `SET_PLAYERS_CLOSE_TO_PASS` into a packet-21C pass-contest dispatcher instead of treating it as part of the packet-21B defensive chase family
- per-step miss/interception-window continuation state now lives in `PlayerCommandExecutionContext.PassContestState`

This preserves the documented split: host side ranks and primes the participants, while the command runtime owns the receiver-miss / defender-only interception semantics after entry.

## Bounded validation evidence

### `git diff --check`
- passed after the packet-21C live handler slice changes

### Direct code inspection checks
Confirmed by inspection that:
- `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now routes the live `SET_PLAYERS_CLOSE_TO_PASS` seam into `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH`
- `PlayerCommandRuntime.StepPlayerCommand(...)` dispatches `SET_PLAYERS_CLOSE_TO_PASS` into the new pass-contest dispatcher rather than the packet-21B defensive-reaction dispatcher
- `PassTargetingService` and `CommandRuntimeBoundaryHoldingArea` remain explicit Bank19_20-side owners of ranking/priming
- the new runtime state keeps the source bug path explicit by policy instead of silently fixing it

## Structural validation limit

A full build gate remains structurally unavailable in this repo because there is still no `.csproj` or `.sln` present under `/home/montblanc/repos/football-game`.
