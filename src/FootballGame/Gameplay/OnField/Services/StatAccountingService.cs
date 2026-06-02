using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns play distance calculation and per-play stat accounting represented inside Bank19_20.
/// </summary>
public sealed class StatAccountingService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.UPDATE_STATS,
        OnFieldRoutine.CALCULATE_PLAY_DISTANCE,
    ];

    public void UpdateInGameStats(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.UPDATE_STATS);
        state.RecordEvent("Updated the in-game stats for the completed play.");
    }

    public void CalculatePlayDistance(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.CALCULATE_PLAY_DISTANCE);
        state.RecordEvent("Calculated the completed play distance from LOS to the final ball spot.");
    }

    public void ResetSeriesAfterTurnover(OnFieldGameState state, OnFieldTeam newPossessionTeam)
    {
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_FIRST_DOWN_OR_TOD);
        state.RecordEvent($"Reset down-and-distance state after turnover to {newPossessionTeam} possession.");
    }

    public void SpotBallAndUpdateHashForNextSnap(OnFieldGameState state, OnFieldTeam newPossessionTeam)
    {
        state.RecordRoutine(OnFieldRoutine.UPDATE_HASHMARK_FOR_NEXT_SNAP);

        if (state.PendingNextSnapYardLine is int pendingYardLine)
        {
            state.RecordEvent($"Spotted the ball at the source-directed {pendingYardLine}-yard line and updated the hashmark for the next {newPossessionTeam} snap.");
            state.PendingNextSnapYardLine = null;
            return;
        }

        state.RecordEvent($"Spotted the ball and updated the hashmark for the next {newPossessionTeam} snap.");
    }
}
