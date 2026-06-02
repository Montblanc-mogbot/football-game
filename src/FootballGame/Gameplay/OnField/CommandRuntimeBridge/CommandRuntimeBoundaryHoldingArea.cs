using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Holds Bank19_20-to-Bank21_22 boundary responsibilities that must be revisited when the command-runtime bank is converted.
/// These sections are still represented by Bank19_20 services, but they also need explicit carry-forward visibility.
/// </summary>
public static class CommandRuntimeBoundaryHoldingArea
{
    public static IReadOnlyList<OnFieldRoutine> DeferredRoutines { get; } =
    [
        OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE,
        OnFieldRoutine.CHECK_SNAP_PUNT,
        OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
        OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS,
    ];

    public static IReadOnlyList<string> BridgeSymbols { get; } =
    [
        "JUMP_DEF_JUMP_DIVE_CHECK_PASS",
        "JUMP_DO_NEXT_PLAYER_COMMAND",
        "JUMP_WR_JUMP_DIVE_CHECK_PASS",
    ];
}
