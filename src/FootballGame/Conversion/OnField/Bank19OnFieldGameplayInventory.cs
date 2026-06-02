using System.Collections.Generic;

namespace FootballGame.Conversion.OnField;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Container for the source-faithful Bank19_20 section inventory plus the modern ownership map
/// used to keep controller, service, and cross-bank boundaries explicit during conversion.
/// </summary>
public sealed record Bank19OnFieldGameplayInventory
{
    public required IReadOnlyList<Bank19EntryPointRecord> EntryPoints { get; init; }

    public required IReadOnlyList<Bank19ScriptPointerFamilyRecord> ScriptPointerFamilies { get; init; }

    public required IReadOnlyList<Bank19ExternalJumpConstantRecord> ExternalJumpConstants { get; init; }

    public required IReadOnlyList<Bank19CrossBankDependencyRecord> ExternalDependencies { get; init; }

    public required IReadOnlyList<Bank19SectionRecord> Sections { get; init; }
}
