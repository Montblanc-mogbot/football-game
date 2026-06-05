# Bank21_22 quarterback / pass-control runtime validation — 2026-06-05

## Scope

Bounded slice B for the remaining Bank21_22 conversion:
- `QB_DROPBACK_COMMAND_START`
- `COM_WAIT_TO_PASS_COMMAND_START`
- `COM_PASS_COMMAND_START`
- directly adjacent host/runtime seam setup needed to carry those semantics through the existing on-field boundary

## Source anchors

- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1652-1704` — `COM_PASS_COMMAND_START`
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1893-1954` — `QB_DROPBACK_COMMAND_START`
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1957-1993` — `COM_WAIT_TO_PASS_COMMAND_START`
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:3858-3906` — `UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS` host-side pass timing / target-indicator seam

## What changed

### New runtime family

Added bounded quarterback/pass-control runtime files under `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/`:
- `QuarterbackPassCommandDispatcher.cs`
- `IQuarterbackPassCommandHandler.cs`
- `QuarterbackPassCommandState.cs`
- `QuarterbackDropbackCommandHandler.cs`
- `QuarterbackWaitToPassCommandHandler.cs`
- `ComputerPassCommandHandler.cs`

This keeps the Bank21_22 quarterback/pass-control family inside the existing `PlayerCommandRuntime` command-family model instead of pushing these semantics into generic host flags.

### Runtime dispatch wiring

Updated:
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`

The runtime now exposes `QuarterbackPassCommandState` alongside the existing offensive-exchange / movement / player-control / presnap / targeting families, so the seam can carry:
- dropback target and animation-loop semantics
- wait-to-pass timer / pressure-exit semantics
- CPU pass target-selection and pass-attempt start semantics

### Host/runtime seam integration

Updated:
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
- `src/FootballGame/Gameplay/OnField/OnFieldGameState.cs`

Key seam decisions:
- `CommandRuntimeBoundaryHoldingArea` now includes `UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS` as an explicit host/runtime bridge routine rather than inventing a separate QB-only integration path.
- `PassTargetingService.QueueQuarterbackPassControl(...)` primes that existing host seam, preserving service ownership of *when* the visible pass-target / pass-timing moment occurs.
- `OnFieldPlayCoordinator.RunPassPlayLoop(...)` releases one bounded quarterback/pass-control runtime step through the same `TryAdvanceLiveCommandRuntime(...)` seam already used for the other Bank21_22 families.
- `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now maps:
  - `CHECK_SNAP_PUNT` → `QuarterbackDropbackCommand` / `QB_DROPBACK_COMMAND_START`
  - `UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS` → `ComputerPassCommand` / `COM_PASS_COMMAND_START`
- `OnFieldPlayCoordinator.ApplyRuntimeStateToHost(...)` mirrors the new runtime side effects back into explicit host state:
  - `QuarterbackDropbackRelativeX`
  - `QuarterbackDropbackTargetY`
  - `QuarterbackPassWaitFrames`
  - `QuarterbackWaitsForPassPressure`
  - `QuarterbackSackChanceThreshold`
  - `PendingCpuPassTargetSlotKey`
  - `PendingCpuPassTargetPriority`
  - `PassAttempted` when the runtime actually starts the pass attempt

## Ownership boundary check

The slice stays seam-first:
- `OnFieldPlayCoordinator` still owns the high-level pass-play loop and dead-ball resolution
- `PassTargetingService` still owns pass-target indicator timing and ranking/ordering setup
- `PreSnapControlService` still owns the snapped-ball gate
- `CommandRuntimeBoundaryHoldingArea` still carries the host/runtime bridge inventory
- `PlayerCommandRuntime` now owns the bounded QB/pass-control command-family semantics rather than collapsing them into host booleans alone

## Validation

### Build gate

Passed:
- `dotnet build /tmp/Bank21_22Subset.csproj`

### Tree hygiene

Passed:
- `git diff --check`

### Direct inspection checks

Confirmed by code inspection:
- `PlayerCommandRuntime` dispatches the new quarterback/pass-control family through `QuarterbackPassCommandDispatcher`
- `PassTargetingService` uses the existing host/runtime seam instead of inventing a parallel coordinator path
- `OnFieldPlayCoordinator.CreateLiveStepDefinition(...)` now emits production-facing `QuarterbackDropbackCommand` and `ComputerPassCommand` step definitions with source-faithful notes/operands
- `OnFieldPlayCoordinator.ApplyRuntimeStateToHost(...)` mirrors the new runtime state back into explicit host fields

## Remaining follow-up

This closes the requested bounded QB/pass-control slice at the runtime-family level, but the remaining Bank21_22 completion work still includes:
- special-teams / return families from slice C
- any final post-slice audit leftovers from slice D
