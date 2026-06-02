using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Bank21Bridge;

/// <summary>
/// Holds Bank19_20-to-Bank21_22 boundary responsibilities that must be revisited when the command-runtime bank is converted.
/// These sections are still represented by Bank19_20 services, but they also need explicit carry-forward visibility.
/// </summary>
public static class Bank19ToBank21BoundaryHoldingArea
{
    public static IReadOnlyList<Bank19SectionName> DeferredBridgeSections { get; } =
    [
        Bank19SectionName.DEFENDER_CHANGE_BEFORE_HIKE,
        Bank19SectionName.CHECK_SNAP_PUNT,
        Bank19SectionName.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
        Bank19SectionName.SET_PLAYERS_CLOSE_TO_PASS,
    ];

    public static IReadOnlyList<string> BridgeSymbols { get; } =
    [
        "JUMP_DEF_JUMP_DIVE_CHECK_PASS",
        "JUMP_DO_NEXT_PLAYER_COMMAND",
        "JUMP_WR_JUMP_DIVE_CHECK_PASS",
    ];
}
