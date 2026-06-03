using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:7811-7838.
/// Handles the target-runner continuation that receives a regular handoff in packet 21A.
/// </summary>
public sealed class RunnerReceiveHandoffCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "RunnerReceiveHandoffCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = "RunnerReceiveHandoff",
            ManualControlRetargeted = true,
            BallCarrierAssigned = true,
            BallAnimationStarted = false,
            BallAnimationResolved = true,
            ContinuationStage = "RunnerReceiveHandoffAnimation",
            PostExchangeDelayFrames = 20,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Granted the retargeted runner ball-carrier ownership, moved manual control/icon ownership to that runner, and held the two-phase receive-handoff animation.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
