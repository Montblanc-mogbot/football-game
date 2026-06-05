using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:2778-2854.
/// Handles the bounded CPU-ball-carrier control handoff command family.
/// </summary>
public sealed class CpuControlBallCarrierCommandHandler : IPlayerControlCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "CpuControlBallCarrierCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool cpuOwnsBallCarrier = GetBoolOperand(context.CommandDefinition, "cpuOwnsBallCarrier", true);
        bool interceptedBall = GetBoolOperand(context.CommandDefinition, "interceptedBall", false);
        bool cpuBoostApplied = GetBoolOperand(context.CommandDefinition, "cpuBoostApplied", !interceptedBall && cpuOwnsBallCarrier);

        return new PlayerCommandHandlerResult
        {
            Summary = cpuOwnsBallCarrier
                ? $"Handed Bank21_22 control to the CPU ball-carrier loop{(cpuBoostApplied ? " after applying the source-visible juice/facing setup" : string.Empty)} and left the command running across frames."
                : "Skipped the CPU ball-carrier handoff because the current control gate did not resolve to CPU ownership.",
            AwaitingContinuation = cpuOwnsBallCarrier,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = null,
            PlayerControlCommandState = new PlayerControlCommandState
            {
                CommandKind = "CpuControlBallCarrier",
                ControlOwner = cpuOwnsBallCarrier ? "CPU" : "Fallthrough",
                BallCarrierAssigned = cpuOwnsBallCarrier,
                ManualControlRequested = false,
                CpuBoostApplied = cpuBoostApplied,
                QueuedFacingRefresh = cpuOwnsBallCarrier,
                QueuedVelocityInitialization = cpuOwnsBallCarrier,
                AwaitingLongRunningControlLoop = cpuOwnsBallCarrier,
            },
            ControlFlowState = null,
            PointerOverride = null,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }

    private static bool GetBoolOperand(PlayerCommandDefinition commandDefinition, string key, bool defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            && bool.TryParse(value, out bool parsedValue)
            ? parsedValue
            : defaultValue;
    }
}
