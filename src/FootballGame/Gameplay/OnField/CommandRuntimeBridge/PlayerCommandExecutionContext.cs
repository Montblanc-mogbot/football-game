using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal per-player execution-layer context for the first Bank21_22 runtime skeleton.
/// This keeps resumable player-command ownership explicit without claiming full gameplay semantics yet.
/// </summary>
public sealed class PlayerCommandExecutionContext
{
    public PlayerCommandExecutionContext(string playerSlotKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerSlotKey);

        PlayerSlotKey = playerSlotKey;
        Pointer = PlayerCommandPointer.Empty;
    }

    public string PlayerSlotKey { get; }

    public PlayerCommandPointer Pointer { get; private set; }

    public string? PendingCommandName { get; private set; }

    public bool IsAwaitingCompletion { get; private set; }

    public string? LastStepSummary { get; private set; }

    public DefensiveReactionCommandState? DefensiveReactionState { get; private set; }

    public PassContestCommandState? PassContestState { get; private set; }

    public OffensiveExchangeCommandState? OffensiveExchangeState { get; private set; }

    public void InstallPointer(PlayerCommandPointer pointer, string? pendingCommandName)
    {
        Pointer = pointer;
        PendingCommandName = pendingCommandName;
        IsAwaitingCompletion = pendingCommandName is not null;
        LastStepSummary = pendingCommandName is null
            ? $"Installed {pointer.ScriptFamilyKey} with no pending command."
            : $"Installed {pointer.ScriptFamilyKey} and primed '{pendingCommandName}'.";
    }

    public void RecordStep(PlayerCommandDefinition commandDefinition, PlayerCommandHandlerResult? handlerResult = null)
    {
        PendingCommandName = commandDefinition.CommandName;
        IsAwaitingCompletion = handlerResult?.AwaitingContinuation ?? commandDefinition.RequiresContinuation;
        DefensiveReactionState = handlerResult?.DefensiveReactionState;
        PassContestState = handlerResult?.PassContestState;
        OffensiveExchangeState = handlerResult?.OffensiveExchangeState;
        Pointer = Pointer.Advance(commandDefinition.ByteLength);
        LastStepSummary = handlerResult?.Summary ?? $"Stepped {commandDefinition.CommandName} from {commandDefinition.SourceLabel} (+{commandDefinition.ByteLength} bytes).";
    }

    public void ClearPendingCommand(string completionReason)
    {
        IsAwaitingCompletion = false;
        LastStepSummary = completionReason;
    }
}
