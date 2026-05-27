using System.Collections.Generic;

namespace FootballGame.GameData.Backgrounds.Models;

/// <summary>
/// Source: Bank3_formation_metatile_data.asm metatile layout records.
/// Keeps source ordering and row structure explicit without preserving ROM pointer mechanics.
/// </summary>
public sealed record MetatileLayoutRecord
{
    public required int PointerIndex { get; init; }

    public required string SourceLabel { get; init; }

    public required MetatileLayoutHeader Header { get; init; }

    public required IReadOnlyList<IReadOnlyList<byte>> MetatileRows { get; init; }
}
