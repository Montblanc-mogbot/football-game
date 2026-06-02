using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns the narrow CPU kickoff/special-teams decision logic that supports Bank19_20 host flow.
/// </summary>
public sealed class CpuPlayDecisionService
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.CPU_PLAY_LOGIC,
    ];

    public void ChooseCpuKickoffStrategy()
    {
    }

}
