using System.Collections.Generic;

using FootballGame.Gameplay.OnField;
using FootballGame.Gameplay.OnField.CommandRuntimeBridge;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns defender switching, snap gating, and other pre-snap control-side helpers that remain inside Bank19_20.
/// </summary>
public sealed class PreSnapControlService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE,
        OnFieldRoutine.CHECK_SNAP_PUNT,
        OnFieldRoutine.MAN_CONTROLLED_PLAYER_FUNCTIONS,
    ];

    public void PrepareRegularPlayForSnap(OnFieldGameState state, OnFieldTeam offenseTeam)
    {
        state.RecordRoutine(OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE);
        state.Phase = OnFieldPhase.PreSnap;
        state.BallSnapped = false;
        QueueRuntimeRequest(state, OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE, "ACTIVE_DEFENDER", "DEFENDER_PRE_SNAP_CONTROL");
        state.RecordEvent($"Prepared defender-change and snap-gating flow for {offenseTeam} before the hike.");
    }

    public void PreparePuntForSnap(OnFieldGameState state, OnFieldTeam puntingTeam)
    {
        state.RecordRoutine(OnFieldRoutine.CHECK_SNAP_PUNT);
        state.Phase = OnFieldPhase.PreSnap;
        state.BallSnapped = false;
        QueueRuntimeRequest(state, OnFieldRoutine.CHECK_SNAP_PUNT, "PUNT_SNAP_GROUP", "PUNT_PRE_SNAP_CONTROL");
        state.RecordEvent($"Prepared punt snap gate for {puntingTeam} before the kick.");
    }

    public void MarkBallSnapped(OnFieldGameState state)
    {
        state.BallSnapped = true;
        state.RecordEvent("Marked the host-side snapped-ball state so Bank21_22 command stepping can proceed.");
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

            if (triggerRoutine == OnFieldRoutine.CHECK_SNAP_PUNT && hostRequest.LiveCommandNameOverride is not null)
            {
                bool wantsPunt = state.PlayType == OnFieldPlayType.Punt && hostRequest.LiveCommandNameOverride == "PuntCommand";
                bool wantsFieldGoal = state.PlayType == OnFieldPlayType.FieldGoal && hostRequest.LiveCommandNameOverride == "FieldGoalKickCommand";
                bool wantsExtraPoint = state.PlayType == OnFieldPlayType.ExtraPoint && hostRequest.LiveCommandNameOverride == "ExtraPointKickCommand";
                if (!wantsPunt && !wantsFieldGoal && !wantsExtraPoint)
                {
                    continue;
                }
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
