namespace FootballGame.Gameplay.OnField;

/// <summary>
/// One runtime-facing placement for a Bank19_20 section.
/// </summary>
public sealed record Bank19RuntimeSectionPlacement
{
    public required Bank19SectionName Section { get; init; }

    public required Bank19RuntimeOwnerKind OwnerKind { get; init; }

    public required string OwnerTypeName { get; init; }

    public required string Notes { get; init; }
}
