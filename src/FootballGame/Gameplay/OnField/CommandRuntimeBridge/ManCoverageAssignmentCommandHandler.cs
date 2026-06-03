using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1462-1476.
/// Handles the bounded man-coverage assignment family documented in packet 21B.
/// </summary>
public sealed class ManCoverageAssignmentCommandHandler : IDefensiveReactionCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ManCoverageAssignmentCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool looseCoverage = ContainsSourceNote(context.CommandDefinition, "loose-coverage high bit");
        DefensiveReactionCommandState reactionState = new()
        {
            CoverageTargetPlayerSlot = context.CommandDefinition.OperandValues.TryGetValue("playerToDefend", out string? target) ? target : "TARGET_RECEIVER",
            CoverageTimeSelector = context.CommandDefinition.OperandValues.TryGetValue("defendTimeSelector", out string? defendTime) && int.TryParse(defendTime, out int parsedDefendTime)
                ? parsedDefendTime
                : null,
            LooseCoverageEnabled = looseCoverage,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = looseCoverage
                ? "Stored loose man-coverage target/time and handed off to the shared defender coverage loop."
                : "Stored tight man-coverage target/time and handed off to the shared defender coverage loop.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = reactionState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }

    private static bool ContainsSourceNote(PlayerCommandDefinition commandDefinition, string fragment)
    {
        foreach (string sourceNote in commandDefinition.SourceNotes)
        {
            if (sourceNote.Contains(fragment, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
