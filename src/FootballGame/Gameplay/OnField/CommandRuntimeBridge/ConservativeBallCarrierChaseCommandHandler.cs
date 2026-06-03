using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2714-2772, 3368-3383.
/// Handles the bounded conservative chase loop from packet 21B.
/// </summary>
public sealed class ConservativeBallCarrierChaseCommandHandler : IDefensiveReactionCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ConservativeBallCarrierChaseCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        DefensiveReactionCommandState reactionState = new()
        {
            ChaseMode = "Conservative",
            DiveDelayFrames = 5,
            DiveChancePercent = 60,
            TurnSmoothingTableSize = 16,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Updated the defender into conservative chase steering with carrier-aware turn smoothing before the same dive loop.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = reactionState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
