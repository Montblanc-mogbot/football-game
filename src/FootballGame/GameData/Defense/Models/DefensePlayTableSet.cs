using System.Collections.Generic;

namespace FootballGame.GameData.Defense.Models;

/// <summary>
/// Container for the full Bank4 defensive pointer-table layer.
/// This is the semantic bridge between raw extracted Bank4 artifacts and later Bank5/21 consumers.
/// </summary>
public sealed record DefensePlayTableSet
{
    public required IReadOnlyList<DefensiveExecutionRecord> DefensiveExecutionTables { get; init; }

    public required IReadOnlyList<SpecialDefensePlayRecord> SpecialDefensePlayTables { get; init; }
}
