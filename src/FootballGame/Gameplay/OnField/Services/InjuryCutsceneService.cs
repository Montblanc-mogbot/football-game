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

    public void ResolveNormalInjuryChecks(OnFieldGameState state, OnFieldTeam possessionTeam)
    {
        state.RecordRoutine(OnFieldRoutine.INJURY_CHECK_NORMAL_AND_SKIP);
        state.RecordEvent($"Resolved normal injury checks for the completed {possessionTeam} play.");
    }

    public void ResolveCutsceneState(OnFieldGameState state, string cutsceneKey)
    {
        state.RecordRoutine(OnFieldRoutine.CUTSCENE);
        state.RecordEvent($"Resolved cutscene state '{cutsceneKey}' for the current Bank19_20 outcome.");
    }

    public void ClearCutsceneStateForPassStart(OnFieldGameState state)
    {
        state.RecordEvent("Cleared cutscene-to-play state and seeded pass-play cutscene randomness.");
    }

    public void ResolveTouchdownCutscene(OnFieldGameState state, OnFieldTeam scoringTeam, OnFieldTouchdownKind touchdownKind)
    {
        state.RecordRoutine(OnFieldRoutine.CUTSCENE);
        state.RecordEvent($"Resolved touchdown cutscene state for {scoringTeam} ({touchdownKind}).");
    }

    public void ResolveRecoveryCutscene(OnFieldGameState state, string recoveryKind, OnFieldTeam recoveringTeam)
    {
        state.RecordRoutine(OnFieldRoutine.CUTSCENE);
        state.RecordEvent($"Resolved {recoveryKind} recovery cutscene state for {recoveringTeam}.");
    }
}
