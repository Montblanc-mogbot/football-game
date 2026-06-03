using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Result of a production-facing player-command handler step.
/// </summary>
public sealed record PlayerCommandHandlerResult
{
    public required string Summary { get; init; }

    public required bool AwaitingContinuation { get; init; }

    public DefensiveReactionCommandState? DefensiveReactionState { get; init; }

    public PassContestCommandState? PassContestState { get; init; }

    public OffensiveExchangeCommandState? OffensiveExchangeState { get; init; }

    public required IReadOnlyList<string> SourceNotes { get; init; }
}
