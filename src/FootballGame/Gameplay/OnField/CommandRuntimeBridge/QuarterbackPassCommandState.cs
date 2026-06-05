namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal state for bounded Bank21_22 quarterback dropback / pass-control commands.
/// </summary>
public sealed record QuarterbackPassCommandState
{
    public required string CommandKind { get; init; }

    public int? RelativeDropbackX { get; init; }

    public int? TargetY { get; init; }

    public required bool AppliedPlayerTwoXInversion { get; init; }

    public required bool QueuedDirectionUpdate { get; init; }

    public required bool QueuedVelocityInitialization { get; init; }

    public required bool AwaitingContinuation { get; init; }

    public int? AnimationToggleFrames { get; init; }

    public int? WaitingFrames { get; init; }

    public bool WaitsForNearbyPressure { get; init; }

    public bool SackWindowEnabled { get; init; }

    public int? SackChanceThreshold { get; init; }

    public int? TargetReceiverCount { get; init; }

    public int? SelectedTargetPriorityIndex { get; init; }

    public string? SelectedTargetPlayerSlot { get; init; }

    public bool StartedPassAttempt { get; init; }

    public bool QueuedPostThrowDelay { get; init; }

    public int? PostThrowDelayFrames { get; init; }
}
