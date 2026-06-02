using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns banners, music, scroll limits, LOS markers, draw-task startup, and similar Bank19_20 presentation helpers.
/// </summary>
public sealed class OnFieldPresentationService
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.CHECK_FOR_UPDATE_BANNER,
        Bank19SectionName.UPDATE_SCORE_FUNCTIONS,
        Bank19SectionName.DRAW_RECOVER,
        Bank19SectionName.SET_ONFIELD_SONG,
        Bank19SectionName.STOP_CURRENT_SONG,
        Bank19SectionName.SIDE_CHANGE_BANNER_AND_SONG,
        Bank19SectionName.UPDATE_SCROLL_LIMITS,
        Bank19SectionName.START_DRAW_GAME_FIELD,
        Bank19SectionName.UPDATE_LOS_MARKERS,
    ];

    public void UpdateOnFieldBannerAndSongState()
    {
    }

    public void UpdateScrollAndFieldMarkers()
    {
    }

}
