using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3609-3760.
/// Handles the bounded field-goal kick command family.
/// </summary>
public sealed class KickFieldGoalCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "KickFieldGoalCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return CreateKickResult(context, kickType: "FieldGoal", manualKickDefault: false);
    }

    internal static PlayerCommandHandlerResult CreateKickResult(PlayerCommandHandlerContext context, string kickType, bool manualKickDefault)
    {
        bool manualKick = GetBoolOperand(context.CommandDefinition, "manualKick", manualKickDefault);
        int computerDelayFrames = GetIntOperand(context.CommandDefinition, "computerDelayFrames", 10);

        return new PlayerCommandHandlerResult
        {
            Summary = manualKick
                ? $"Captured the Bank21_22 {kickType} flow through kicker-skill setup, holder-collision wait, and ball-approach kickoff, then left the kick command awaiting manual input and the downstream kick/block outcome path."
                : $"Captured the Bank21_22 {kickType} flow through kicker-skill setup, holder-collision wait, and ball-approach kickoff, preserving the bounded computer kick delay ({computerDelayFrames} frames) before the kick/block outcome path continues.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = new SpecialTeamsCommandState
            {
                CommandKind = kickType == "ExtraPoint" ? "KickExtraPoint" : "KickFieldGoal",
                KickType = kickType,
                ReturnType = null,
                WaitsForBallKicked = false,
                WaitsForBallSnapped = true,
                WaitsForBallCollision = true,
                WaitsForManualKickInput = manualKick,
                WaitsForComputerKickDelay = !manualKick,
                SetsPlayerLocationRelativeToSnap = false,
                MovesRelativeToFinalBallLanding = false,
                SetsManualControlToReturner = false,
                AssignsBallCarrierOnCatch = false,
                StartsKickAttempt = false,
                StartsPuntAttempt = false,
                StartsFieldGoalAttempt = true,
                HoldsPostKickDelay = !manualKick,
                PostKickDelayFrames = manualKick ? null : computerDelayFrames,
                RelativeX = null,
                RelativeY = null,
                AppliedPlayerTwoXInversion = false,
                TargetPlayerSlot = "KICKER_STARTER_ID",
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

    private static int GetIntOperand(PlayerCommandDefinition commandDefinition, string key, int defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            && int.TryParse(value, out int parsedValue)
            ? parsedValue
            : defaultValue;
    }
}
