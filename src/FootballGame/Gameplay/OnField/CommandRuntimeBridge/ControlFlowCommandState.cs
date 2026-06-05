namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal control-flow state for bounded Bank21_22 branch/jump command semantics.
/// </summary>
public sealed record ControlFlowCommandState
{
    public required string CommandKind { get; init; }

    public required bool ConditionalGatePassed { get; init; }

    public required bool BranchTaken { get; init; }

    public required bool UsesRelativeOffset { get; init; }

    public required int TargetInstructionOffset { get; init; }

    public string? TargetLabel { get; init; }

    public required bool YieldsOneFrameBeforeResume { get; init; }
}
