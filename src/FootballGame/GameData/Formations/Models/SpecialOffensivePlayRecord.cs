using System.Collections.Generic;

namespace FootballGame.GameData.Formations.Models;

/// <summary>
/// Source: Bank3_formation_metatile_data.asm special offensive-play pointer tables.
/// These remain source-shaped because their twelve-entry ordering is still parity-relevant.
/// </summary>
public sealed record SpecialOffensivePlayRecord
{
    public required string SourceLabel { get; init; }

    public required IReadOnlyList<FormationReactionPointer> PlayerReactionsInSourceOrder { get; init; }
}
