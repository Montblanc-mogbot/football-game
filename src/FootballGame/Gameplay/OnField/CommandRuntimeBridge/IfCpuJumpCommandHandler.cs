using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2225-2238.
/// Handles the bounded opponent-CPU jump command family.
/// </summary>
public sealed class IfCpuJumpCommandHandler : IControlFlowCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "IfCpuJumpCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool isCpuControlled = GetBoolOperand(context.CommandDefinition, "isCpuControlled", false);
        int targetInstructionOffset = GetIntOperand(context.CommandDefinition, "targetInstructionOffset", context.ExecutionContext.Pointer.InstructionOffset);
        string? targetLabel = GetOptionalOperand(context.CommandDefinition, "targetLabel");
        PlayerCommandPointer? pointerOverride = isCpuControlled
            ? context.ExecutionContext.Pointer.SetInstructionOffset(targetInstructionOffset, targetLabel)
            : null;

        return new PlayerCommandHandlerResult
        {
            Summary = isCpuControlled
                ? $"Confirmed the opposing side is CPU-controlled and redirected the script cursor to {targetInstructionOffset}."
                : "Detected a man-controlled side and left the script cursor on the fallthrough path.",
            AwaitingContinuation = isCpuControlled,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            ControlFlowState = new ControlFlowCommandState
            {
                CommandKind = "IfCpuJump",
                ConditionalGatePassed = isCpuControlled,
                BranchTaken = isCpuControlled,
                UsesRelativeOffset = false,
                TargetInstructionOffset = isCpuControlled
                    ? targetInstructionOffset
                    : context.ExecutionContext.Pointer.InstructionOffset + context.CommandDefinition.ByteLength,
                TargetLabel = isCpuControlled ? targetLabel : null,
                YieldsOneFrameBeforeResume = isCpuControlled,
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
