using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2156-2168.
/// Handles the bounded CPU-juice-gated jump command family.
/// </summary>
public sealed class CpuJumpBasedOnJuiceCommandHandler : IControlFlowCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "CpuJumpBasedOnJuiceCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool isCpuControlled = GetBoolOperand(context.CommandDefinition, "isCpuControlled", false);
        int cpuJuiceLevel = GetIntOperand(context.CommandDefinition, "cpuJuiceLevel", 0);
        int requiredJuiceLevel = GetIntOperand(context.CommandDefinition, "requiredJuiceLevel", 0);
        bool branchTaken = isCpuControlled && cpuJuiceLevel >= requiredJuiceLevel;
        int targetInstructionOffset = GetIntOperand(context.CommandDefinition, "targetInstructionOffset", context.ExecutionContext.Pointer.InstructionOffset);
        string? targetLabel = GetOptionalOperand(context.CommandDefinition, "targetLabel");
        PlayerCommandPointer? pointerOverride = branchTaken
            ? context.ExecutionContext.Pointer.SetInstructionOffset(targetInstructionOffset, targetLabel)
            : null;

        return new PlayerCommandHandlerResult
        {
            Summary = branchTaken
                ? $"Passed the CPU-juice gate ({cpuJuiceLevel} >= {requiredJuiceLevel}) and redirected the script cursor to {targetInstructionOffset}."
                : $"Skipped the CPU-juice jump because the gate did not pass ({cpuJuiceLevel} < {requiredJuiceLevel}) or the player was man-controlled.",
            AwaitingContinuation = branchTaken,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            ControlFlowState = new ControlFlowCommandState
            {
                CommandKind = "CpuJuiceConditionalJump",
                ConditionalGatePassed = branchTaken,
                BranchTaken = branchTaken,
                UsesRelativeOffset = false,
                TargetInstructionOffset = branchTaken
                    ? targetInstructionOffset
                    : context.ExecutionContext.Pointer.InstructionOffset + context.CommandDefinition.ByteLength,
                TargetLabel = branchTaken ? targetLabel : null,
                YieldsOneFrameBeforeResume = branchTaken,
            },
            PointerOverride = pointerOverride,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }

    private static bool GetBoolOperand(PlayerCommandDefinition commandDefinition, string key, bool defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            && bool.TryParse(value, out bool parsedValue)
            ? parsedValue
            : defaultValue;
    }

    private static int GetIntOperand(PlayerCommandDefinition commandDefinition, string key, int defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            && int.TryParse(value, out int parsedValue)
            ? parsedValue
            : defaultValue;
    }

    private static string? GetOptionalOperand(PlayerCommandDefinition commandDefinition, string key)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            ? value
            : null;
    }
}
