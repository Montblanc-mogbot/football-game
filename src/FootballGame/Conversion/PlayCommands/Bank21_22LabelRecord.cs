namespace FootballGame.Conversion.PlayCommands;

/// <summary>
/// A global label discovered inside a Bank21_22 section.
/// </summary>
public sealed record Bank21_22LabelRecord
{
    public required string Label { get; init; }

    public required int Line { get; init; }
}
