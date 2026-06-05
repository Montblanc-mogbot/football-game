using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3079-3212.
/// Handles the bounded per-player running/max-speed and hitting-power mutator family.
/// </summary>
public sealed class PlayerStatMutationCommandHandler : IPlayerPresentationCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is
            "SetRunningSpeedCommand"
            or "SetMaxSpeedCommand"
            or "IncrementDecrementRunningSpeedCommand"
            or "IncrementDecrementMaxSpeedCommand"
            or "SetHittingPowerCommand"
            or "ChangeHittingPowerCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string commandKind = context.CommandDefinition.CommandName;
        int amount = GetIntOperand(context.CommandDefinition, "amount", 0);
        int targetValue = GetIntOperand(context.CommandDefinition, "targetValue", amount);
        bool usesDefaultReset = GetBoolOperand(context.CommandDefinition, "usesDefaultReset", false);
        bool appliesCpuJuiceBoost = GetBoolOperand(context.CommandDefinition, "appliesCpuJuiceBoost", false);

        return new PlayerCommandHandlerResult
        {
            Summary = $"Captured the Bank21_22 {commandKind} player-stat mutation so the runtime now records running-speed/max-speed/hitting-power changes and the required speed/velocity refresh on the existing host seam.",
            AwaitingContinuation = false,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            PlayerPresentationCommandState = new PlayerPresentationCommandState
            {
                CommandKind = commandKind,
                StatMutationKind = commandKind,
                StatMutationAmount = amount,
                TargetStatValue = targetValue,
                TargetsRunningSpeedIndex = commandKind is "SetRunningSpeedCommand" or "IncrementDecrementRunningSpeedCommand",
                TargetsMaxSpeed = commandKind is "SetMaxSpeedCommand" or "IncrementDecrementMaxSpeedCommand",
                TargetsHittingPower = commandKind is "SetHittingPowerCommand" or "ChangeHittingPowerCommand",
                UsesOffenseDefenseDefaultReset = usesDefaultReset,
                AppliesCpuJuiceBoost = appliesCpuJuiceBoost,
                QueuedSpeedRefresh = commandKind is not "SetHittingPowerCommand" and not "ChangeHittingPowerCommand",
                QueuedVelocityInitialization = commandKind is not "SetHittingPowerCommand" and not "ChangeHittingPowerCommand",
            },
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

    private static int GetIntOperand(PlayerCommandDefinition commandDefinition, string key, int defaultValue)
    {
        return commandDefinition.OperandValues.TryGetValue(key, out string? value)
            && int.TryParse(value, out int parsedValue)
            ? parsedValue
            : defaultValue;
    }
}
