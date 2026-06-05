using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:4479-4490.
/// Handles the bounded absolute-jump command family.
/// </summary>
public sealed class JumpCommandHandler : IControlFlowCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "JumpCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int targetInstructionOffset = GetIntOperand(context.CommandDefinition, "targetInstructionOffset", context.ExecutionContext.Pointer.InstructionOffset);
        string? targetLabel = GetOptionalOperand(context.CommandDefinition, "targetLabel");
        PlayerCommandPointer pointerOverride = context.ExecutionContext.Pointer.SetInstructionOffset(targetInstructionOffset, targetLabel);

        return new PlayerCommandHandlerResult
        {
            Summary = $"Replaced the player's script cursor with the absolute jump target {targetInstructionOffset} and queued the one-frame yield before Bank21_22 resumes at the new address.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            ControlFlowState = new ControlFlowCommandState
            {
                CommandKind = "AbsoluteJump",
                ConditionalGatePassed = true,
                BranchTaken = true,
                UsesRelativeOffset = false,
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
