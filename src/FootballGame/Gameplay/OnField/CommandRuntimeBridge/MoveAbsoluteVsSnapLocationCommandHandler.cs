using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2578-2595.
/// Handles the bounded move-vs-line-of-scrimmage command family.
/// </summary>
public sealed class MoveAbsoluteVsSnapLocationCommandHandler : IMovementCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "MoveAbsoluteVsSnapLocationCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int relativeYToSnap = GetIntOperand(context.CommandDefinition, "relativeYToSnap", 0);
        int lineOfScrimmageY = GetIntOperand(context.CommandDefinition, "lineOfScrimmageY", 0);
        int rawRelativeX = GetIntOperand(context.CommandDefinition, "relativeX", 0);
        int lineOfScrimmageX = GetIntOperand(context.CommandDefinition, "lineOfScrimmageX", 0);
        bool invertXForPlayerTwo = GetBoolOperand(context.CommandDefinition, "invertXForPlayerTwo", false);
        bool isPlayerTwo = GetBoolOperand(context.CommandDefinition, "isPlayerTwo", false);
        bool appliedPlayerTwoXInversion = invertXForPlayerTwo && isPlayerTwo;
        int resolvedRelativeX = appliedPlayerTwoXInversion ? -rawRelativeX : rawRelativeX;
        int absoluteTargetY = lineOfScrimmageY + relativeYToSnap;
        int absoluteTargetX = lineOfScrimmageX + resolvedRelativeX;

        return new PlayerCommandHandlerResult
        {
            Summary = $"Translated the Bank21_22 move-vs-snap target into an absolute destination ({absoluteTargetX}, {absoluteTargetY}), then queued the shared facing/speed refresh and move-until-arrival loop.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = new MovementCommandState
            {
                CommandKind = "AbsoluteMoveVsSnapLocation",
                AnchorKind = "LineOfScrimmage",
                RelativeX = resolvedRelativeX,
                RelativeY = relativeYToSnap,
                AbsoluteTargetX = absoluteTargetX,
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
