using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:4222-4269.
/// Handles the bounded kick/punt returner handoff family.
/// </summary>
public sealed class ReturnKickPuntCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ReturnKickPuntCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool kickoffReturn = GetBoolOperand(context.CommandDefinition, "kickoffReturn", false);

        return new PlayerCommandHandlerResult
        {
            Summary = kickoffReturn
                ? "Switched the displayed icon/manual-control pointer to the kickoff returner, waited for the kicked-ball gate, ran the returner to the final catch spot or back of the end zone, turned the returner toward the incoming ball, and granted possession once ball collision resolved the catch."
                : "Switched the displayed icon/manual-control pointer to the punt returner, waited for the kicked-ball gate, ran the returner to the final catch spot or back of the end zone, turned the returner toward the incoming ball, and granted possession once ball collision resolved the catch.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = new SpecialTeamsCommandState
            {
                CommandKind = "ReturnKickPunt",
                SetupKind = kickoffReturn ? "KickoffReturn" : "PuntReturn",
                WaitedForSnapOrKickGate = true,
                WaitedForBallArrival = true,
                BallCarrierAssigned = true,
                BallAnimationStarted = false,
                BallAnimationResolved = true,
                KickMeterOrArrowStarted = false,
                UsesComputerTimingWindow = false,
                KickOrPuntDistanceComputed = false,
                KickDirectionRandomizedForCpu = false,
                ReturnerIconApplied = true,
                ManualControlRetargeted = true,
                ReturnerTurnedTowardBall = true,
                ReturnerRunbackStarted = true,
                WaitsForKickRelease = false,
                PreservesAvoidBlockBugByPolicy = false,
                ContinuationStage = kickoffReturn ? "KickoffReturnCatchSetup" : "PuntReturnCatchSetup",
                PostActionDelayFrames = 1,
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
