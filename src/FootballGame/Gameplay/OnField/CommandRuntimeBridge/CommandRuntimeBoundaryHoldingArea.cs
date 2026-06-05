using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

using static FootballGame.Gameplay.OnField.OnFieldRoutine;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Holds Bank19_20-to-Bank21_22 boundary responsibilities that must be revisited when the command-runtime bank is converted.
/// These sections are still represented by Bank19_20 services, but they also need explicit carry-forward visibility.
/// </summary>
public static class CommandRuntimeBoundaryHoldingArea
{
    public static IReadOnlyList<OnFieldRoutine> DeferredRoutines { get; } =
    [
        DEFENDER_CHANGE_BEFORE_HIKE,
        CHECK_SNAP_PUNT,
        LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
        SET_PLAYERS_CLOSE_TO_PASS,
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
                LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
                "PlayAssignmentService installs or reassigns reaction-script families before the Bank21_22 stepper resumes.",
                bridgeSymbol: "JUMP_DO_NEXT_PLAYER_COMMAND",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam)),
            CreateHostRequest(
                LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
                "Kickoff coverage/return script installation can hand the next bounded Bank21_22 step into the special-teams setup family without collapsing Bank19_20 host ownership.",
                bridgeSymbol: "JUMP_DO_NEXT_PLAYER_COMMAND",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam),
                liveCommandNameOverride: "SetAndMoveKickoffCommand",
                liveSourceLabelOverride: "SET_AND_MOVE_KICKOFF_COMMAND_START",
                liveOperandOverrides: new Dictionary<string, string>
                {
                    ["moveDuringKickoff"] = bool.FalseString,
                    ["invertXForPlayerTwo"] = bool.TrueString,
                    ["isPlayerTwo"] = (state.PossessionTeam == OnFieldTeam.Player2).ToString(),
                }),
            CreateHostRequest(
                LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
                "Kickoff coverage/return script installation can also hand the live seam into the returner icon/catch setup command once the ball is in flight.",
                bridgeSymbol: "JUMP_DO_NEXT_PLAYER_COMMAND",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam),
                liveCommandNameOverride: "ReturnKickPuntCommand",
                liveSourceLabelOverride: "RETURN_KICK_PUNT_COMMAND_START",
                liveOperandOverrides: new Dictionary<string, string>
                {
                    ["kickoffReturn"] = (state.PlayType == OnFieldPlayType.Kickoff).ToString(),
                }),
            CreateHostRequest(
                DEFENDER_CHANGE_BEFORE_HIKE,
                "PreSnapControlService primes the manually controlled defender before the hike and then hands the field back to per-player command stepping.",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam)),
            CreateHostRequest(
                CHECK_SNAP_PUNT,
                "PreSnapControlService gates punt snap timing before Bank21_22 command execution resumes.",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam)),
            CreateHostRequest(
                CHECK_SNAP_PUNT,
                "PreSnapControlService keeps the punt/field-goal snap gate explicit while the live Bank21_22 seam can sample the punt launch command family after the host declares the snap.",
                offenseTeam: state.PossessionTeam,
                defenseTeam: GetOpposingTeam(state.PossessionTeam),
                liveCommandNameOverride: state.PlayType == OnFieldPlayType.Punt ? "PuntCommand" : state.PlayType == OnFieldPlayType.ExtraPoint ? "ExtraPointKickCommand" : "FieldGoalKickCommand",
                liveSourceLabelOverride: state.PlayType == OnFieldPlayType.Punt ? "PUNT_COMMAND_START" : state.PlayType == OnFieldPlayType.ExtraPoint ? "KICK_XP_COMMAND_START" : "KICK_FG_COMMAND_START",
                liveOperandOverrides: new Dictionary<string, string>
                {
                    ["cpuControlled"] = bool.FalseString,
                    ["preserveAvoidBlockBug"] = bool.TrueString,
                }),
            CreateHostRequest(
                SET_PLAYERS_CLOSE_TO_PASS,
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
        OnFieldTeam? defenseTeam = null,
        string? liveCommandNameOverride = null,
        string? liveSourceLabelOverride = null,
        IReadOnlyDictionary<string, string>? liveOperandOverrides = null)
    {
        return new PlayerCommandRuntimeHostRequest
        {
            TriggerRoutine = triggerRoutine,
            TriggerDescription = triggerDescription,
            BridgeSymbol = bridgeSymbol,
            OffenseTeam = offenseTeam,
            DefenseTeam = defenseTeam,
            LiveCommandNameOverride = liveCommandNameOverride,
            LiveSourceLabelOverride = liveSourceLabelOverride,
            LiveOperandOverrides = liveOperandOverrides,
        };
    }

    private static OnFieldTeam GetOpposingTeam(OnFieldTeam team)
    {
        return team == OnFieldTeam.Player1 ? OnFieldTeam.Player2 : OnFieldTeam.Player1;
    }
}
