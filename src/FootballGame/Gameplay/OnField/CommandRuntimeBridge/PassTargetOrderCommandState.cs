namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal state for bounded Bank21_22 pass-target ordering commands.
/// </summary>
public sealed record PassTargetOrderCommandState
{
    public required string CommandKind { get; init; }

    public required string ReceiverPlayerSlot { get; init; }

    public required int TargetPriorityIndex { get; init; }

    public required bool InstalledIntoPassTargetArray { get; init; }

    public required bool BecameCurrentPassTarget { get; init; }
}
