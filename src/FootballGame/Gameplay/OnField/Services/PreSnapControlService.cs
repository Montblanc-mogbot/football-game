using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns defender switching, snap gating, and other pre-snap control-side helpers that remain inside Bank19_20.
/// </summary>
public sealed class PreSnapControlService
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.DEFENDER_CHANGE_BEFORE_HIKE,
        Bank19SectionName.CHECK_SNAP_PUNT,
        Bank19SectionName.MAN_CONTROLLED_PLAYER_FUNCTIONS,
    ];

    public void RunDefenderChangeBeforeHike()
    {
    }

    public void RunPuntSnapGate()
    {
    }

}
