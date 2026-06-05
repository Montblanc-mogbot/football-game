using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1957-1994.
/// Handles the bounded CPU quarterback wait-to-pass command family.
/// </summary>
public sealed class CpuWaitToPassCommandHandler : IQuarterbackPassCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "CpuWaitToPassCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int waitFrames = GetIntOperand(context.CommandDefinition, "waitFrames", 0);
        int takeSackChance = GetIntOperand(context.CommandDefinition, "takeSackChance", 0);
        bool throwWhenCollisionThreatened = GetBoolOperand(context.CommandDefinition, "throwWhenCollisionThreatened", true);

        return new PlayerCommandHandlerResult
        {
            Summary = throwWhenCollisionThreatened
                ? $"Started the CPU wait-to-pass timer ({waitFrames} frames), preserving the source branch that throws early if nearby collision pressure appears before the timer expires."
                : $"Started the CPU wait-to-pass timer ({waitFrames} frames) and left the quarterback waiting strictly for the timer to expire.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            QuarterbackPassCommandState = new QuarterbackPassCommandState
            {
                CommandKind = "CpuWaitToPass",
                QuarterbackHasBall = true,
                AwaitingContinuation = true,
                QueuedDirectionUpdate = false,
                QueuedVelocityInitialization = false,
                CyclingAnimationFrames = false,
                ExitOnBackOfEndZone = false,
                RelativeDropbackX = null,
                TargetY = null,
                AppliedPlayerTwoXInversion = false,
                WaitFrames = waitFrames,
                ThrowsEarlyWhenCollisionThreatened = throwWhenCollisionThreatened,
                TakeSackChanceThreshold = takeSackChance,
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
