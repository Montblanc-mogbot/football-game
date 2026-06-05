using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:1592-1631.
/// Handles the bounded pre-snap motion defender-follow family.
/// </summary>
public sealed class PreSnapMotionCommandHandler : IPreSnapCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "PreSnapMotionCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string followTarget = context.CommandDefinition.OperandValues.TryGetValue("followTargetPlayerSlot", out string? target)
            ? target
            : "MOTION_RECEIVER";
        int followDelayFrames = GetIntOperand(context.CommandDefinition, "followDelayFrames", 9);
        int nearThreshold = GetIntOperand(context.CommandDefinition, "nearMotionPlayerYThreshold", 10);

        return new PlayerCommandHandlerResult
        {
            Summary = $"Stored the mirrored motion target '{followTarget}', waited {followDelayFrames} pre-snap frames between checks, and left the defender in the Bank21_22 follow-motion loop until the snap or near-alignment exit condition fires.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = null,
            PlayerControlCommandState = null,
            ControlFlowState = null,
            PointerOverride = null,
            SourceNotes = context.CommandDefinition.SourceNotes,
            PreSnapCommandState = new PreSnapCommandState
            {
                CommandKind = "PreSnapMotionFollow",
                FollowTargetPlayerSlot = followTarget,
                FollowDelayFrames = followDelayFrames,
                NearMotionPlayerYThreshold = nearThreshold,
                WaitsForBallSnapExit = true,
                StopsWhenAlignedWithinThreshold = true,
                QueuedFacingResetWhenAligned = true,
                QueuedVelocityInitializationWhileFollowing = true,
                AwaitingFollowLoopContinuation = true,
            },
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
