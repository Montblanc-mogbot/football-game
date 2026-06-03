using System.Collections.Generic;

namespace FootballGame.Conversion.PlayCommands;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm.
/// Container for the source-faithful Bank21_22 command-runtime inventory and representation notes.
/// </summary>
public sealed record Bank21_22PlayCommandInventory
{
    public required IReadOnlyList<Bank21_22SectionRecord> Sections { get; init; }

    public required IReadOnlyList<Bank21_22ConstantRecord> Constants { get; init; }

    public required Bank21_22CommandDispatcherRecord CommandDispatcher { get; init; }
}
