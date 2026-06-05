using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:394-398, 2395-2536.
/// Handles the bounded pre-snap stance / formation-shift family.
/// </summary>
public sealed class PreSnapStanceCommandHandler : IPlayerPresentationCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is
            "ThreePointStanceCommand"
            or "FormationShiftCommand"
            or "TwoPointStanceCommand"
            or "OffMotionCommand"
            or "QuarterbackPreSnapStanceCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string commandKind = context.CommandDefinition.CommandName;
        bool waitsForSnap = GetBoolOperand(context.CommandDefinition, "waitForSnap", true);

        return new PlayerCommandHandlerResult
        {
            Summary = $"Captured the Bank21_22 {commandKind} posture family on the existing seam so host-owned stance/facing presentation can be applied explicitly while the runtime keeps the snap-gated wait semantics source-visible.",
            AwaitingContinuation = waitsForSnap,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            PlayerPresentationCommandState = new PlayerPresentationCommandState
            {
                CommandKind = commandKind,
                StanceKind = commandKind,
                WaitsForBallSnapExit = waitsForSnap,
                QueuedVelocityZeroing = true,
                QueuedStandingSpriteUpdate = true,
                AwaitingContinuation = waitsForSnap,
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
}
