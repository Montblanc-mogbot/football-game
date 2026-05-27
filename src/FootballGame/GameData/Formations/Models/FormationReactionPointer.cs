namespace FootballGame.GameData.Formations.Models;

/// <summary>
/// One ordered Bank3 reaction pointer inside a formation or offensive execution table.
/// </summary>
public sealed record FormationReactionPointer
{
    public required int SlotIndex { get; init; }

    public required string ReactionLabel { get; init; }
}
