using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:5125-5459.
/// Handles the bounded packet-21C defensive jump/dive contest family entry.
/// </summary>
public sealed class DefensiveJumpDiveCatchPassCommandHandler : IPassContestCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "DefensiveJumpDiveCatchPassCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        PassContestCommandState contestState = new()
        {
            ReceiverJumpOrDiveAttemptResolved = true,
            DefenderOnlyInterceptionWindowActive = false,
            RankedDefenderWindowSize = context.CommandDefinition.OperandValues.TryGetValue("rankedDefenderWindowSize", out string? rankedWindow)
                && int.TryParse(rankedWindow, out int parsedWindow)
                ? parsedWindow
                : 3,
            PreserveSourceBugByPolicy = false,
            ResolutionStage = "DefensiveJumpDiveCatchSetup",
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Entered the defensive jump/dive catch family, preserving the defender-side movement, collision, and near-ball resolution stage as a production-facing runtime command.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = contestState,
            OffensiveExchangeState = null,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
