# Bank21_22 control-flow command runtime validation

Date: 2026-06-05

## Scope

This validation slice closes the next parity-critical Bank21_22 runtime gap after the live offensive-exchange, defensive-reaction, and pass-contest seams: script control-flow.

Implemented runtime support:

- `DoActionIfCpuJumpCommand` (`DO_ACTION_IF_COM_COMMAND_START`)
- `CpuJumpBasedOnJuiceCommand` (`COM_JUMP_BASED_ON_JUICE_COMMAND_START`)
- `IfCpuJumpCommand` (`IF_COM_JUMP_COMMAND_START`)
- `BranchCommand` (`BRANCH_COMMAND_START`)
- `JumpCommand` (`JUMP_COMMAND_START`)

## Source anchors

Reference bank source:

- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2148-2168`
  - CPU-only jump path
  - CPU+juice-gated jump path
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2225-2237`
  - CPU-conditional jump path
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:4464-4490`
  - signed one-byte branch
  - absolute two-byte jump

## Runtime changes

Files added:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ControlFlowCommandState.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IControlFlowCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ControlFlowCommandDispatcher.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/DoActionIfCpuJumpCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CpuJumpBasedOnJuiceCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/IfCpuJumpCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/BranchCommandHandler.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/JumpCommandHandler.cs`

Files updated:

- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandPointer.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandHandlerResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandStepResult.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandExecutionContext.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/DefensiveReactionCommandDispatcher.cs`

Key semantic change:

- `PlayerCommandExecutionContext.RecordStep(...)` now accepts handler-supplied pointer overrides so Bank21_22 commands that rewrite script cursors do not get flattened into naive byte-length advancement.
- `PlayerCommandStepResult` now exposes the resulting pointer plus any `ControlFlowCommandState`, making command retargeting visible to host-side validation.

## Verification

Bounded compile gate executed successfully with a temporary SDK project that compiled:

- `src/FootballGame/Gameplay/OnField/**/*.cs`

Command shape:

```bash
dotnet build /tmp/.../Bank21_22Subset.csproj -nologo
```

Result:

- Build succeeded
- 0 warnings
- 0 errors

## Notes

This advances Bank21_22 runtime parity materially, but it does **not** mean the full bank conversion is finished. The repo still needs additional command-family coverage beyond the currently implemented exchange, reaction, pass-contest, and control-flow slices.
