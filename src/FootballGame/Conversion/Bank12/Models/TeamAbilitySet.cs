namespace FootballGame.Conversion.Bank12.Models;

/// <summary>
/// Decoded Bank1_2 team ability container keyed by canonical roster slots.
/// </summary>
public sealed record TeamAbilitySet
{
    public required TeamId TeamId { get; init; }

    public required string SourceLabel { get; init; }

    public required IReadOnlyDictionary<RosterSlot, BaseAbilityRecord> AbilitiesBySlot { get; init; }
}
