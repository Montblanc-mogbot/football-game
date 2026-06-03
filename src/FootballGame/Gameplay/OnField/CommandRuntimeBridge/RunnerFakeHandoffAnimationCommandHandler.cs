using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:7839-7848.
/// Handles the target-runner continuation that plays the fake-handoff animation without transferring possession.
/// </summary>
public sealed class RunnerFakeHandoffAnimationCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "RunnerFakeHandoffAnimationCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = "RunnerFakeHandoffAnimation",
            ManualControlRetargeted = false,
            BallCarrierAssigned = false,
            BallAnimationStarted = false,
            BallAnimationResolved = true,
            FakeExchange = true,
            ContinuationStage = "RunnerFakeHandoffAnimation",
            PostExchangeDelayFrames = 20,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Played the retargeted runner fake-handoff animation without transferring ball-carrier ownership.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
