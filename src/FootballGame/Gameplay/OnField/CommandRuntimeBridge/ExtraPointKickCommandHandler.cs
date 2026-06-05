using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3720-4221.
/// Handles the bounded extra-point kick / block-check family via the shared FG/XP logic.
/// </summary>
public sealed class ExtraPointKickCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ExtraPointKickCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool cpuControlled = context.CommandDefinition.OperandValues.TryGetValue("cpuControlled", out string? cpuValue)
            && bool.TryParse(cpuValue, out bool parsedCpu)
            && parsedCpu;
        bool preserveAvoidBlockBug = !context.CommandDefinition.OperandValues.TryGetValue("preserveAvoidBlockBug", out string? bugValue)
            || !bool.TryParse(bugValue, out bool parsedBug)
            || parsedBug;

        return new PlayerCommandHandlerResult
        {
            Summary = cpuControlled
                ? "Reused the shared FG/XP kick path for the extra point, including CPU arrow timing/accuracy boost, kicker approach, and block-vs-flight resolution while keeping the source avoid-block bug explicit."
                : "Reused the shared FG/XP kick path for the extra point, including manual arrow timing, kicker approach, and block-vs-flight resolution while keeping the source avoid-block bug explicit.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = new SpecialTeamsCommandState
            {
                CommandKind = "ExtraPoint",
                SetupKind = "ExtraPoint",
                WaitedForSnapOrKickGate = true,
                WaitedForBallArrival = true,
                BallCarrierAssigned = false,
                BallAnimationStarted = false,
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
                PreservesAvoidBlockBugByPolicy = preserveAvoidBlockBug,
                ContinuationStage = cpuControlled ? "CpuExtraPointArrowAndKick" : "ManualExtraPointArrowAndKick",
                PostActionDelayFrames = 1,
            },
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
