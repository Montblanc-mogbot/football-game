using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns banners, music, scroll limits, LOS markers, draw-task startup, and similar Bank19_20 presentation helpers.
/// </summary>
public sealed class OnFieldPresentationService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.CHECK_FOR_UPDATE_BANNER,
        OnFieldRoutine.UPDATE_SCORE_FUNCTIONS,
        OnFieldRoutine.DRAW_RECOVER,
        OnFieldRoutine.SET_ONFIELD_SONG,
        OnFieldRoutine.STOP_CURRENT_SONG,
        OnFieldRoutine.SIDE_CHANGE_BANNER_AND_SONG,
        OnFieldRoutine.UPDATE_SCROLL_LIMITS,
        OnFieldRoutine.START_DRAW_GAME_FIELD,
        OnFieldRoutine.UPDATE_LOS_MARKERS,
    ];

    public void PrepareKickoffPresentation(OnFieldGameState state, OnFieldTeam kickingTeam, bool isSafetyKickoff)
    {
        state.RecordRoutine(OnFieldRoutine.UPDATE_SCROLL_LIMITS);
        state.RecordRoutine(OnFieldRoutine.START_DRAW_GAME_FIELD);
        state.CurrentBannerKey = isSafetyKickoff ? "SAFETY_KICKOFF" : "KICKOFF";
        state.CurrentSongSide = kickingTeam.ToString();
        state.RecordEvent($"Prepared kickoff presentation for {kickingTeam} (safety kickoff: {isSafetyKickoff}).");
    }

    public void PreparePlaySelectionPresentation(OnFieldGameState state, OnFieldTeam possessionTeam)
    {
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_UPDATE_BANNER);
        state.RecordRoutine(OnFieldRoutine.UPDATE_SCORE_FUNCTIONS);
        state.CurrentBannerKey = $"{possessionTeam}_PLAY_SELECTION";
        state.RecordEvent($"Prepared play-selection presentation for {possessionTeam}.");
    }

    public void PrepareRegularPlayPresentation(OnFieldGameState state, OnFieldTeam possessionTeam)
    {
        state.RecordRoutine(OnFieldRoutine.UPDATE_SCROLL_LIMITS);
        state.RecordRoutine(OnFieldRoutine.UPDATE_LOS_MARKERS);
        state.CurrentBannerKey = $"{possessionTeam}_DOWN_DISTANCE";
        state.CurrentSongSide = possessionTeam.ToString();
        state.RecordEvent($"Prepared regular-play scroll, marker, and banner presentation for {possessionTeam}.");
    }

    public void PrepareSpecialTeamsPresentation(OnFieldGameState state, OnFieldTeam possessionTeam, OnFieldPlayType playType)
    {
        state.RecordRoutine(OnFieldRoutine.UPDATE_SCROLL_LIMITS);
        state.RecordRoutine(OnFieldRoutine.UPDATE_LOS_MARKERS);
        state.CurrentBannerKey = $"{possessionTeam}_{playType}";
        state.CurrentSongSide = possessionTeam.ToString();
        state.RecordEvent($"Prepared special-teams presentation for {possessionTeam} {playType}.");
    }

    public void UpdateScrollAndFieldMarkers(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.UPDATE_LOS_MARKERS);
        state.RecordEvent("Updated LOS markers and other on-field presentation anchors.");
    }
}
