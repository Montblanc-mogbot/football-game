using System.Collections.Generic;

namespace FootballGame.Conversion.PlayCommands;

/// <summary>
/// Named summary of the command-dispatch portion of Bank21_22.
/// </summary>
public sealed record Bank21_22CommandDispatcherRecord
{
    public required string SectionName { get; init; }

    public required int SourceStartLine { get; init; }

    public required int GroupCommandCount { get; init; }

    public required int SingleCommandCount { get; init; }

    public required IReadOnlyList<string> GroupDispatchTargets { get; init; }

    public required IReadOnlyList<string> SingleDispatchTargetsSample { get; init; }

    public required IReadOnlyList<string> BridgeJumpExports { get; init; }
}
