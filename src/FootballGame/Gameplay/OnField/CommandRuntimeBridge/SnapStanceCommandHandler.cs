using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:191-199, 394-398, 4498-4598.
/// Handles the bounded snap-gated stance family.
/// </summary>
public sealed class SnapStanceCommandHandler : IPlayerPresentationCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "CenterHikeCommand" or "ShotgunHikeCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string commandKind = context.CommandDefinition.CommandName;
        int waitFrames = GetIntOperand(context.CommandDefinition, "postSnapDelayFrames", commandKind == "ShotgunHikeCommand" ? 30 : 4);

        return new PlayerCommandHandlerResult
        {
            Summary = commandKind == "ShotgunHikeCommand"
                ? "Captured the Bank21_22 shotgun-hike posture gate so the runtime now records the long-snap release, waits on the host snap gate, and preserves the post-snap delay without inventing a second snap path."
                : "Captured the Bank21_22 center-hike posture gate so the runtime now records the under-center snap stance, waits on the host snap gate, and preserves the short post-snap delay on the existing seam.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            PlayerPresentationCommandState = new PlayerPresentationCommandState
            {
                CommandKind = commandKind,
                StanceKind = commandKind == "ShotgunHikeCommand" ? "ShotgunSnap" : "CenterSnap",
                WaitFrames = waitFrames,
                WaitsForBallSnapExit = true,
                QueuedStandingSpriteUpdate = true,
                AwaitingContinuation = true,
            },
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }

    private static int GetIntOperand(PlayerCommandDefinition commandDefinition, string key, int defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            && int.TryParse(value, out int parsedValue)
            ? parsedValue
            : defaultValue;
    }
}
