namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal state for bounded Bank21_22 player-control handoff commands.
/// </summary>
public sealed record PlayerControlCommandState
{
    public required string CommandKind { get; init; }

    public required string ControlOwner { get; init; }

    public required bool BallCarrierAssigned { get; init; }

    public required bool ManualControlRequested { get; init; }

    public required bool CpuBoostApplied { get; init; }

    public required bool QueuedFacingRefresh { get; init; }

    public required bool QueuedVelocityInitialization { get; init; }

    public required bool AwaitingLongRunningControlLoop { get; init; }
}
