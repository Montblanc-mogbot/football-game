using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns pass-target ranking, defender proximity ordering, and related Bank19_20 pass-contest setup helpers.
/// </summary>
public sealed class PassTargetingService
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.SET_PLAYERS_CLOSE_TO_PASS,
        Bank19SectionName.UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS,
    ];

    public void OrderPassCollisionPlayers()
    {
    }

    public void UpdatePassTargetIndicator()
    {
    }

}
