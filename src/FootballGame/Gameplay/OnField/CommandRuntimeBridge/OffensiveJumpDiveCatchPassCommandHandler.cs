using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:4759-4911.
/// Handles the bounded packet-21C offensive receiver jump/dive contest setup before the miss/interception continuation window.
/// </summary>
public sealed class OffensiveJumpDiveCatchPassCommandHandler : IPassContestCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "OffensiveJumpDiveCatchPassCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        PassContestCommandState contestState = new()
        {
            ReceiverJumpOrDiveAttemptResolved = false,
            DefenderOnlyInterceptionWindowActive = false,
            RankedDefenderWindowSize = context.CommandDefinition.OperandValues.TryGetValue("rankedDefenderWindowSize", out string? rankedWindow)
                && int.TryParse(rankedWindow, out int parsedWindow)
                ? parsedWindow
                : 3,
            PreserveSourceBugByPolicy = context.CommandDefinition.OperandValues.TryGetValue("bugPolicy", out string? bugPolicy)
                && string.Equals(bugPolicy, "PreserveExplicitlyUntilParityDecision", StringComparison.Ordinal),
            ResolutionStage = "OffensiveJumpDiveCatchSetup",
        };

        PlayerCommandRetargetRequest[] retargetRequests =
        [
            new PlayerCommandRetargetRequest
            {
                SourcePlayerSlotKey = context.PlayerSlotKey,
                TargetPlayerSlotKey = context.PlayerSlotKey,
                ContinuationCommandName = "ReceiverMissedBallInterceptionWindowCommand",
                ContinuationSourceLabel = "CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH",
                Reason = "Packet 21C bounded live continuation: after the receiver jump/dive setup, reuse the existing explicit miss/interception-window command so the pass-contest seam keeps progressing through the source-visible miss branch.",
                SkipIfTargetInvalid = false,
            },
        ];

        return new PlayerCommandHandlerResult
        {
            Summary = "Primed the receiver jump/dive catch contest and emitted an explicit same-player continuation into the receiver-miss interception window.",
            AwaitingContinuation = true,
            RetargetRequests = retargetRequests,
            DefensiveReactionState = null,
            PassContestState = contestState,
            OffensiveExchangeState = null,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
