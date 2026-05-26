using System.Collections.Generic;

using FootballGame.Conversion.Bank12.Models;

namespace FootballGame.Conversion.Bank12;

/// <summary>
/// Fully loaded Bank1_2 semantic data set.
/// </summary>
public sealed record Bank12DataSet
{
    public required IReadOnlyList<TeamRosterRecord> TeamRosters { get; init; }

    public required IReadOnlyList<TeamAbilitySet> TeamAbilities { get; init; }
}
