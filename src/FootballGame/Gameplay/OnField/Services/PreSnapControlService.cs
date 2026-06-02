using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

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
        state.RecordEvent($"Prepared defender-change and snap-gating flow for {offenseTeam} before the hike.");
    }

    public void PreparePuntForSnap(OnFieldGameState state, OnFieldTeam puntingTeam)
    {
        state.RecordRoutine(OnFieldRoutine.CHECK_SNAP_PUNT);
        state.Phase = OnFieldPhase.PreSnap;
        state.RecordEvent($"Prepared punt snap gate for {puntingTeam} before the kick.");
    }
}
