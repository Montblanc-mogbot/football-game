namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal host-facing player presentation / wait / per-player stat mutation state
/// for the bounded Bank21_22 posture-family seam.
/// </summary>
public sealed record PlayerPresentationCommandState
{
    public required string CommandKind { get; init; }

    public string? StanceKind { get; init; }

    public string? FacingDirectionKind { get; init; }

    public int? WaitFrames { get; init; }

    public int? WaitFramesMinimum { get; init; }

    public int? WaitFramesMaximum { get; init; }

    public bool WaitsForBallSnapExit { get; init; }

    public bool QueuedVelocityZeroing { get; init; }

    public bool QueuedStandingSpriteUpdate { get; init; }

    public bool QueuedFacingReset { get; init; }

    public bool QueuedDirectionUpdate { get; init; }

    public bool QueuedSpeedRefresh { get; init; }

    public bool QueuedVelocityInitialization { get; init; }

    public string? StatMutationKind { get; init; }

    public int? StatMutationAmount { get; init; }

    public int? TargetStatValue { get; init; }

    public bool TargetsRunningSpeedIndex { get; init; }

    public bool TargetsMaxSpeed { get; init; }

    public bool TargetsHittingPower { get; init; }

    public bool UsesOffenseDefenseDefaultReset { get; init; }

    public bool AppliesCpuJuiceBoost { get; init; }

    public bool AwaitingContinuation { get; init; }
}
