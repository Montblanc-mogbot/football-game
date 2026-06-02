namespace FootballGame.Conversion.OnField;

/// <summary>
/// One named cross-bank jump/entry constant declared near the top of Bank19/20.
/// This preserves the explicit symbol-to-address bridge used by the host bank.
/// </summary>
public sealed record Bank19ExternalJumpConstantRecord
{
    public required string Symbol { get; init; }

    public required string Value { get; init; }

    public required int Line { get; init; }
}
