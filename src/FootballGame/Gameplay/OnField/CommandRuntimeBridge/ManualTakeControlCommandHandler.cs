using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3228-3315.
/// Handles the bounded manual-control handoff command family.
/// </summary>
public sealed class ManualTakeControlCommandHandler : IPlayerControlCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "ManualTakeControlCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool isManControlled = GetBoolOperand(context.CommandDefinition, "isManControlled", false);
        bool defenseBranch = GetBoolOperand(context.CommandDefinition, "defenseBranch", false);
        bool quarterbackHasBall = GetBoolOperand(context.CommandDefinition, "quarterbackHasBall", false);

        return new PlayerCommandHandlerResult
        {
            Summary = isManControlled
                ? $"Entered the Bank21_22 {(defenseBranch ? "defensive" : "offensive")} manual-control path, staged the facing/input reset, and left the command inside its long-running man-control loop{(quarterbackHasBall ? " with quarterback/pass ownership preserved" : string.Empty)}."
                : "Skipped the manual-control handoff because the current team-control gate did not resolve to man control.",
            AwaitingContinuation = isManControlled,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            DefensiveReactionState = null,
            PassContestState = null,
            OffensiveExchangeState = null,
            MovementCommandState = null,
            PlayerControlCommandState = new PlayerControlCommandState
            {
                CommandKind = "ManualTakeControl",
                ControlOwner = isManControlled ? "Manual" : "Fallthrough",
                BallCarrierAssigned = quarterbackHasBall,
                ManualControlRequested = isManControlled,
                CpuBoostApplied = false,
                QueuedFacingRefresh = isManControlled,
                QueuedVelocityInitialization = isManControlled,
                AwaitingLongRunningControlLoop = isManControlled,
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
