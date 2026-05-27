using System.Collections.Generic;

namespace FootballGame.GameData.Formations.Models;

/// <summary>
/// Source: Bank3_formation_metatile_data.asm formation pointer families.
/// Preserves canonical formation order while removing raw pointer mechanics.
/// </summary>
public sealed record FormationFamilyRecord
{
    public required FormationId FormationId { get; init; }

    public required string SourceLabel { get; init; }

    public required IReadOnlyList<FormationReactionPointer> PlayerReactionsInSourceOrder { get; init; }
}
