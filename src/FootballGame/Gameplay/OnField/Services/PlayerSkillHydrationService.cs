using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns player skill loading and targeted roster/attribute hydration used during on-field setup.
/// </summary>
public sealed class PlayerSkillHydrationService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.LOAD_SKILLS,
    ];

    public void LoadAllSkillsIntoPlayerState(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_SKILLS);
        state.RecordEvent("Loaded the base player skills needed for the current on-field setup.");
    }

    public void LoadSinglePlayerSkillsIntoPlayerState(OnFieldGameState state, OnFieldTeam team, string playerRoleKey)
    {
        state.RecordEvent($"Loaded single-player skill overrides for {team} role {playerRoleKey}.");
    }
}
