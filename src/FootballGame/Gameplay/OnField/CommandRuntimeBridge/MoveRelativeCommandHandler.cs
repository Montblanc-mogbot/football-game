using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2547-2570.
/// Handles the bounded relative-movement command family.
/// </summary>
public sealed class MoveRelativeCommandHandler : IMovementCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "MoveRelativeCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int relativeY = GetIntOperand(context.CommandDefinition, "relativeY", 0);
        int rawRelativeX = GetIntOperand(context.CommandDefinition, "relativeX", 0);
        bool invertXForPlayerTwo = GetBoolOperand(context.CommandDefinition, "invertXForPlayerTwo", false);
        bool isPlayerTwo = GetBoolOperand(context.CommandDefinition, "isPlayerTwo", false);
        bool appliedPlayerTwoXInversion = invertXForPlayerTwo && isPlayerTwo;
        int resolvedRelativeX = appliedPlayerTwoXInversion ? -rawRelativeX : rawRelativeX;

        return new PlayerCommandHandlerResult
        {
            Summary = $"Captured the Bank21_22 relative-move target ({resolvedRelativeX}, {relativeY}), queued the facing/speed refresh, and left the command awaiting its move-until-arrival continuation loop.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = new MovementCommandState
            {
                CommandKind = "RelativeMove",
                RelativeX = resolvedRelativeX,
                RelativeY = relativeY,
                AppliedPlayerTwoXInversion = appliedPlayerTwoXInversion,
                QueuedDirectionUpdate = true,
                QueuedVelocityInitialization = true,
                AwaitingArrivalLoop = true,
            },
            ControlFlowState = null,
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
