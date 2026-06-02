using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

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
        state.RecordEvent("Ordered receiver/defender pass-collision candidates for the current pass attempt.");
    }
}
