namespace FootballGame.Gameplay.OnField;

/// <summary>
/// One runtime-facing placement for a Bank19_20 section.
/// </summary>
public sealed record OnFieldRoutinePlacement
{
    public required OnFieldRoutine Routine { get; init; }

    public required OnFieldOwnerKind OwnerKind { get; init; }

    public required string OwnerTypeName { get; init; }

    public required string Notes { get; init; }
}
