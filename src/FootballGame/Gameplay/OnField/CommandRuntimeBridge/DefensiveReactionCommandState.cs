namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal continuation state for the first live defensive-reaction command family port from Bank21_22.
/// </summary>
public sealed record DefensiveReactionCommandState
{
    public string? CoverageTargetPlayerSlot { get; init; }

    public int? CoverageTimeSelector { get; init; }

    public bool LooseCoverageEnabled { get; init; }

    public bool IsHoldingMirrorLane { get; init; }

    public string? ChaseMode { get; init; }

    public int? DiveDelayFrames { get; init; }

    public int? DiveChancePercent { get; init; }

    public int? TurnSmoothingTableSize { get; init; }
}
