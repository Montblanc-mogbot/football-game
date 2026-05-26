namespace FootballGame.Conversion.Bank12.Models;

/// <summary>
/// Bank1_2 team roster preserving canonical team order and slot identity.
/// </summary>
public sealed record TeamRosterRecord
{
    public required TeamId TeamId { get; init; }

    public required string TeamListLabel { get; init; }

    public required IReadOnlyList<PlayerIdentityRecord> PlayersInCanonicalSlotOrder { get; init; }
}
