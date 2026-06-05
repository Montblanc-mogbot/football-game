using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1652-1704.
/// Handles the bounded CPU pass-selection/pass-attempt command family.
/// </summary>
public sealed class CpuPassCommandHandler : IQuarterbackPassCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "CpuPassCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool quarterbackHasBall = GetBoolOperand(context.CommandDefinition, "quarterbackHasBall", true);
        int targetCount = GetIntOperand(context.CommandDefinition, "targetCount", 0);
        string selectedTarget = GetStringOperand(context.CommandDefinition, "selectedTargetPlayerSlot", "WR1");
        int postPassDelayFrames = GetIntOperand(context.CommandDefinition, "postPassDelayFrames", 8);

        return new PlayerCommandHandlerResult
        {
            Summary = quarterbackHasBall
                ? $"Selected the CPU pass target '{selectedTarget}' from {targetCount} scripted options, advanced past the packed receiver table, and started the source-visible pass-attempt branch with its {postPassDelayFrames}-frame post-throw hold."
                : "Held the CPU pass command on the quarterback until ball-carrier ownership returns, matching the source loop that keeps yielding one frame and retrying.",
            AwaitingContinuation = !quarterbackHasBall ? true : false,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            QuarterbackPassCommandState = new QuarterbackPassCommandState
            {
                CommandKind = "CpuPassCommand",
                QuarterbackHasBall = quarterbackHasBall,
                AwaitingContinuation = !quarterbackHasBall,
                QueuedDirectionUpdate = false,
                QueuedVelocityInitialization = false,
                CyclingAnimationFrames = false,
                ExitOnBackOfEndZone = false,
                RelativeDropbackX = null,
                TargetY = null,
                AppliedPlayerTwoXInversion = false,
                WaitFrames = null,
                ThrowsEarlyWhenCollisionThreatened = false,
                TakeSackChanceThreshold = null,
                TargetCount = targetCount,
                SelectedTargetPlayerSlot = selectedTarget,
                StartedPassAttempt = quarterbackHasBall,
                PostPassDelayFrames = postPassDelayFrames,
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

    private static string GetStringOperand(PlayerCommandDefinition commandDefinition, string key, string defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : defaultValue;
    }
}
