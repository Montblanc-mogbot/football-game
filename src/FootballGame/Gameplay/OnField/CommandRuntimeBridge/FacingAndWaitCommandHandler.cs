using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:404-407, 2395-2457.
/// Handles the bounded face/stand/turn/wait family.
/// </summary>
public sealed class FacingAndWaitCommandHandler : IPlayerPresentationCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "FaceLineOfScrimmageCommand" or "StandCommand" or "TurnCommand" or "WaitCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string commandKind = context.CommandDefinition.CommandName;
        int minimumFrames = GetIntOperand(context.CommandDefinition, "minimumFrames", 0);
        int maximumFrames = GetIntOperand(context.CommandDefinition, "maximumFrames", minimumFrames);

        return new PlayerCommandHandlerResult
        {
            Summary = $"Captured the Bank21_22 {commandKind} presentation wait so the runtime now records facing/idle semantics, explicit timed waits, and sprite/velocity updates through the existing host/runtime seam.",
            AwaitingContinuation = commandKind is "TurnCommand" or "WaitCommand",
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            PlayerPresentationCommandState = new PlayerPresentationCommandState
            {
                CommandKind = commandKind,
                FacingDirectionKind = commandKind == "FaceLineOfScrimmageCommand" ? "LineOfScrimmage" : commandKind == "TurnCommand" ? "TimedTurn" : null,
                WaitFramesMinimum = minimumFrames,
                WaitFramesMaximum = maximumFrames,
                QueuedVelocityZeroing = commandKind is "StandCommand" or "WaitCommand",
                QueuedStandingSpriteUpdate = true,
                QueuedFacingReset = commandKind == "FaceLineOfScrimmageCommand",
                QueuedDirectionUpdate = commandKind == "TurnCommand",
                AwaitingContinuation = commandKind is "TurnCommand" or "WaitCommand",
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
