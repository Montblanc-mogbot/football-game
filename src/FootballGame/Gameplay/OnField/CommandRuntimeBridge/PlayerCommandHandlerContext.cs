using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal host/runtime context passed into production-facing player-command handlers.
/// </summary>
public sealed record PlayerCommandHandlerContext
{
    public required OnFieldRoutine TriggerRoutine { get; init; }

    public required string PlayerSlotKey { get; init; }

    public required PlayerCommandExecutionContext ExecutionContext { get; init; }

    public required PlayerCommandDefinition CommandDefinition { get; init; }
}
