using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1750-1835 and 4288-4317.
/// Handles the bounded pre-kick kickoff setup/move family.
/// </summary>
public sealed class SetAndMoveKickoffCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "SetAndMoveKickoffCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool moveDuringKickoff = GetBoolOperand(context.CommandDefinition, "moveDuringKickoff", false);
        bool invertXForPlayerTwo = GetBoolOperand(context.CommandDefinition, "invertXForPlayerTwo", true);
        bool isPlayerTwo = GetBoolOperand(context.CommandDefinition, "isPlayerTwo", false);

        return new PlayerCommandHandlerResult
        {
            Summary = moveDuringKickoff
                ? "Stored the kickoff-relative movement target, waited for the kicked-ball gate, translated the final-ball-relative destination with player-two X inversion when needed, and left the player in the move-until-arrival coverage loop."
                : "Resolved the kickoff pre-snap placement relative to the line of scrimmage, preserving the compact X-distance modifier semantics before normal stepping resumes.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = new SpecialTeamsCommandState
            {
                CommandKind = "SetAndMoveKickoff",
                SetupKind = moveDuringKickoff ? "KickoffCoverageMove" : "KickoffPreSnapPlacement",
                WaitedForSnapOrKickGate = moveDuringKickoff,
                WaitedForBallArrival = false,
                BallCarrierAssigned = false,
                BallAnimationStarted = false,
                BallAnimationResolved = false,
                KickMeterOrArrowStarted = false,
                UsesComputerTimingWindow = false,
                KickOrPuntDistanceComputed = moveDuringKickoff,
                KickDirectionRandomizedForCpu = false,
                ReturnerIconApplied = false,
                ManualControlRetargeted = false,
                ReturnerTurnedTowardBall = false,
                ReturnerRunbackStarted = moveDuringKickoff,
                WaitsForKickRelease = false,
                PreservesAvoidBlockBugByPolicy = false,
                ContinuationStage = moveDuringKickoff
                    ? (invertXForPlayerTwo && isPlayerTwo ? "KickoffCoverageMoveWithPlayerTwoXInversion" : "KickoffCoverageMove")
                    : "KickoffPreSnapPlacement",
                PostActionDelayFrames = moveDuringKickoff ? 1 : null,
            },
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }

    private static bool GetBoolOperand(PlayerCommandDefinition commandDefinition, string key, bool defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            && bool.TryParse(value, out bool parsedValue)
            ? parsedValue
            : defaultValue;
    }
}
