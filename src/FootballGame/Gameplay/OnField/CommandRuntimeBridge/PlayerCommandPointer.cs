namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source-faithful cursor over one player's current Bank5_6 reaction script family.
/// </summary>
public sealed record PlayerCommandPointer
{
    public static PlayerCommandPointer Empty { get; } = new()
    {
        ScriptFamilyKey = "UNASSIGNED",
        InstructionOffset = 0,
        ResumeLabel = null,
    };

    public required string ScriptFamilyKey { get; init; }

    public required int InstructionOffset { get; init; }

    public string? ResumeLabel { get; init; }

    public PlayerCommandPointer Advance(int byteLength)
    {
        return this with { InstructionOffset = InstructionOffset + byteLength };
    }

    public PlayerCommandPointer SetInstructionOffset(int instructionOffset, string? resumeLabel = null)
    {
        return this with
        {
            InstructionOffset = instructionOffset,
            ResumeLabel = resumeLabel,
        };
    }
}
