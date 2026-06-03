using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// First production-facing Bank21_22 runtime skeleton.
/// It models host handoff plus per-player stepping without absorbing Bank19_20 host responsibilities.
/// </summary>
public sealed class PlayerCommandRuntime
{
    private readonly Dictionary<string, PlayerCommandExecutionContext> executionContexts = new(StringComparer.Ordinal);
    private readonly DefensiveReactionCommandDispatcher defensiveReactionDispatcher;

    public PlayerCommandRuntime(DefensiveReactionCommandDispatcher? defensiveReactionDispatcher = null)
    {
        this.defensiveReactionDispatcher = defensiveReactionDispatcher ?? new DefensiveReactionCommandDispatcher();
    }

    public IReadOnlyCollection<PlayerCommandExecutionContext> ExecutionContexts => executionContexts.Values;

    public PlayerCommandExecutionContext InstallHostRequest(PlayerCommandRuntimeHostRequest hostRequest, string playerSlotKey, PlayerCommandPointer pointer)
    {
        ArgumentNullException.ThrowIfNull(hostRequest);
        ArgumentException.ThrowIfNullOrWhiteSpace(playerSlotKey);
        ArgumentNullException.ThrowIfNull(pointer);

        PlayerCommandExecutionContext executionContext = GetOrCreateExecutionContext(playerSlotKey);
        executionContext.InstallPointer(pointer, hostRequest.BridgeSymbol ?? hostRequest.TriggerRoutine.ToString());
        return executionContext;
    }

    public PlayerCommandStepResult StepPlayerCommand(string playerSlotKey, PlayerCommandDefinition commandDefinition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerSlotKey);
        ArgumentNullException.ThrowIfNull(commandDefinition);

        PlayerCommandExecutionContext executionContext = GetOrCreateExecutionContext(playerSlotKey);
        PlayerCommandHandlerResult? handlerResult = TryDispatchDefensiveReaction(executionContext, commandDefinition);
        executionContext.RecordStep(commandDefinition, handlerResult);

        return new PlayerCommandStepResult
        {
            PlayerSlotKey = executionContext.PlayerSlotKey,
            CommandName = commandDefinition.CommandName,
            AwaitingContinuation = executionContext.IsAwaitingCompletion,
            Summary = executionContext.LastStepSummary ?? $"Stepped {commandDefinition.CommandName}.",
            DefensiveReactionState = executionContext.DefensiveReactionState,
        };
    }

    private PlayerCommandHandlerResult? TryDispatchDefensiveReaction(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        if (commandDefinition.TriggerRoutine is not (Gameplay.OnField.OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE or Gameplay.OnField.OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS))
        {
            return null;
        }

        return defensiveReactionDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine.Value,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private PlayerCommandExecutionContext GetOrCreateExecutionContext(string playerSlotKey)
    {
        if (executionContexts.TryGetValue(playerSlotKey, out PlayerCommandExecutionContext? executionContext))
        {
            return executionContext;
        }

        executionContext = new PlayerCommandExecutionContext(playerSlotKey);
        executionContexts.Add(playerSlotKey, executionContext);
        return executionContext;
    }
}
