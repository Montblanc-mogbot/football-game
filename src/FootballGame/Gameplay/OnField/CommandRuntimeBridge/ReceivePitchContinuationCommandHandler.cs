using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:7931-7944.
/// Handles the target-runner continuation that waits for the pitched ball to arrive.
/// </summary>
public sealed class ReceivePitchContinuationCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ReceivePitchContinuationCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = "ReceivePitchContinuation",
            ManualControlRetargeted = true,
            BallCarrierAssigned = true,
            BallAnimationStarted = true,
            BallAnimationResolved = true,
            ContinuationStage = "WaitForPitchCollision",
            PostExchangeDelayFrames = 1,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Repeatedly treated the retargeted runner as the incoming pitch receiver, updated manual-control/display ownership, and completed once ball collision resolved the catch.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
