using System.Collections.Generic;

namespace FootballGame.Conversion.PlayCommands;

/// <summary>
/// One top-level _F{...} section from Bank21_22_play_commands_on_field_logic.asm.
/// </summary>
public sealed record Bank21_22SectionRecord
{
    public required string SectionName { get; init; }

    public required int SourceStartLine { get; init; }

    public required int SourceEndLine { get; init; }

    public required string SourceStartMarker { get; init; }

    public required string SourceEndMarker { get; init; }

    public required int LineCount { get; init; }

    public required string Category { get; init; }

    public required string Notes { get; init; }

    public required IReadOnlyList<string> PrimaryEntryLabels { get; init; }

    public required IReadOnlyList<Bank21_22LabelRecord> Labels { get; init; }
}
