using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1762-1890.
/// Handles the bounded kickoff coverage setup / move-relative-to-ball command family.
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
        int relativeY = GetIntOperand(context.CommandDefinition, "relativeY", 0);
        int rawRelativeX = GetIntOperand(context.CommandDefinition, "relativeX", 0);
        bool invertXForPlayerTwo = GetBoolOperand(context.CommandDefinition, "invertXForPlayerTwo", false);
        bool isPlayerTwo = GetBoolOperand(context.CommandDefinition, "isPlayerTwo", false);
        bool appliedPlayerTwoXInversion = invertXForPlayerTwo && isPlayerTwo;
        int resolvedRelativeX = appliedPlayerTwoXInversion ? -rawRelativeX : rawRelativeX;

        return new PlayerCommandHandlerResult
        {
            Summary = moveDuringKickoff
                ? $"Captured the Bank21_22 kickoff coverage move-relative-to-ball setup ({resolvedRelativeX}, {relativeY}), waited for the ball-kicked gate, then queued the shared direction/speed refresh and arrival loop toward the final landing-relative coverage spot."
                : $"Captured the Bank21_22 kickoff pre-kick snap-relative alignment ({resolvedRelativeX}, {relativeY}) so the host/runtime seam can preserve the source-visible coverage starting location without inventing a parallel setup path.",
            AwaitingContinuation = moveDuringKickoff,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = new SpecialTeamsCommandState
            {
                CommandKind = "SetAndMoveKickoff",
                KickType = "Kickoff",
                ReturnType = null,
                WaitsForBallKicked = moveDuringKickoff,
                WaitsForBallSnapped = false,
                WaitsForBallCollision = false,
                WaitsForManualKickInput = false,
                WaitsForComputerKickDelay = false,
                SetsPlayerLocationRelativeToSnap = !moveDuringKickoff,
                MovesRelativeToFinalBallLanding = moveDuringKickoff,
                SetsManualControlToReturner = false,
                AssignsBallCarrierOnCatch = false,
                StartsKickAttempt = false,
                StartsPuntAttempt = false,
                StartsFieldGoalAttempt = false,
                HoldsPostKickDelay = false,
                PostKickDelayFrames = null,
                RelativeX = resolvedRelativeX,
                RelativeY = relativeY,
                AppliedPlayerTwoXInversion = appliedPlayerTwoXInversion,
                TargetPlayerSlot = null,
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
