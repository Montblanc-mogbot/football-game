using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3540-3713.
/// Handles the bounded punt receive / meter / launch family.
/// </summary>
public sealed class PuntCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "PuntCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool cpuControlled = GetBoolOperand(context.CommandDefinition, "cpuControlled", false);

        return new PlayerCommandHandlerResult
        {
            Summary = cpuControlled
                ? "Waited for the host punt snap gate, resolved the holder receive/catch transition, then ran the source-visible CPU punt timing windows before computing punt distance from meter plus punter skill and launching the special-teams cutscene path."
                : "Waited for the host punt snap gate, resolved the holder receive/catch transition, then waited for manual punt input before computing punt distance from meter plus punter skill and launching the special-teams cutscene path.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = new SpecialTeamsCommandState
            {
                CommandKind = "Punt",
                SetupKind = "PuntLaunch",
                WaitedForSnapOrKickGate = true,
                WaitedForBallArrival = true,
                BallCarrierAssigned = true,
                BallAnimationStarted = true,
                BallAnimationResolved = true,
                KickMeterOrArrowStarted = true,
                UsesComputerTimingWindow = cpuControlled,
                KickOrPuntDistanceComputed = true,
                KickDirectionRandomizedForCpu = false,
                ReturnerIconApplied = false,
                ManualControlRetargeted = true,
                ReturnerTurnedTowardBall = false,
                ReturnerRunbackStarted = false,
                WaitsForKickRelease = true,
                PreservesAvoidBlockBugByPolicy = false,
                ContinuationStage = cpuControlled ? "CpuPuntTiming" : "ManualPuntTiming",
                PostActionDelayFrames = 8,
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
