using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1893-1955.
/// Handles the bounded quarterback dropback command family.
/// </summary>
public sealed class QuarterbackDropbackCommandHandler : IQuarterbackPassCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "QuarterbackDropbackCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int targetY = GetIntOperand(context.CommandDefinition, "targetY", 0);
        int rawDropbackX = GetIntOperand(context.CommandDefinition, "dropbackX", 0);
        bool invertXForPlayerTwo = GetBoolOperand(context.CommandDefinition, "invertXForPlayerTwo", true);
        bool isPlayerTwo = GetBoolOperand(context.CommandDefinition, "isPlayerTwo", false);
        bool appliedPlayerTwoXInversion = invertXForPlayerTwo && isPlayerTwo;
        int resolvedDropbackX = appliedPlayerTwoXInversion ? -rawDropbackX : rawDropbackX;
        int animationDelayFrames = GetIntOperand(context.CommandDefinition, "animationDelayFrames", 12);

        return new PlayerCommandHandlerResult
        {
            Summary = $"Captured the quarterback dropback target ({resolvedDropbackX}, {targetY}), queued the source-visible facing/speed refresh, and left the command cycling its alternating feet animation until the quarterback reaches the back-of-end-zone-safe final spot.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            QuarterbackPassCommandState = new QuarterbackPassCommandState
            {
                CommandKind = "QuarterbackDropback",
                QuarterbackHasBall = true,
                AwaitingContinuation = true,
                QueuedDirectionUpdate = true,
                QueuedVelocityInitialization = true,
                CyclingAnimationFrames = true,
                ExitOnBackOfEndZone = true,
                RelativeDropbackX = resolvedDropbackX,
                TargetY = targetY,
                AppliedPlayerTwoXInversion = appliedPlayerTwoXInversion,
                WaitFrames = animationDelayFrames,
                ThrowsEarlyWhenCollisionThreatened = false,
                TakeSackChanceThreshold = null,
                TargetCount = null,
                SelectedTargetPlayerSlot = null,
                StartedPassAttempt = false,
                PostPassDelayFrames = null,
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
