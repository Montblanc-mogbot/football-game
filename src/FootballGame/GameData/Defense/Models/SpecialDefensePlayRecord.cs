using System.Collections.Generic;

namespace FootballGame.GameData.Defense.Models;

/// <summary>
/// Source: Bank4_def_spec_play_pointers_data.asm special defense-play pointer tables.
/// These remain source-shaped because their twelve-entry ordering is still parity-relevant.
/// </summary>
public sealed record SpecialDefensePlayRecord
{
    public required string SourceLabel { get; init; }

    public required IReadOnlyList<DefensiveReactionPointer> PlayerReactionsInSourceOrder { get; init; }
}
