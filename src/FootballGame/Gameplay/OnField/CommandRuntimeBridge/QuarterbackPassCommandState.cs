namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal state for bounded Bank21_22 quarterback dropback/pass-control commands.
/// </summary>
public sealed record QuarterbackPassCommandState
{
    public required string CommandKind { get; init; }

    public required bool QuarterbackHasBall { get; init; }

    public required bool AwaitingContinuation { get; init; }

    public required bool QueuedDirectionUpdate { get; init; }

    public required bool QueuedVelocityInitialization { get; init; }

    public required bool CyclingAnimationFrames { get; init; }

    public required bool ExitOnBackOfEndZone { get; init; }

    public int? RelativeDropbackX { get; init; }

    public int? TargetY { get; init; }

    public bool AppliedPlayerTwoXInversion { get; init; }

    public int? WaitFrames { get; init; }

    public bool ThrowsEarlyWhenCollisionThreatened { get; init; }

    public int? TakeSackChanceThreshold { get; init; }

    public int? TargetCount { get; init; }

    public string? SelectedTargetPlayerSlot { get; init; }

    public bool StartedPassAttempt { get; init; }

    public int? PostPassDelayFrames { get; init; }
}
