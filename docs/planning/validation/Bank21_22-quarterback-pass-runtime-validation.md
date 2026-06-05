# Bank21_22 quarterback/pass runtime validation

Updated: 2026-06-05

## Scope

Bounded Bank21_22 quarterback/pass-control conversion slice covering:
- `QB_DROPBACK_COMMAND_START`
- `COM_WAIT_TO_PASS_COMMAND_START`
- `COM_PASS_COMMAND_START`
- the adjacent live host/runtime seam sample routed through `SET_PLAYERS_CLOSE_TO_PASS`

This slice intentionally keeps the existing Bank19_20 / Bank21_22 split explicit:
- `PassTargetingService` still owns the Bank19_20-side pass-target ordering trigger.
- `OnFieldPlayCoordinator` still owns the live host seam release and the explicit adjacent continuation sequencing.
- the new `QuarterbackPassCommandDispatcher` + handlers own the bounded Bank21_22 quarterback dropback / wait / CPU-pass semantics once the seam is entered.

## Source anchors

- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1652-1704` — `COM_PASS_COMMAND_START`
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1893-1955` — `QB_DROPBACK_COMMAND_START`
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1957-1994` — `COM_WAIT_TO_PASS_COMMAND_START`
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:3580-3856` — `SET_PLAYERS_CLOSE_TO_PASS` host-side seam that still primes the runtime entry

## Production-facing runtime changes

### New Bank21_22 runtime state/dispatch surface

Added:
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/QuarterbackPassCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IQuarterbackPassCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/QuarterbackPassCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/QuarterbackDropbackCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CpuWaitToPassCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CpuPassCommandHandler.cs`

Updated:
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`

These changes preserve the source-visible semantics without collapsing them into generic Bank19_20 flags:
- dropback target capture, player-two X inversion, direction/speed initialization intent, alternating animation loop, and back-of-end-zone-safe exit
- randomized CPU wait-to-pass timing with the adjacent collision-pressure early-throw gate
- CPU pass target-table selection, target-player choice, pass-attempt start, and post-throw hold timing

### Live host/runtime seam update

Updated:
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`

The current live `SET_PLAYERS_CLOSE_TO_PASS` sample no longer enters the defender jump/dive contest command directly.
Instead it now samples the quarterback pass-control slice as a coherent adjacent sequence:
1. `QuarterbackDropbackCommand`
2. explicit same-player continuation into `CpuWaitToPassCommand`
3. explicit same-player continuation into `CpuPassCommand`

This keeps the Bank19_20 host seam explicit while giving the runtime a production-facing quarterback pre-throw control loop.

## Task evidence

- `PassTargetingService` remains the explicit Bank19_20-side owner of `SET_PLAYERS_CLOSE_TO_PASS` request priming.
- `PlayerCommandRuntime` now carries a separate `QuarterbackPassCommandState` branch rather than overloading movement, player-control, or pass-contest state.
- `OnFieldPlayCoordinator` still performs the live seam release and now also sequences the adjacent quarterback follow-up commands explicitly, matching the source-local command adjacency without pretending the full decoder already exists.

## Validation gate

Bounded compile gate run over `src/FootballGame/Gameplay/OnField/**/*.cs` using a temporary subset csproj:
- `dotnet build /tmp/Bank21_22Subset.csproj`

Result:
- `dotnet build /tmp/Bank21_22Subset.csproj` passed cleanly on 2026-06-05 (`0 Warning(s)`, `0 Error(s)`).
