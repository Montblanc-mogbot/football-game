using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2606-2608.
/// Handles the bounded move-vs-middle-of-field command family.
/// </summary>
public sealed class MoveAbsoluteVsMiddleCommandHandler : IMovementCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "MoveAbsoluteVsMiddleCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int relativeYToMiddle = GetIntOperand(context.CommandDefinition, "relativeYToMiddle", 0);
        int middleOfFieldY = GetIntOperand(context.CommandDefinition, "middleOfFieldY", 0);
        int rawRelativeX = GetIntOperand(context.CommandDefinition, "relativeX", 0);
        bool invertXForPlayerTwo = GetBoolOperand(context.CommandDefinition, "invertXForPlayerTwo", false);
        bool isPlayerTwo = GetBoolOperand(context.CommandDefinition, "isPlayerTwo", false);
        bool appliedPlayerTwoXInversion = invertXForPlayerTwo && isPlayerTwo;
        int resolvedRelativeX = appliedPlayerTwoXInversion ? -rawRelativeX : rawRelativeX;
        int absoluteTargetY = middleOfFieldY + relativeYToMiddle;

        return new PlayerCommandHandlerResult
        {
            Summary = $"Translated the Bank21_22 move-vs-middle target into an absolute Y destination ({absoluteTargetY}) while reusing the shared absolute-move setup path and arrival loop.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = new MovementCommandState
            {
                CommandKind = "AbsoluteMoveVsMiddle",
                AnchorKind = "FieldMiddle",
                RelativeX = resolvedRelativeX,
                RelativeY = relativeYToMiddle,
                AbsoluteTargetX = null,
                AbsoluteTargetY = absoluteTargetY,
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
