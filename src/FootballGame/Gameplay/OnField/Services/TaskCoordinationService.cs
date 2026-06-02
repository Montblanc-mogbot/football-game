using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns task/game-status startup and teardown helpers that Bank19_20 uses around the live on-field loop.
/// </summary>
public sealed class TaskCoordinationService
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.END_SPECIFIC_TASKS,
        Bank19SectionName.SET_GAME_STATUS_ON_FIELD_START_PLAYER_TASK,
    ];

    public void StartOnFieldTasks()
    {
    }

    public void EndSpecificTasks()
    {
    }

}
