using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:7850-7930.
/// Handles the packet-21A pitch-launch command that explicitly retargets the target runner continuation.
/// </summary>
public sealed class PitchExchangeCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "PitchBallCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string targetPlayerSlot = context.CommandDefinition.OperandValues.TryGetValue("targetPlayerSlot", out string? targetSlot)
            ? targetSlot
            : "PITCH_TARGET";
        bool retargetSkipped = context.CommandDefinition.OperandValues.TryGetValue("retargetSkippedBecauseTargetInvalid", out string? skippedValue)
            && bool.TryParse(skippedValue, out bool parsedSkipped)
            && parsedSkipped;

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = "Pitch",
            QuarterbackStoppedForExchange = true,
            BallCarrierAssigned = true,
            InFlightBallStateCreated = true,
            QuarterbackReleasedBallCarrierState = true,
            HandoffOrPitchIconTimerStarted = false,
            RetargetedPlayerSlot = retargetSkipped ? null : targetPlayerSlot,
            RetargetedContinuationCommand = retargetSkipped ? null : "ReceivePitchContinuationCommand",
            RetargetSkippedBecauseTargetInvalid = retargetSkipped,
            FakeExchange = false,
            ContinuationStage = "PitchBallInFlight",
            PostExchangeDelayFrames = 20,
        };

        PlayerCommandRetargetRequest[] retargetRequests = retargetSkipped
            ? Array.Empty<PlayerCommandRetargetRequest>()
            :
            [
                new PlayerCommandRetargetRequest
                {
                    SourcePlayerSlotKey = context.PlayerSlotKey,
                    TargetPlayerSlotKey = targetPlayerSlot,
                    ContinuationCommandName = "ReceivePitchContinuationCommand",
                    ContinuationSourceLabel = "WAIT_FOR_PLAYER_RECEIVES_PITCH",
                    Reason = "Packet 21A pitch retarget via UPDATE_LOCAL_PLAYER_COMMAND_ADDR_IF_VALID after the ball leaves the passer's hand.",
                    SkipIfTargetInvalid = true,
                },
            ];

        return new PlayerCommandHandlerResult
        {
            Summary = retargetSkipped
                ? "Stopped the quarterback, created the in-flight pitch state, and kept the target retarget explicit while skipping it because the target was invalid/collided/on the ground."
                : "Stopped the quarterback, created the in-flight pitch state, cleared quarterback ball-carrier ownership, and explicitly retargeted the target runner into the receive-pitch continuation.",
            AwaitingContinuation = true,
            RetargetRequests = retargetRequests,
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
