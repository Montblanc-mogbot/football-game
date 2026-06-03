namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Explicit cross-player continuation request emitted by a Bank21_22 runtime step.
/// This keeps retargeting visible in the runtime layer instead of hiding it inside Bank19_20 host state.
/// </summary>
public sealed record PlayerCommandRetargetRequest
{
    public required string SourcePlayerSlotKey { get; init; }

    public required string TargetPlayerSlotKey { get; init; }

    public required string ContinuationCommandName { get; init; }

    public required string ContinuationSourceLabel { get; init; }

    public required string Reason { get; init; }

    public required bool SkipIfTargetInvalid { get; init; }
}
