using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1893-1954.
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

        int relativeDropbackX = GetIntOperand(context.CommandDefinition, "relativeDropbackX", 0);
        int targetY = GetIntOperand(context.CommandDefinition, "targetY", 0);
        bool invertXForPlayerTwo = GetBoolOperand(context.CommandDefinition, "invertXForPlayerTwo", true);
        bool isPlayerTwo = GetBoolOperand(context.CommandDefinition, "isPlayerTwo", false);
        int animationToggleFrames = GetIntOperand(context.CommandDefinition, "animationToggleFrames", 12);
        bool appliedPlayerTwoXInversion = invertXForPlayerTwo && isPlayerTwo;
        int resolvedRelativeDropbackX = appliedPlayerTwoXInversion ? -relativeDropbackX : relativeDropbackX;

        return new PlayerCommandHandlerResult
        {
            Summary = $"Captured the Bank21_22 QB dropback target (x {resolvedRelativeDropbackX}, y {targetY}), queued direction/speed refresh, and left the quarterback in the alternating dropback animation loop until the final spot or end-zone stop exits.",
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
                CommandKind = "QuarterbackDropback",
                RelativeDropbackX = resolvedRelativeDropbackX,
                TargetY = targetY,
                AppliedPlayerTwoXInversion = appliedPlayerTwoXInversion,
                QueuedDirectionUpdate = true,
                QueuedVelocityInitialization = true,
                AwaitingContinuation = true,
                AnimationToggleFrames = animationToggleFrames,
                WaitingFrames = null,
                WaitsForNearbyPressure = false,
                SackWindowEnabled = false,
                SackChanceThreshold = null,
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
