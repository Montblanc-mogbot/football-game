namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal continuation state for the bounded Bank21_22 special-teams / return command slice.
/// </summary>
public sealed record SpecialTeamsCommandState
{
    public string? CommandKind { get; init; }

    public bool WaitedForHostSnapGate { get; init; }

    public bool WaitedForBallKickedState { get; init; }

    public bool UpdatedManualControlAndDisplay { get; init; }

    public bool BallCarrierAssigned { get; init; }

    public bool BallCarrierReleased { get; init; }

    public bool BallAnimationStarted { get; init; }

    public bool BallAnimationResolved { get; init; }

    public bool KickMeterStarted { get; init; }

    public bool KickArrowControlStarted { get; init; }

    public bool KickPowerCalculated { get; init; }

    public bool KickDirectionCalculated { get; init; }

    public bool KickCutsceneQueued { get; init; }

    public bool TouchbackEligible { get; init; }

    public bool ReturnerTurnedTowardBall { get; init; }

    public bool ReturnerWaitsForCatch { get; init; }

    public bool StartedCoverageRunToBallRelativeSpot { get; init; }

    public bool FakeOrOnsideAware { get; init; }

    public string? ReturnerRole { get; init; }

    public string? KickType { get; init; }

    public string? ContinuationStage { get; init; }

    public int? WaitFrames { get; init; }

    public int? MinimumDistanceYards { get; init; }

    public int? MaximumDistanceYards { get; init; }
}
