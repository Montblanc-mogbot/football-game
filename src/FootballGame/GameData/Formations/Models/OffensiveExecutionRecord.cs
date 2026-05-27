using System.Collections.Generic;

namespace FootballGame.GameData.Formations.Models;

/// <summary>
/// Source: Bank3_formation_metatile_data.asm OFFENSIVE_EXECUTION_n tables.
/// </summary>
public sealed record OffensiveExecutionRecord
{
    public required int ExecutionNumber { get; init; }

    public required string SourceLabel { get; init; }

    public required IReadOnlyList<FormationReactionPointer> PlayerReactionsInSourceOrder { get; init; }
}
