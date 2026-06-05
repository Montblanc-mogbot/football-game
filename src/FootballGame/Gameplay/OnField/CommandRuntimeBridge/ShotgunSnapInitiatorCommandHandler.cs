using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2402-2435.
/// Handles the bounded shotgun snap-initiator release/wait step from packet 21A.
/// </summary>
public sealed class ShotgunSnapInitiatorCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ShotgunSnapInitiatorCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = "ShotgunSnapInitiator",
            WaitedForHostSnapGate = true,
            ManualControlRetargeted = false,
            BallCarrierAssigned = false,
            BallAnimationStarted = true,
            BallAnimationResolved = false,
            WaitsForKickRelease = false,
            InFlightBallStateCreated = true,
            ContinuationStage = "SnapTravelDelay",
            PostExchangeDelayFrames = 30,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Seeded the shotgun snap-release state, waited for the host snap gate, and held the long travel delay without yet granting ball-carrier ownership.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
