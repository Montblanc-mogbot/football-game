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
}
