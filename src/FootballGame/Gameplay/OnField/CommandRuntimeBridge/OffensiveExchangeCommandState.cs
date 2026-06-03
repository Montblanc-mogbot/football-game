namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal continuation state for the first live Bank21_22 snap-exchange receive slice.
/// </summary>
public sealed record OffensiveExchangeCommandState
{
    public string? ExchangeKind { get; init; }

    public bool WaitedForHostSnapGate { get; init; }

    public bool ManualControlRetargeted { get; init; }

    public bool BallCarrierAssigned { get; init; }

    public bool BallAnimationStarted { get; init; }

    public bool BallAnimationResolved { get; init; }

    public bool WaitsForKickRelease { get; init; }

    public bool QuarterbackStoppedForExchange { get; init; }

    public bool QuarterbackReleasedBallCarrierState { get; init; }

    public bool HandoffOrPitchIconTimerStarted { get; init; }

    public string? RetargetedPlayerSlot { get; init; }

    public string? RetargetedContinuationCommand { get; init; }

    public bool RetargetSkippedBecauseTargetInvalid { get; init; }

    public bool FakeExchange { get; init; }

    public bool InFlightBallStateCreated { get; init; }

    public string? ContinuationStage { get; init; }

    public int? PostExchangeDelayFrames { get; init; }
}
