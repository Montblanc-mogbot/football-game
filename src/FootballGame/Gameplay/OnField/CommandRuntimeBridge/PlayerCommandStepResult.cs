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

    public DefensiveReactionCommandState? DefensiveReactionState { get; init; }

    public PassContestCommandState? PassContestState { get; init; }

    public OffensiveExchangeCommandState? OffensiveExchangeState { get; init; }
}
