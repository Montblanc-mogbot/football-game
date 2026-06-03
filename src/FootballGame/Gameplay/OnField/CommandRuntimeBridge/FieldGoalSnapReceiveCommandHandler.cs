using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2501-2534.
/// Handles the bounded FG/XP holder long-snap receive step from packet 21A.
/// </summary>
public sealed class FieldGoalSnapReceiveCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "FieldGoalSnapReceiveCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = "FieldGoalSnapReceive",
            WaitedForHostSnapGate = true,
            ManualControlRetargeted = false,
            BallCarrierAssigned = true,
            BallAnimationStarted = true,
            BallAnimationResolved = true,
            WaitsForKickRelease = true,
            PostExchangeDelayFrames = 60,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Waited for the host snap gate, resolved the holder long-snap ball animation into ball-carrier ownership, then held the post-kick release delay.",
            AwaitingContinuation = true,
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
