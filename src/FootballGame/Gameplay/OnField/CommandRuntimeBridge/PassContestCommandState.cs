namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal continuation state for the first live Bank21_22 pass-contest edge-case slice.
/// </summary>
public sealed record PassContestCommandState
{
    public bool ReceiverJumpOrDiveAttemptResolved { get; init; }

    public bool DefenderOnlyInterceptionWindowActive { get; init; }

    public int RankedDefenderWindowSize { get; init; }

    public bool PreserveSourceBugByPolicy { get; init; }

    public string? ResolutionStage { get; init; }
}
