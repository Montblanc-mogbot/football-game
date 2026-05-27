namespace FootballGame.GameData.Defense.Models;

/// <summary>
/// One ordered Bank4 defensive reaction pointer inside a defensive execution or special-play table.
/// </summary>
public sealed record DefensiveReactionPointer
{
    public required int SlotIndex { get; init; }

    public required string ReactionLabel { get; init; }
}
