using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2618-2635.
/// Handles the bounded aggressive chase loop from packet 21B.
/// </summary>
public sealed class AggressiveBallCarrierChaseCommandHandler : IDefensiveReactionCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "AggressiveBallCarrierChaseCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        DefensiveReactionCommandState reactionState = new()
        {
            ChaseMode = "Aggressive",
            DiveDelayFrames = 5,
            DiveChancePercent = 60,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Updated the defender into direct ball pursuit with repeated high-pressure dive checks.",
            AwaitingContinuation = true,
            DefensiveReactionState = reactionState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
