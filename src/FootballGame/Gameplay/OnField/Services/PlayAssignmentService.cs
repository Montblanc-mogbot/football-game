using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns initial script assignment plus mid-play bulk script reassignment for Bank19_20 transitions.
/// </summary>
public sealed class PlayAssignmentService
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.LOAD_P1_OR_P2_OFF_PLAY_INFO,
        Bank19SectionName.LOAD_OFF_FORMATIONS,
        Bank19SectionName.LOAD_DEF_PLAY_INFO,
        Bank19SectionName.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
    ];

    public void LoadInitialPlayScripts()
    {
    }

    public void ReassignForTurnoverOrReturn()
    {
    }

    public void ApplyManControlledPlayerPolicy()
    {
    }

}
