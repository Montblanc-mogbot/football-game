using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1750-1834.
/// Handles the bounded kickoff setup / move-relative-to-final-ball-location command family.
/// </summary>
public sealed class SetAndMoveKickoffCoverageCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "SetAndMoveKickoffCoverageCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        SpecialTeamsCommandState state = new()
        {
            CommandKind = "SetAndMoveKickoffCoverage",
            WaitedForBallKickedState = true,
            StartedCoverageRunToBallRelativeSpot = true,
            FakeOrOnsideAware = true,
            ContinuationStage = "MoveRelativeToFinalBallLocation",
            WaitFrames = 1,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Stored the kickoff-relative setup/move operands, waited for the ball-kicked flag, then turned the player toward the final ball-relative coverage landmark and entered the move-until-arrival continuation.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = state,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
