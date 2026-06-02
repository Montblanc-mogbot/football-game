namespace FootballGame.Conversion.OnField;

/// <summary>
/// One global label declared inside a Bank19/20 section.
/// </summary>
public sealed record Bank19SectionLabelRecord
{
    public required string Label { get; init; }

    public required int Line { get; init; }
}
