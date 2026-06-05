using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:4222-4288.
/// Handles the bounded kickoff/punt returner command family.
/// </summary>
public sealed class ReturnKickOrPuntCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ReturnKickOrPuntCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string returnerRole = context.CommandDefinition.OperandValues.TryGetValue("returnerRole", out string? role)
            ? role
            : "KR";

        SpecialTeamsCommandState state = new()
        {
            CommandKind = "ReturnKickOrPunt",
            WaitedForBallKickedState = true,
            UpdatedManualControlAndDisplay = true,
            ReturnerRole = returnerRole,
            ReturnerTurnedTowardBall = true,
            ReturnerWaitsForCatch = true,
            BallCarrierAssigned = true,
            BallAnimationResolved = true,
            ContinuationStage = "ReceiveKickOrPunt",
        };

        return new PlayerCommandHandlerResult
        {
            Summary = $"Retargeted the displayed icon/manual control to the {returnerRole}, waited for the kicked-ball state, ran the source visible run-to-final-location then turn-to-ball sequence, and completed once the returner received the ball.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = state,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
