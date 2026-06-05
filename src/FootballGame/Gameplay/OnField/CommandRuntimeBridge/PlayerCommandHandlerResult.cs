using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Result of a production-facing player-command handler step.
/// </summary>
public sealed record PlayerCommandHandlerResult
{
    public required string Summary { get; init; }

    public required bool AwaitingContinuation { get; init; }

    public required IReadOnlyList<PlayerCommandRetargetRequest> RetargetRequests { get; init; }

    public DefensiveReactionCommandState? DefensiveReactionState { get; init; }

    public PassContestCommandState? PassContestState { get; init; }

    public OffensiveExchangeCommandState? OffensiveExchangeState { get; init; }

    public MovementCommandState? MovementCommandState { get; init; }

    public PlayerControlCommandState? PlayerControlCommandState { get; init; }

    public ControlFlowCommandState? ControlFlowState { get; init; }

    public PlayerCommandPointer? PointerOverride { get; init; }

    public required IReadOnlyList<string> SourceNotes { get; init; }
}
