using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:4464-4478.
/// Handles the bounded relative-branch command family.
/// </summary>
public sealed class BranchCommandHandler : IControlFlowCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "BranchCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int relativeOffset = GetIntOperand(context.CommandDefinition, "relativeOffset", 0);
        int baseInstructionOffset = context.ExecutionContext.Pointer.InstructionOffset + context.CommandDefinition.ByteLength;
        int targetInstructionOffset = baseInstructionOffset + relativeOffset;
        string? targetLabel = GetOptionalOperand(context.CommandDefinition, "targetLabel");
        PlayerCommandPointer pointerOverride = context.ExecutionContext.Pointer.SetInstructionOffset(targetInstructionOffset, targetLabel);

        return new PlayerCommandHandlerResult
        {
            Summary = $"Applied the signed one-byte branch offset ({relativeOffset}) and retargeted the player's script cursor to {targetInstructionOffset} before the next Bank21_22 step.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            ControlFlowState = new ControlFlowCommandState
            {
                CommandKind = "RelativeBranch",
                ConditionalGatePassed = true,
                BranchTaken = true,
                UsesRelativeOffset = true,
                TargetInstructionOffset = targetInstructionOffset,
                TargetLabel = targetLabel,
                YieldsOneFrameBeforeResume = true,
            },
            PointerOverride = pointerOverride,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
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
