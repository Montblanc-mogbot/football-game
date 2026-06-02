using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns initial script assignment plus mid-play bulk script reassignment for Bank19_20 transitions.
/// </summary>
public sealed class PlayAssignmentService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.LOAD_P1_OR_P2_OFF_PLAY_INFO,
        OnFieldRoutine.LOAD_OFF_FORMATIONS,
        OnFieldRoutine.LOAD_DEF_PLAY_INFO,
        OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
    ];

    public void LoadKickoffScripts(OnFieldGameState state, OnFieldTeam receivingTeam)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_OFF_FORMATIONS);
        state.RecordRoutine(OnFieldRoutine.LOAD_DEF_PLAY_INFO);
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        state.OffensiveFormationKey = "KICKOFF_OFF_FORMATION_ID";
        state.DefensivePlayKey = "KICKOFF_DEF_PLAY_ID";
        state.RecordEvent($"Loaded kickoff script families with {receivingTeam} as the return side.");
    }

    public void LoadPlaySelectionScripts(OnFieldGameState state, OnFieldTeam possessionTeam, OnFieldPlayType playType)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_P1_OR_P2_OFF_PLAY_INFO);
        state.RecordRoutine(OnFieldRoutine.LOAD_OFF_FORMATIONS);
        state.RecordRoutine(OnFieldRoutine.LOAD_DEF_PLAY_INFO);
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        state.PossessionTeam = possessionTeam;
        state.PlayType = playType;
        state.RecordEvent($"Loaded play-selection script families for {possessionTeam} {playType}.");
    }

    public void ReassignForTurnoverOrReturn(OnFieldGameState state, string transitionKey)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        state.RecordEvent($"Reassigned live-play scripts for transition '{transitionKey}'.");
    }

    public void ApplyManControlledPlayerPolicy(OnFieldGameState state, bool includeManControlledPlayer)
    {
        string policy = includeManControlledPlayer ? "include-man" : "exclude-man";
        state.RecordEvent($"Applied play-assignment policy '{policy}' during Bank19_20 script installation.");
    }
}
