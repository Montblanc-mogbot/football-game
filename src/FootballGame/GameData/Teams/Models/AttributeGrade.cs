namespace FootballGame.GameData.Teams.Models;

/// <summary>
/// Source-faithful 16-step Bank1_2 attribute grade scale.
/// Backed by the original nibble ordinals rather than flattened percentages.
/// </summary>
public enum AttributeGrade : byte
{
    Grade6 = 0x0,
    Grade13 = 0x1,
    Grade19 = 0x2,
    Grade25 = 0x3,
    Grade31 = 0x4,
    Grade38 = 0x5,
    Grade44 = 0x6,
    Grade50 = 0x7,
    Grade56 = 0x8,
    Grade63 = 0x9,
    Grade69 = 0xA,
    Grade75 = 0xB,
    Grade81 = 0xC,
    Grade88 = 0xD,
    Grade94 = 0xE,
    Grade100 = 0xF,
}
