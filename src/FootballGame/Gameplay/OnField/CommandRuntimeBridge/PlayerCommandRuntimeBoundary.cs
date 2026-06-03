using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Explicit seam between Bank19_20 host services and the future Bank21_22 per-player runtime.
/// For now this only captures handoff requests and routes one bounded step into <see cref="PlayerCommandRuntime"/>.
/// </summary>
public sealed class PlayerCommandRuntimeBoundary
{
    private readonly PlayerCommandRuntime playerCommandRuntime;

    public PlayerCommandRuntimeBoundary(PlayerCommandRuntime playerCommandRuntime)
    {
        this.playerCommandRuntime = playerCommandRuntime ?? throw new ArgumentNullException(nameof(playerCommandRuntime));
    }

    public PlayerCommandExecutionContext PrimeExecutionContext(PlayerCommandRuntimeHostRequest hostRequest, string playerSlotKey, PlayerCommandPointer pointer)
    {
        return playerCommandRuntime.InstallHostRequest(hostRequest, playerSlotKey, pointer);
    }

    public PlayerCommandStepResult StepPlayerCommand(string playerSlotKey, PlayerCommandDefinition commandDefinition)
    {
        return playerCommandRuntime.StepPlayerCommand(playerSlotKey, commandDefinition);
    }

    public PlayerCommandExecutionContext? FindExecutionContext(string playerSlotKey)
    {
        foreach (PlayerCommandExecutionContext executionContext in playerCommandRuntime.ExecutionContexts)
        {
            if (string.Equals(executionContext.PlayerSlotKey, playerSlotKey, StringComparison.Ordinal))
            {
                return executionContext;
            }
        }

        return null;
    }
}
