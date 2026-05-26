using System.Collections.Generic;

namespace FootballGame.Conversion.PlayScripts;

/// <summary>
/// Source: Bank5_6_off_def_play_data.asm, OFFENSE_PLAYER_REACTION_091.
/// Models one bounded offensive pitch family without committing to broader runtime architecture yet.
/// </summary>
public sealed record OffensivePitchPlayFamily
{
    public required string FamilyId { get; init; }

    public required string SourceLabel { get; init; }

    public required SnapStyle SnapStyle { get; init; }

    public required string FakeHandoffTarget { get; init; }

    public required string PitchTarget { get; init; }

    public required IReadOnlyList<BallPlacementStep> BallPlacementSteps { get; init; }

    public required string ExitReactionLabel { get; init; }
}

public enum SnapStyle
{
    UnderCenter,
    Shotgun,
}

public sealed record BallPlacementStep
{
    public required string SourceAddress { get; init; }

    public required sbyte VerticalDelta { get; init; }

    public required sbyte HorizontalDelta { get; init; }

    public required string Notes { get; init; }
}
