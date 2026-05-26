using System.Collections.Generic;

using FootballGame.GameData.Teams.Models;

namespace FootballGame.GameData.Teams;

/// <summary>
/// Fully loaded team-data semantic data set derived from Bank1_2.
/// </summary>
public sealed record TeamDataSet
{
    public required IReadOnlyList<TeamRosterRecord> TeamRosters { get; init; }

    public required IReadOnlyList<TeamAbilitySet> TeamAbilities { get; init; }
}
