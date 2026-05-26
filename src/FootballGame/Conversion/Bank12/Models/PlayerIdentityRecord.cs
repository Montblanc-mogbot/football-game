namespace FootballGame.Conversion.Bank12.Models;

/// <summary>
/// Decoded Bank1_2 identity record.
/// Preserves exact source payload without forcing display-name normalization.
/// </summary>
public sealed record PlayerIdentityRecord
{
    public required TeamId TeamId { get; init; }

    public required RosterSlot RosterSlot { get; init; }

    public required string SourceLabel { get; init; }

    public required byte JerseyNumber { get; init; }

    public required string SourceNamePayload { get; init; }

    public bool IsPlaceholderQuarterback => JerseyNumber == 0 && SourceNamePayload.StartsWith("qb", StringComparison.Ordinal);
}
