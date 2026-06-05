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
    private readonly MovementCommandDispatcher movementCommandDispatcher;
    private readonly PlayerControlCommandDispatcher playerControlCommandDispatcher;
    private readonly ControlFlowCommandDispatcher controlFlowDispatcher;
    private readonly PreSnapCommandDispatcher preSnapCommandDispatcher;
    private readonly TargetingCommandDispatcher targetingCommandDispatcher;
    private readonly QuarterbackPassCommandDispatcher quarterbackPassCommandDispatcher;
    private readonly SpecialTeamsCommandDispatcher specialTeamsCommandDispatcher;

    public PlayerCommandRuntime(
        DefensiveReactionCommandDispatcher? defensiveReactionDispatcher = null,
        PassContestCommandDispatcher? passContestDispatcher = null,
        OffensiveExchangeCommandDispatcher? offensiveExchangeDispatcher = null,
        MovementCommandDispatcher? movementCommandDispatcher = null,
        PlayerControlCommandDispatcher? playerControlCommandDispatcher = null,
        ControlFlowCommandDispatcher? controlFlowDispatcher = null,
        PreSnapCommandDispatcher? preSnapCommandDispatcher = null,
        TargetingCommandDispatcher? targetingCommandDispatcher = null,
        QuarterbackPassCommandDispatcher? quarterbackPassCommandDispatcher = null,
        SpecialTeamsCommandDispatcher? specialTeamsCommandDispatcher = null)
    {
        this.defensiveReactionDispatcher = defensiveReactionDispatcher ?? new DefensiveReactionCommandDispatcher();
        this.passContestDispatcher = passContestDispatcher ?? new PassContestCommandDispatcher();
        this.offensiveExchangeDispatcher = offensiveExchangeDispatcher ?? new OffensiveExchangeCommandDispatcher();
        this.movementCommandDispatcher = movementCommandDispatcher ?? new MovementCommandDispatcher();
        this.playerControlCommandDispatcher = playerControlCommandDispatcher ?? new PlayerControlCommandDispatcher();
        this.controlFlowDispatcher = controlFlowDispatcher ?? new ControlFlowCommandDispatcher();
        this.preSnapCommandDispatcher = preSnapCommandDispatcher ?? new PreSnapCommandDispatcher();
        this.targetingCommandDispatcher = targetingCommandDispatcher ?? new TargetingCommandDispatcher();
        this.quarterbackPassCommandDispatcher = quarterbackPassCommandDispatcher ?? new QuarterbackPassCommandDispatcher();
        this.specialTeamsCommandDispatcher = specialTeamsCommandDispatcher ?? new SpecialTeamsCommandDispatcher();
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
            ?? TryDispatchOffensiveExchange(executionContext, commandDefinition)
            ?? TryDispatchMovement(executionContext, commandDefinition)
            ?? TryDispatchPlayerControl(executionContext, commandDefinition)
            ?? TryDispatchControlFlow(executionContext, commandDefinition)
            ?? TryDispatchPreSnap(executionContext, commandDefinition)
            ?? TryDispatchTargeting(executionContext, commandDefinition)
            ?? TryDispatchQuarterbackPass(executionContext, commandDefinition)
            ?? TryDispatchSpecialTeams(executionContext, commandDefinition);
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
            MovementCommandState = executionContext.MovementCommandState,
            PlayerControlCommandState = executionContext.PlayerControlCommandState,
            ControlFlowState = executionContext.ControlFlowState,
            PreSnapCommandState = executionContext.PreSnapCommandState,
            PassTargetOrderCommandState = executionContext.PassTargetOrderCommandState,
            QuarterbackPassCommandState = executionContext.QuarterbackPassCommandState,
            SpecialTeamsCommandState = executionContext.SpecialTeamsCommandState,
            ResultingPointer = executionContext.Pointer,
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

    private PlayerCommandHandlerResult? TryDispatchMovement(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        return movementCommandDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine ?? Gameplay.OnField.OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private PlayerCommandHandlerResult? TryDispatchPlayerControl(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        return playerControlCommandDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine ?? Gameplay.OnField.OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private PlayerCommandHandlerResult? TryDispatchControlFlow(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        return controlFlowDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine ?? Gameplay.OnField.OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private PlayerCommandHandlerResult? TryDispatchPreSnap(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        return preSnapCommandDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine ?? Gameplay.OnField.OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private PlayerCommandHandlerResult? TryDispatchTargeting(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        return targetingCommandDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine ?? Gameplay.OnField.OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private PlayerCommandHandlerResult? TryDispatchQuarterbackPass(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        return quarterbackPassCommandDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine ?? Gameplay.OnField.OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private PlayerCommandHandlerResult? TryDispatchSpecialTeams(PlayerCommandExecutionContext executionContext, PlayerCommandDefinition commandDefinition)
    {
        return specialTeamsCommandDispatcher.Dispatch(new PlayerCommandHandlerContext
        {
            TriggerRoutine = commandDefinition.TriggerRoutine ?? Gameplay.OnField.OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
            PlayerSlotKey = executionContext.PlayerSlotKey,
            ExecutionContext = executionContext,
            CommandDefinition = commandDefinition,
        });
    }

    private static bool IsOffensiveExchangeContinuationCommand(string commandName)
    {
        return commandName is "UnderCenterSnapReceiveCommand"
            or "ShotgunSnapReceiveCommand"
            or "FieldGoalSnapReceiveCommand"
            or "BackfieldHandoffCommand"
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
