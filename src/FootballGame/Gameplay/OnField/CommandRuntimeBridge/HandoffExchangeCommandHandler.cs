using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:7748-7848.
/// Handles the immediate post-snap regular/fake handoff family from packet 21A.
/// </summary>
public sealed class HandoffExchangeCommandHandler : IOffensiveExchangeCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "BackfieldHandoffCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool fakeExchange = context.CommandDefinition.OperandValues.TryGetValue("fakeExchange", out string? fakeExchangeValue)
            && bool.TryParse(fakeExchangeValue, out bool parsedFakeExchange)
            && parsedFakeExchange;
        string targetPlayerSlot = context.CommandDefinition.OperandValues.TryGetValue("targetPlayerSlot", out string? targetSlot)
            ? targetSlot
            : "TARGET_BACK";
        string continuationCommand = fakeExchange
            ? "RunnerFakeHandoffAnimationCommand"
            : "RunnerReceiveHandoffCommand";
        string continuationSourceLabel = fakeExchange
            ? "RB_FAKE_HANDOFF_ANIMATION"
            : "RB_RECEIVES_HANDOFF_START";

        OffensiveExchangeCommandState exchangeState = new()
        {
            ExchangeKind = fakeExchange ? "FakeHandoff" : "Handoff",
            QuarterbackStoppedForExchange = true,
            HandoffOrPitchIconTimerStarted = true,
            QuarterbackReleasedBallCarrierState = !fakeExchange,
            RetargetedPlayerSlot = targetPlayerSlot,
            RetargetedContinuationCommand = continuationCommand,
            RetargetSkippedBecauseTargetInvalid = false,
            FakeExchange = fakeExchange,
            InFlightBallStateCreated = false,
            ContinuationStage = fakeExchange ? "QuarterbackFakeExchangeDelay" : "QuarterbackHandoffDelay",
            PostExchangeDelayFrames = fakeExchange ? 26 : 26,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = fakeExchange
                ? "Stopped the quarterback, started the fake-exchange icon timing, and explicitly retargeted the runner into the fake-handoff continuation."
                : "Stopped the quarterback, dropped quarterback ball-carrier ownership, and explicitly retargeted the runner into the receive-handoff continuation.",
            AwaitingContinuation = true,
            RetargetRequests =
            [
                new PlayerCommandRetargetRequest
                {
                    SourcePlayerSlotKey = context.PlayerSlotKey,
                    TargetPlayerSlotKey = targetPlayerSlot,
                    ContinuationCommandName = continuationCommand,
                    ContinuationSourceLabel = continuationSourceLabel,
                    Reason = fakeExchange
                        ? "Packet 21A fake handoff retarget via UPDATE_LOCAL_PLAYER_COMMAND_ADDR_IF_VALID."
                        : "Packet 21A handoff retarget via UPDATE_LOCAL_PLAYER_COMMAND_ADDR_IF_VALID.",
                    SkipIfTargetInvalid = true,
                },
            ],
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = exchangeState,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
