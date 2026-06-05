using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Host-owned request describing why Bank19_20 is handing a player or script family toward the Bank21_22 command runtime seam.
/// </summary>
public sealed record PlayerCommandRuntimeHostRequest
{
    public required OnFieldRoutine TriggerRoutine { get; init; }

    public required string TriggerDescription { get; init; }

    public string? BridgeSymbol { get; init; }

    public OnFieldTeam? OffenseTeam { get; init; }

    public OnFieldTeam? DefenseTeam { get; init; }

    public string? LiveCommandNameOverride { get; init; }

    public string? LiveSourceLabelOverride { get; init; }

    public IReadOnlyDictionary<string, string>? LiveOperandOverrides { get; init; }
}
