using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns injury checks, injury replacement, cutscene selection, and related outcome-presentation support inside Bank19_20.
/// </summary>
public sealed class InjuryCutsceneService
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.INJURY_CHECK_NORMAL_AND_SKIP,
        Bank19SectionName.CHECK_IF_PLAYER_CAN_BE_INJURED,
        Bank19SectionName.PLAYER_CHANGE_INJURY,
        Bank19SectionName.CUTSCENE,
        Bank19SectionName.GENERATE_CUTSCENE_RANDOM,
        Bank19SectionName.INJURY_ANIMATION,
        Bank19SectionName.CUTSCENE_SEQUENCE_PTRS_AND_SEQUENCES,
    ];

    public void ResolveInjuryChecks()
    {
    }

    public void ResolveCutsceneState()
    {
    }

}
