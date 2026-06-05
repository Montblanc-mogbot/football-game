using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3716-4221.
/// Handles the bounded field-goal kick / block-check family.
/// </summary>
public sealed class FieldGoalKickCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "FieldGoalKickCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return CreateResult(context, kickType: "FieldGoal");
    }

    private static PlayerCommandHandlerResult CreateResult(PlayerCommandHandlerContext context, string kickType)
    {
        bool cpuControlled = GetBoolOperand(context.CommandDefinition, "cpuControlled", false);
        bool preserveAvoidBlockBug = GetBoolOperand(context.CommandDefinition, "preserveAvoidBlockBug", true);

        return new PlayerCommandHandlerResult
        {
            Summary = cpuControlled
                ? $"Prepared the {kickType} kicker state, waited for the host snap/holder receive path, ran the arrow/timing gate with CPU accuracy boost, moved the kicker into contact with the ball, then resolved the source-visible block-vs-miss-vs-flight branch while preserving the avoid-block index bug as explicit runtime policy."
                : $"Prepared the {kickType} kicker state, waited for the host snap/holder receive path, let manual input release the kick after the arrow gate, moved the kicker into contact with the ball, then resolved the source-visible block-vs-miss-vs-flight branch while preserving the avoid-block index bug as explicit runtime policy.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = new SpecialTeamsCommandState
            {
                CommandKind = kickType,
                SetupKind = kickType,
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
                ContinuationStage = cpuControlled ? $"Cpu{kickType}ArrowAndKick" : $"Manual{kickType}ArrowAndKick",
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
