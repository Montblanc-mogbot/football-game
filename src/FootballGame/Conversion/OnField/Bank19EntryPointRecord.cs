namespace FootballGame.Conversion.OnField;

/// <summary>
/// One explicit Bank19/20 bank entrypoint label and its immediate target.
/// </summary>
public sealed record Bank19EntryPointRecord
{
    public required string SourceLabel { get; init; }

    public required string TargetLabel { get; init; }

    public required int Line { get; init; }

    public required string Notes { get; init; }
}
