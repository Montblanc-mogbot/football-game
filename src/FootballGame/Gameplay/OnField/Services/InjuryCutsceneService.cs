using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns injury checks, injury replacement, cutscene selection, and related outcome-presentation support inside Bank19_20.
/// </summary>
public sealed class InjuryCutsceneService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.INJURY_CHECK_NORMAL_AND_SKIP,
        OnFieldRoutine.CHECK_IF_PLAYER_CAN_BE_INJURED,
        OnFieldRoutine.PLAYER_CHANGE_INJURY,
        OnFieldRoutine.CUTSCENE,
        OnFieldRoutine.GENERATE_CUTSCENE_RANDOM,
        OnFieldRoutine.INJURY_ANIMATION,
        OnFieldRoutine.CUTSCENE_SEQUENCE_PTRS_AND_SEQUENCES,
    ];

    public void ResolveInjuryChecks()
    {
    }

    public void ResolveCutsceneState()
    {
    }

}
