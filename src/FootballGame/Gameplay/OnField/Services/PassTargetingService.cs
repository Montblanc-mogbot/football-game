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

    public void QueueQuarterbackPassControl(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS);
        QueueRuntimeRequest(state, OnFieldRoutine.UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS, "CPU_QUARTERBACK", "CPU_PASS_CONTROL");
        state.RecordEvent("Primed the host/runtime seam for the Bank21_22 QB pass-control family while updating the visible pass-target indicator timing.");
    }

    public void UpdatePassTargetIndicator(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS);

        if (state.CurrentPassTargetPriority.HasValue
            && state.PassTargets.TryGetValue(state.CurrentPassTargetPriority.Value, out string? currentTarget))
        {
            state.RecordEvent($"Updated the current pass target and target-indicator state to '{currentTarget}' (priority {state.CurrentPassTargetPriority.Value}).");
            return;
        }

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
        QueueRuntimeRequest(state, OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS, "PASS_CONTEST_GROUP", "PASS_CONTEST_GROUP");
    }

    private static void QueueRuntimeRequest(OnFieldGameState state, OnFieldRoutine triggerRoutine, string playerSlotKey, string scriptFamilyKey)
    {
        if (state.CommandRuntimeBoundary is null)
        {
            return;
        }

        foreach (PlayerCommandRuntimeHostRequest hostRequest in CommandRuntimeBoundaryHoldingArea.CreateHostRequests(state))
        {
            if (hostRequest.TriggerRoutine != triggerRoutine)
            {
                continue;
            }

            state.PendingCommandRuntimeRequests.Add(hostRequest);
            state.CommandRuntimeBoundary.PrimeExecutionContext(
                hostRequest,
                playerSlotKey,
                new PlayerCommandPointer
                {
                    ScriptFamilyKey = scriptFamilyKey,
                    InstructionOffset = 0,
                    ResumeLabel = hostRequest.BridgeSymbol,
                });
            return;
        }
    }
}
