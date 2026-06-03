using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:4911-4919, 5552-5588.
/// Handles the bounded receiver-miss / defender-only interception-window edge case from packet 21C.
/// </summary>
public sealed class ReceiverMissedBallInterceptionWindowCommandHandler : IPassContestCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ReceiverMissedBallInterceptionWindowCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        PassContestCommandState contestState = new()
        {
            ReceiverJumpOrDiveAttemptResolved = true,
            DefenderOnlyInterceptionWindowActive = true,
            RankedDefenderWindowSize = context.CommandDefinition.OperandValues.TryGetValue("rankedDefenderWindowSize", out string? rankedWindow)
                && int.TryParse(rankedWindow, out int parsedWindow)
                ? parsedWindow
                : 3,
            PreserveSourceBugByPolicy = true,
            ResolutionStage = "ReceiverMissedBallDefenderPriorityWindow",
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Resolved the receiver miss into the ranked defender-only interception window while keeping the source bug path explicit by policy.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = contestState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
