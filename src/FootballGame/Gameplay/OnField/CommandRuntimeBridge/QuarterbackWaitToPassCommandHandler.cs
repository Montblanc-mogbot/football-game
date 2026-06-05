using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1957-1993.
/// Handles the bounded quarterback pass-timing / sack-window command family.
/// </summary>
public sealed class QuarterbackWaitToPassCommandHandler : IQuarterbackPassCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "QuarterbackWaitToPassCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int waitFrames = GetIntOperand(context.CommandDefinition, "waitFrames", 0);
        int sackChanceThreshold = GetIntOperand(context.CommandDefinition, "sackChanceThreshold", 0);
        bool waitForNearbyPressure = GetBoolOperand(context.CommandDefinition, "waitForNearbyPressure", false);

        return new PlayerCommandHandlerResult
        {
            Summary = waitForNearbyPressure
                ? $"Installed the Bank21_22 QB pass timer for {waitFrames} frames and kept the quarterback in the throw-or-sack loop that exits early when nearby collision pressure arrives."
                : $"Installed the Bank21_22 QB pass timer for {waitFrames} frames and waited until it expired before allowing the next pass command step.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = null,
            PlayerControlCommandState = null,
            ControlFlowState = null,
            PreSnapCommandState = null,
            PassTargetOrderCommandState = null,
            QuarterbackPassCommandState = new QuarterbackPassCommandState
            {
                CommandKind = "QuarterbackWaitToPass",
                RelativeDropbackX = null,
                TargetY = null,
                AppliedPlayerTwoXInversion = false,
                QueuedDirectionUpdate = false,
                QueuedVelocityInitialization = false,
                AwaitingContinuation = true,
                AnimationToggleFrames = null,
                WaitingFrames = waitFrames,
                WaitsForNearbyPressure = waitForNearbyPressure,
                SackWindowEnabled = true,
                SackChanceThreshold = sackChanceThreshold,
                TargetReceiverCount = null,
                SelectedTargetPriorityIndex = null,
                SelectedTargetPlayerSlot = null,
                StartedPassAttempt = false,
                QueuedPostThrowDelay = false,
                PostThrowDelayFrames = null,
            },
            PointerOverride = null,
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
