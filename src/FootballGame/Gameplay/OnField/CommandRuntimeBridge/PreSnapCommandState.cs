namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal state for bounded Bank21_22 pre-snap motion command semantics.
/// </summary>
public sealed record PreSnapCommandState
{
    public required string CommandKind { get; init; }

    public required string FollowTargetPlayerSlot { get; init; }

    public required int FollowDelayFrames { get; init; }

    public required int NearMotionPlayerYThreshold { get; init; }

    public required bool WaitsForBallSnapExit { get; init; }

    public required bool StopsWhenAlignedWithinThreshold { get; init; }

    public required bool QueuedFacingResetWhenAligned { get; init; }

    public required bool QueuedVelocityInitializationWhileFollowing { get; init; }

    public required bool AwaitingFollowLoopContinuation { get; init; }
}
