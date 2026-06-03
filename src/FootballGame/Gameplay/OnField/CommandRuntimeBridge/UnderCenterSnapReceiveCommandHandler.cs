using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2436-2447.
/// Handles the bounded under-center snap receive step from packet 21A.
/// </summary>
public sealed class UnderCenterSnapReceiveCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "UnderCenterSnapReceiveCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = "UnderCenterSnapReceive",
            WaitedForHostSnapGate = true,
            ManualControlRetargeted = true,
            BallCarrierAssigned = true,
            BallAnimationStarted = false,
            BallAnimationResolved = true,
            WaitsForKickRelease = false,
            PostExchangeDelayFrames = 4,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Retargeted manual control to the snap receiver, granted ball-carrier ownership after the host snap gate, and held the short receive delay.",
            AwaitingContinuation = true,
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
