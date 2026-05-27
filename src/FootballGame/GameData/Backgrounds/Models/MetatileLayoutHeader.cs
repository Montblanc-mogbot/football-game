namespace FootballGame.GameData.Backgrounds.Models;

/// <summary>
/// Source: Bank3_formation_metatile_data.asm metatile header.
/// Represents the semantic meaning of the seven-byte layout prefix.
/// </summary>
public sealed record MetatileLayoutHeader
{
    public required byte ChrBankPrimary { get; init; }

    public required byte ChrBankSecondary { get; init; }

    public required byte TileBankOffset { get; init; }

    public required byte BackgroundPaletteSetIndex { get; init; }

    public required byte HeightInMetatiles { get; init; }

    public required byte WidthInMetatiles { get; init; }

    public required byte StartingScreenLocation { get; init; }
}
