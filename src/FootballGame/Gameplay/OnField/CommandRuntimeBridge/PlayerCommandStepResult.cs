namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Result of one player-command runtime skeleton step.
/// </summary>
public sealed record PlayerCommandStepResult
{
    public required string PlayerSlotKey { get; init; }

    public required string CommandName { get; init; }

    public required string Summary { get; init; }

    public required bool AwaitingContinuation { get; init; }

    public required IReadOnlyList<PlayerCommandRetargetRequest> RetargetRequests { get; init; }

    public DefensiveReactionCommandState? DefensiveReactionState { get; init; }

    public PassContestCommandState? PassContestState { get; init; }

    public OffensiveExchangeCommandState? OffensiveExchangeState { get; init; }

    public MovementCommandState? MovementCommandState { get; init; }

    public PlayerControlCommandState? PlayerControlCommandState { get; init; }

    public ControlFlowCommandState? ControlFlowState { get; init; }

    public required PlayerCommandPointer ResultingPointer { get; init; }
}
