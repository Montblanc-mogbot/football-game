using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1652-1704.
/// Handles the bounded computer-pass target-selection / throw-start command family.
/// </summary>
public sealed class ComputerPassCommandHandler : IQuarterbackPassCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ComputerPassCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int targetReceiverCount = GetIntOperand(context.CommandDefinition, "targetReceiverCount", 0);
        int selectedTargetPriorityIndex = GetIntOperand(context.CommandDefinition, "selectedTargetPriorityIndex", 0);
        string selectedTargetPlayerSlot = GetStringOperand(context.CommandDefinition, "selectedTargetPlayerSlot", "PRIMARY_RECEIVER");
        int postThrowDelayFrames = GetIntOperand(context.CommandDefinition, "postThrowDelayFrames", 8);
        bool quarterbackHasBall = GetBoolOperand(context.CommandDefinition, "quarterbackHasBall", true);

        return new PlayerCommandHandlerResult
        {
            Summary = quarterbackHasBall
                ? $"Resolved the Bank21_22 computer-pass target order across {targetReceiverCount} eligible receivers, selected priority {selectedTargetPriorityIndex} ('{selectedTargetPlayerSlot}'), started the pass-attempt routine, and queued the source-visible {postThrowDelayFrames}-frame post-throw delay before normal stepping resumes."
                : "Left the computer-pass command parked for one frame because the quarterback does not currently hold the ball.",
            AwaitingContinuation = quarterbackHasBall,
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
                CommandKind = "ComputerPass",
                RelativeDropbackX = null,
                TargetY = null,
                AppliedPlayerTwoXInversion = false,
                QueuedDirectionUpdate = false,
                QueuedVelocityInitialization = false,
                AwaitingContinuation = quarterbackHasBall,
                AnimationToggleFrames = null,
                WaitingFrames = null,
                WaitsForNearbyPressure = false,
                SackWindowEnabled = false,
                SackChanceThreshold = null,
                TargetReceiverCount = targetReceiverCount,
                SelectedTargetPriorityIndex = selectedTargetPriorityIndex,
                SelectedTargetPlayerSlot = selectedTargetPlayerSlot,
                StartedPassAttempt = quarterbackHasBall,
                QueuedPostThrowDelay = quarterbackHasBall,
                PostThrowDelayFrames = postThrowDelayFrames,
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

    private static string GetStringOperand(PlayerCommandDefinition commandDefinition, string key, string defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : defaultValue;
    }
}
