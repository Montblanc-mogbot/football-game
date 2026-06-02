using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns task/game-status startup and teardown helpers that Bank19_20 uses around the live on-field loop.
/// </summary>
public sealed class TaskCoordinationService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.END_SPECIFIC_TASKS,
        OnFieldRoutine.SET_GAME_STATUS_ON_FIELD_START_PLAYER_TASK,
    ];

    public void StartOnFieldTasks(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.SET_GAME_STATUS_ON_FIELD_START_PLAYER_TASK);
        state.RecordEvent("Started on-field tasks and marked the game state as on-field.");
    }

    public void EndSpecificTasks(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.END_SPECIFIC_TASKS);
        state.RecordEvent("Ended the Bank19_20-specific on-field tasks for the current play.");
    }
}
