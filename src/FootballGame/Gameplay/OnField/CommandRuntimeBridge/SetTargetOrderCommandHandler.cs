using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1714-1725.
/// Handles the bounded pass-target ordering family.
/// </summary>
public sealed class SetTargetOrderCommandHandler : ITargetingCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "SetTargetOrderCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int targetPriorityIndex = GetIntOperand(context.CommandDefinition, "targetPriorityIndex", 0);
        bool becameCurrentPassTarget = targetPriorityIndex == 0;

        return new PlayerCommandHandlerResult
        {
            Summary = becameCurrentPassTarget
                ? $"Installed '{context.PlayerSlotKey}' as pass-target priority {targetPriorityIndex} and marked it as the current first-read target."
                : $"Installed '{context.PlayerSlotKey}' as pass-target priority {targetPriorityIndex} while leaving the current first-read target unchanged.",
            AwaitingContinuation = false,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = null,
            PlayerControlCommandState = null,
            ControlFlowState = null,
            PointerOverride = null,
            SourceNotes = context.CommandDefinition.SourceNotes,
            PassTargetOrderCommandState = new PassTargetOrderCommandState
            {
                CommandKind = "SetTargetOrder",
                ReceiverPlayerSlot = context.PlayerSlotKey,
                TargetPriorityIndex = targetPriorityIndex,
                InstalledIntoPassTargetArray = true,
                BecameCurrentPassTarget = becameCurrentPassTarget,
            },
        };
    }

    private static int GetIntOperand(PlayerCommandDefinition commandDefinition, string key, int defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            && int.TryParse(value, out int parsedValue)
            ? parsedValue
            : defaultValue;
    }
}
