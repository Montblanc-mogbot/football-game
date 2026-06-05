using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1592-1638.
/// Handles the bounded defender pre-snap motion mirror loop.
/// </summary>
public sealed class PreSnapMotionCommandHandler : IPreSnapTargetingCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "PreSnapMotionCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string mirrorTargetPlayerSlot = GetStringOperand(context.CommandDefinition, "mirrorTargetPlayerSlot", "MOTION_TARGET");
        int followDelayFrames = GetIntOperand(context.CommandDefinition, "followDelayFrames", 9);
        int verticalProximityLimit = GetIntOperand(context.CommandDefinition, "verticalProximityLimit", 10);

        return new PlayerCommandHandlerResult
        {
            Summary = $"Captured the Bank21_22 pre-snap motion mirror target '{mirrorTargetPlayerSlot}', staged the repeating {followDelayFrames}-frame follow delay, and left the defender in the vertical mirror loop until the host snap gate releases it.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = null,
            PlayerControlCommandState = null,
            PreSnapTargetingCommandState = new PreSnapTargetingCommandState
            {
                CommandKind = "PreSnapMotion",
                MirrorTargetPlayerSlot = mirrorTargetPlayerSlot,
                FollowDelayFrames = followDelayFrames,
                VerticalProximityLimit = verticalProximityLimit,
                WaitsForBallSnapExit = true,
                HoldsVerticalMirrorLoop = true,
                QueuedFacingResetOnHold = true,
                QueuedStandingResetOnHold = true,
                TargetPriorityIndex = null,
                SetAsCurrentPassTarget = false,
                UpdatedPassTargetOrder = false,
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

    private static string GetStringOperand(PlayerCommandDefinition commandDefinition, string key, string defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            && !string.IsNullOrWhiteSpace(value)
            ? value
            : defaultValue;
    }
}
