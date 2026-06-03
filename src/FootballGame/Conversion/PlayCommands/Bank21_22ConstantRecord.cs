namespace FootballGame.Conversion.PlayCommands;

/// <summary>
/// One named gameplay/runtime constant from Bank21_22's top-of-bank constant block.
/// </summary>
public sealed record Bank21_22ConstantRecord
{
    public required string Name { get; init; }

    public required string Value { get; init; }

    public required string Comment { get; init; }

    public required int Line { get; init; }
}
