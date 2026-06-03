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

    public static IReadOnlyList<PlayerCommandRuntimeHostRequest> CreateHostRequests(OnFieldGameState state)
    {
        return
        [
            CreateHostRequest(
                OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
                "PlayAssignmentService installs or reassigns reaction-script families before the Bank21_22 stepper resumes.",
                bridgeSymbol: "JUMP_DO_NEXT_PLAYER_COMMAND",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam)),
            CreateHostRequest(
                OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE,
                "PreSnapControlService primes the manually controlled defender before the hike and then hands the field back to per-player command stepping.",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam)),
            CreateHostRequest(
                OnFieldRoutine.CHECK_SNAP_PUNT,
                "PreSnapControlService gates punt snap timing before Bank21_22 command execution resumes.",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam)),
            CreateHostRequest(
                OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS,
                "PassTargetingService ranks the nearby receiver/defender set and primes the jump/dive command-runtime jump targets.",
                bridgeSymbol: "JUMP_WR_JUMP_DIVE_CHECK_PASS",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam)),
        ];
    }

    private static PlayerCommandRuntimeHostRequest CreateHostRequest(
        OnFieldRoutine triggerRoutine,
        string triggerDescription,
        string? bridgeSymbol = null,
        OnFieldTeam? offenseTeam = null,
        OnFieldTeam? defenseTeam = null)
    {
        return new PlayerCommandRuntimeHostRequest
        {
            TriggerRoutine = triggerRoutine,
            TriggerDescription = triggerDescription,
            BridgeSymbol = bridgeSymbol,
            OffenseTeam = offenseTeam,
            DefenseTeam = defenseTeam,
        };
    }

    private static OnFieldTeam GetOpposingTeam(OnFieldTeam team)
    {
        return team == OnFieldTeam.Player1 ? OnFieldTeam.Player2 : OnFieldTeam.Player1;
    }
}
