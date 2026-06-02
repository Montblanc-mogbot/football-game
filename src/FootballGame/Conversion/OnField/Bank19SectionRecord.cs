using System.Collections.Generic;

namespace FootballGame.Conversion.OnField;

/// <summary>
/// One top-level _F{...} section from Bank19_20_on_field_gameplay_loop.asm, including the
/// source span, major labels, modern ownership, and any explicit Bank21/22 carry-forward bridge.
/// </summary>
public sealed record Bank19SectionRecord
{
    public required string SectionName { get; init; }

    public required int SourceStartLine { get; init; }

    public required int SourceEndLine { get; init; }

    public required string SourceStartMarker { get; init; }

    public required string SourceEndMarker { get; init; }

    public required int LineCount { get; init; }

    public int? Depth { get; init; }

    public string? ParentSectionName { get; init; }

    public required Bank19ModernOwner ModernOwner { get; init; }

    public required Bank19ResponsibilityGroup ResponsibilityGroup { get; init; }

    public required IReadOnlyList<string> PrimaryEntryLabels { get; init; }

    public required IReadOnlyList<string> LabelNames { get; init; }

    public required IReadOnlyList<string> ExternalDependencySymbols { get; init; }

    public required string Notes { get; init; }

    public required bool CarryForwardToBank21_22 { get; init; }

    public string? Bank21_22CarryForwardReason { get; init; }

    public required IReadOnlyList<string> Bank21_22BridgeSymbols { get; init; }
}
