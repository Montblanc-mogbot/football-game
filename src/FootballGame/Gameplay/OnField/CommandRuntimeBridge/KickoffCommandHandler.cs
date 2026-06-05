using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3421-3537.
/// Handles the bounded kickoff meter / launch command family.
/// </summary>
public sealed class KickoffCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "KickoffCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool cpuControlled = GetBoolOperand(context.CommandDefinition, "cpuControlled", false);
        bool onsideKick = GetBoolOperand(context.CommandDefinition, "onsideKick", false);

        return new PlayerCommandHandlerResult
        {
            Summary = cpuControlled
                ? "Prepared the kicker presentation state, ran the kickoff power-bar timing under the CPU/computer wait-window rules, enforced the non-onside meter floor when needed, randomized kick direction, and launched the kick through the shared release path."
                : "Prepared the kicker presentation state, started the kickoff power bar, waited for the manual kick input, and launched the kick through the shared release path.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = new SpecialTeamsCommandState
            {
                CommandKind = "Kickoff",
                SetupKind = onsideKick ? "OnsideKickoff" : "StandardKickoff",
                WaitedForSnapOrKickGate = false,
                WaitedForBallArrival = false,
                BallCarrierAssigned = false,
                BallAnimationStarted = true,
                BallAnimationResolved = false,
                KickMeterOrArrowStarted = true,
                UsesComputerTimingWindow = cpuControlled,
                KickOrPuntDistanceComputed = true,
                KickDirectionRandomizedForCpu = cpuControlled,
                ReturnerIconApplied = false,
                ManualControlRetargeted = true,
                ReturnerTurnedTowardBall = false,
                ReturnerRunbackStarted = false,
                WaitsForKickRelease = true,
                PreservesAvoidBlockBugByPolicy = false,
                ContinuationStage = cpuControlled
                    ? (onsideKick ? "CpuOnsideKickTiming" : "CpuKickoffTiming")
                    : "ManualKickoffTiming",
                PostActionDelayFrames = cpuControlled ? 16 : 3,
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
