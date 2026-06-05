namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal movement-target state for bounded Bank21_22 move-command parity slices.
/// </summary>
public sealed record MovementCommandState
{
    public required string CommandKind { get; init; }

    public required string AnchorKind { get; init; }

    public required int RelativeX { get; init; }

    public required int RelativeY { get; init; }

    public int? AbsoluteTargetX { get; init; }

    public int? AbsoluteTargetY { get; init; }

    public required bool AppliedPlayerTwoXInversion { get; init; }

    public required bool QueuedDirectionUpdate { get; init; }

    public required bool QueuedVelocityInitialization { get; init; }

    public required bool AwaitingArrivalLoop { get; init; }
}
