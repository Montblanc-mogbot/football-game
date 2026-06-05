namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal continuation state for bounded Bank21_22 special-teams kick/return command semantics.
/// </summary>
public sealed record SpecialTeamsCommandState
{
    public required string CommandKind { get; init; }

    public string? SetupKind { get; init; }

    public bool WaitedForSnapOrKickGate { get; init; }

    public bool WaitedForBallArrival { get; init; }

    public bool BallCarrierAssigned { get; init; }

    public bool BallAnimationStarted { get; init; }

    public bool BallAnimationResolved { get; init; }

    public bool KickMeterOrArrowStarted { get; init; }

    public bool UsesComputerTimingWindow { get; init; }

    public bool KickOrPuntDistanceComputed { get; init; }

    public bool KickDirectionRandomizedForCpu { get; init; }

    public bool ReturnerIconApplied { get; init; }

    public bool ManualControlRetargeted { get; init; }

    public bool ReturnerTurnedTowardBall { get; init; }

    public bool ReturnerRunbackStarted { get; init; }

    public bool WaitsForKickRelease { get; init; }

    public bool PreservesAvoidBlockBugByPolicy { get; init; }

    public string? ContinuationStage { get; init; }

    public int? PostActionDelayFrames { get; init; }
}
