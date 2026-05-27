using System.Collections.Generic;

namespace FootballGame.GameData.Defense.Models;

/// <summary>
/// Source: Bank4_def_spec_play_pointers_data.asm DEFENSIVE_EXECUTION_n tables.
/// Preserves the eleven-slot defensive reaction ordering used by the source bank.
/// </summary>
public sealed record DefensiveExecutionRecord
{
    public required int ExecutionNumber { get; init; }

    public required string SourceLabel { get; init; }

    public required IReadOnlyList<DefensiveReactionPointer> PlayerReactionsInSourceOrder { get; init; }
}
