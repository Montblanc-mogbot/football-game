using System.Collections.Generic;

using FootballGame.Gameplay.OnField;
using FootballGame.Gameplay.OnField.CommandRuntimeBridge;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns pass-target ranking, defender proximity ordering, and related Bank19_20 pass-contest setup helpers.
/// </summary>
public sealed class PassTargetingService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS,
        OnFieldRoutine.UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS,
    ];

    public void UpdatePassTargetIndicator(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS);
        state.RecordEvent("Updated the current pass target and target-indicator state.");
    }

    public void OrderPassCollisionPlayers(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS);
        QueueRuntimeRequest(state);
        state.RecordEvent("Ordered receiver/defender pass-collision candidates for the current pass attempt.");
    }

    private static void QueueRuntimeRequest(OnFieldGameState state)
    {
        if (state.CommandRuntimeBoundary is null)
        {
            return;
        }

        foreach (PlayerCommandRuntimeHostRequest hostRequest in CommandRuntimeBoundaryHoldingArea.CreateHostRequests(state))
        {
            if (hostRequest.TriggerRoutine != OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS)
            {
                continue;
            }

            state.PendingCommandRuntimeRequests.Add(hostRequest);
            state.CommandRuntimeBoundary.PrimeExecutionContext(
                hostRequest,
                "PASS_CONTEST_GROUP",
                new PlayerCommandPointer
                {
                    ScriptFamilyKey = hostRequest.BridgeSymbol ?? "PASS_CONTEST_GROUP",
                    InstructionOffset = 0,
                    ResumeLabel = hostRequest.BridgeSymbol,
                });
            return;
        }
    }
}
