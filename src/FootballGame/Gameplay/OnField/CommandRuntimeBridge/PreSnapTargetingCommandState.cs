namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal state for bounded Bank21_22 pre-snap motion and pass-target ordering slices.
/// </summary>
public sealed record PreSnapTargetingCommandState
{
    public string? CommandKind { get; init; }

    public string? MirrorTargetPlayerSlot { get; init; }

    public int? FollowDelayFrames { get; init; }

    public int? VerticalProximityLimit { get; init; }

    public bool WaitsForBallSnapExit { get; init; }

    public bool HoldsVerticalMirrorLoop { get; init; }

    public bool QueuedFacingResetOnHold { get; init; }

    public bool QueuedStandingResetOnHold { get; init; }

    public int? TargetPriorityIndex { get; init; }

    public bool SetAsCurrentPassTarget { get; init; }

    public bool UpdatedPassTargetOrder { get; init; }
}
