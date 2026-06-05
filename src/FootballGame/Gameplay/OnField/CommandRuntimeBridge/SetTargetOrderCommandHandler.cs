using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1714-1723.
/// Handles the bounded pass-target ordering command family.
/// </summary>
public sealed class SetTargetOrderCommandHandler : IPreSnapTargetingCommandHandler
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
        bool setAsCurrentPassTarget = targetPriorityIndex == 0;

        return new PlayerCommandHandlerResult
        {
            Summary = setAsCurrentPassTarget
                ? "Recorded this receiver as Bank21_22 pass-target priority #0 and updated the current-pass-target slot to match the source-visible first-target side effect."
                : $"Recorded this receiver into Bank21_22 pass-target priority slot #{targetPriorityIndex} without replacing the already-selected first target.",
            AwaitingContinuation = false,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = null,
            PlayerControlCommandState = null,
            PreSnapTargetingCommandState = new PreSnapTargetingCommandState
            {
                CommandKind = "SetTargetOrder",
                MirrorTargetPlayerSlot = null,
                FollowDelayFrames = null,
                VerticalProximityLimit = null,
                WaitsForBallSnapExit = false,
                HoldsVerticalMirrorLoop = false,
                QueuedFacingResetOnHold = false,
                QueuedStandingResetOnHold = false,
                TargetPriorityIndex = targetPriorityIndex,
                SetAsCurrentPassTarget = setAsCurrentPassTarget,
                UpdatedPassTargetOrder = true,
            },
            ControlFlowState = null,
            PointerOverride = null,
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
}
