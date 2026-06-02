using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns player skill loading and targeted roster/attribute hydration used during on-field setup.
/// </summary>
public sealed class PlayerSkillHydrationService
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.LOAD_SKILLS,
    ];

    public void LoadAllSkillsIntoPlayerState()
    {
    }

    public void LoadSinglePlayerSkillsIntoPlayerState()
    {
    }

}
