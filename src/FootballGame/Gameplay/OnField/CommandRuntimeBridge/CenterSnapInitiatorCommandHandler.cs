using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2383-2401.
/// Handles the bounded under-center snap-initiator wait/delay step from packet 21A.
/// </summary>
public sealed class CenterSnapInitiatorCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "CenterSnapInitiatorCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = "CenterSnapInitiator",
            WaitedForHostSnapGate = true,
            ManualControlRetargeted = false,
            BallCarrierAssigned = false,
            BallAnimationStarted = false,
            BallAnimationResolved = false,
            WaitsForKickRelease = false,
            ContinuationStage = "PostSnapDelay",
            PostExchangeDelayFrames = 4,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Held the center-hike animation until the host snap gate cleared, then carried the short post-snap delay without transferring possession.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
