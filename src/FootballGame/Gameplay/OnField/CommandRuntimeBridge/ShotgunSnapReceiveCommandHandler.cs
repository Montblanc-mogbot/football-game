using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2450-2498.
/// Handles the bounded shotgun snap receive step from packet 21A.
/// </summary>
public sealed class ShotgunSnapReceiveCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ShotgunSnapReceiveCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = "ShotgunSnapReceive",
            WaitedForHostSnapGate = true,
            ManualControlRetargeted = true,
            BallCarrierAssigned = true,
            BallAnimationStarted = true,
            BallAnimationResolved = true,
            WaitsForKickRelease = false,
            InFlightBallStateCreated = true,
            ContinuationStage = "WaitForShotgunBallCollision",
            PostExchangeDelayFrames = 4,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Retargeted manual control to the shotgun receiver, started the long-snap ball animation after the host snap gate, waited for ball collision with the quarterback, then granted ball-carrier ownership and held the short receive delay.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
