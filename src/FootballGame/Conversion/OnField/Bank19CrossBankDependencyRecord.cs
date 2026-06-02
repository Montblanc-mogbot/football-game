namespace FootballGame.Conversion.OnField;

/// <summary>
/// A named cross-bank dependency referenced by Bank19/20.
/// This keeps the conversion honest about what Bank19/20 owns locally versus what it triggers elsewhere.
/// </summary>
public sealed record Bank19CrossBankDependencyRecord
{
    public required string Symbol { get; init; }

    public required string SourceBank { get; init; }

    public required string DependencyKind { get; init; }

    public required string Notes { get; init; }
}
