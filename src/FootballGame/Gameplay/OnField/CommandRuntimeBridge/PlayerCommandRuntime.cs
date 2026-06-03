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
    private readonly PassContestCommandDispatcher passContestDispatcher;
    private readonly OffensiveExchangeCommandDispatcher offensiveExchangeDispatcher;

    public PlayerCommandRuntime(
        DefensiveReactionCommandDispatcher? defensiveReactionDispatcher = null,
        PassContestCommandDispatcher? passContestDispatcher = null,
        OffensiveExchangeCommandDispatcher? offensiveExchangeDispatcher = null)
    {
        this.defensiveReactionDispatcher = defensiveReactionDispatcher ?? new DefensiveReactionCommandDispatcher();
        this.passContestDispatcher = passContestDispatcher ?? new PassContestCommandDispatcher();
        this.offensiveExchangeDispatcher = offensiveExchangeDispatcher ?? new OffensiveExchangeCommandDispatcher();
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
        PlayerCommandHandlerResult? handlerResult = TryDispatchDefensiveReaction(executionContext, commandDefinition)
            ?? TryDispatchPassContest(executionContext, commandDefinition)
            ?? TryDispatchOffensiveExchange(executionContext, commandDefinition);
        executionContext.RecordStep(commandDefinition, handlerResult);

        return new PlayerCommandStepResult
        {
            PlayerSlotKey = executionContext.PlayerSlotKey,
            CommandName = commandDefinition.CommandName,
            AwaitingContinuation = executionContext.IsAwaitingCompletion,
            Summary = executionContext.LastStepSummary ?? $"Stepped {commandDefinition.CommandName}.",
            RetargetRequests = handlerResult?.RetargetRequests ?? Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = executionContext.DefensiveReactionState,
            PassContestState = executionContext.PassContestState,
            OffensiveExchangeState = executionContext.OffensiveExchangeState,
        };
    }

    private PlayerCommandHandlerResult? TryDispatchDefensiveReaction(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        if (commandDefinition.TriggerRoutine is not Gameplay.OnField.OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE)
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

    private PlayerCommandHandlerResult? TryDispatchPassContest(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        if (commandDefinition.TriggerRoutine is not Gameplay.OnField.OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS)
        {
            return null;
        }

        return passContestDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine.Value,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private PlayerCommandHandlerResult? TryDispatchOffensiveExchange(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        if (commandDefinition.TriggerRoutine is not (Gameplay.OnField.OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS or Gameplay.OnField.OnFieldRoutine.CHECK_SNAP_PUNT))
        {
            if (!IsOffensiveExchangeContinuationCommand(commandDefinition.CommandName))
            {
                return null;
            }
        }

        return offensiveExchangeDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine ?? Gameplay.OnField.OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private static bool IsOffensiveExchangeContinuationCommand(string commandName)
    {
        return commandName is "BackfieldHandoffCommand"
            or "PitchBallCommand"
            or "RunnerReceiveHandoffCommand"
            or "RunnerFakeHandoffAnimationCommand"
            or "ReceivePitchContinuationCommand";
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
