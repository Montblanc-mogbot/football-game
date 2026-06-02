using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns play distance calculation and per-play stat accounting represented inside Bank19_20.
/// </summary>
public sealed class StatAccountingService
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.UPDATE_STATS,
        Bank19SectionName.CALCULATE_PLAY_DISTANCE,
    ];

    public void UpdateInGameStats()
    {
    }

    public void CalculatePlayDistance()
    {
    }

}
