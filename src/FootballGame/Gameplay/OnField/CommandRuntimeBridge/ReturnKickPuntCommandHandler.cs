using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:4289-4341.
/// Handles the bounded kickoff/punt returner receive command family.
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

        bool kickoffReturn = GetBoolOperand(context.CommandDefinition, "kickoffReturn", true);
        string targetPlayerSlot = GetStringOperand(context.CommandDefinition, "targetPlayerSlot", kickoffReturn ? "KR_STARTER_ID" : "PR_STARTER_ID");

        return new PlayerCommandHandlerResult
        {
            Summary = kickoffReturn
                ? $"Captured the Bank21_22 kickoff-return receive flow for '{targetPlayerSlot}', retargeting the displayed-name/manual-control seam to the returner, waiting for ball-kicked and ball-collision gates, then assigning ball-carrier ownership on the catch."
                : $"Captured the Bank21_22 punt-return receive flow for '{targetPlayerSlot}', retargeting the displayed-name/manual-control seam to the returner, waiting for ball-kicked and ball-collision gates, then assigning ball-carrier ownership on the catch.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = new SpecialTeamsCommandState
            {
                CommandKind = "ReturnKickPunt",
                KickType = null,
                ReturnType = kickoffReturn ? "KickoffReturn" : "PuntReturn",
                WaitsForBallKicked = true,
                WaitsForBallSnapped = false,
                WaitsForBallCollision = true,
                WaitsForManualKickInput = false,
                WaitsForComputerKickDelay = false,
                SetsPlayerLocationRelativeToSnap = false,
                MovesRelativeToFinalBallLanding = false,
                SetsManualControlToReturner = true,
                AssignsBallCarrierOnCatch = true,
                StartsKickAttempt = false,
                StartsPuntAttempt = false,
                StartsFieldGoalAttempt = false,
                HoldsPostKickDelay = false,
                PostKickDelayFrames = null,
                RelativeX = null,
                RelativeY = null,
                AppliedPlayerTwoXInversion = false,
                TargetPlayerSlot = targetPlayerSlot,
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

    private static string GetStringOperand(PlayerCommandDefinition commandDefinition, string key, string defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : defaultValue;
    }
}
