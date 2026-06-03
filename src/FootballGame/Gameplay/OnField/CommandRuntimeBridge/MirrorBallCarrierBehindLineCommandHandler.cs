using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2644-2687.
/// Handles the bounded mirror-behind-the-line defensive reaction loop from packet 21B.
/// </summary>
public sealed class MirrorBallCarrierBehindLineCommandHandler : IDefensiveReactionCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "MirrorBallCarrierBehindLineCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        DefensiveReactionCommandState reactionState = new()
        {
            IsHoldingMirrorLane = true,
            DiveDelayFrames = 13,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Mirrored the ball carrier vertically, then held the defender in a patient mirror lane behind the line of scrimmage.",
            AwaitingContinuation = true,
            DefensiveReactionState = reactionState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
