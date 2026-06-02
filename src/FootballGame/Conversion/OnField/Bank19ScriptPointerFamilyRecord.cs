namespace FootballGame.Conversion.OnField;

/// <summary>
/// One top-level Bank19/20 script-pointer family constant used to retarget players into
/// special play contexts such as interceptions, fumble recovery, punts, or celebrations.
/// </summary>
public sealed record Bank19ScriptPointerFamilyRecord
{
    public required string SourceLabel { get; init; }

    public required string Address { get; init; }

    public required string TeamSide { get; init; }

    public required string Purpose { get; init; }

    public required int Line { get; init; }
}
