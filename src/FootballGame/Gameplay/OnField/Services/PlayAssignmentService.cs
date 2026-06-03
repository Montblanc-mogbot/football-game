using System.Collections.Generic;

using FootballGame.Gameplay.OnField;
using FootballGame.Gameplay.OnField.CommandRuntimeBridge;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns initial script assignment plus mid-play bulk script reassignment for Bank19_20 transitions.
/// </summary>
public sealed class PlayAssignmentService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.LOAD_P1_OR_P2_OFF_PLAY_INFO,
        OnFieldRoutine.LOAD_OFF_FORMATIONS,
        OnFieldRoutine.LOAD_DEF_PLAY_INFO,
        OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
    ];

    public void LoadKickoffScripts(OnFieldGameState state, OnFieldTeam receivingTeam)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_OFF_FORMATIONS);
        state.RecordRoutine(OnFieldRoutine.LOAD_DEF_PLAY_INFO);
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        state.OffensiveFormationKey = "KICKOFF_OFF_FORMATION_ID";
        state.DefensivePlayKey = "KICKOFF_DEF_PLAY_ID";
        QueueRuntimeRequest(state, "OFFENSE_FIELD_GROUP", "KICKOFF_OFF_FORMATION_ID");
        QueueRuntimeRequest(state, "DEFENSE_FIELD_GROUP", "KICKOFF_DEF_PLAY_ID");
        state.RecordEvent($"Loaded kickoff script families with {receivingTeam} as the return side.");
    }

    public void LoadPlaySelectionScripts(OnFieldGameState state, OnFieldTeam possessionTeam, OnFieldPlayType playType)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_P1_OR_P2_OFF_PLAY_INFO);
        state.RecordRoutine(OnFieldRoutine.LOAD_OFF_FORMATIONS);
        state.RecordRoutine(OnFieldRoutine.LOAD_DEF_PLAY_INFO);
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        state.PossessionTeam = possessionTeam;
        state.PlayType = playType;
        state.OffensiveFormationKey = $"{possessionTeam}_{playType}_OFF_FORMATION";
        state.DefensivePlayKey = $"{GetOpposingTeam(possessionTeam)}_{playType}_DEF_PLAY";
        QueueRuntimeRequest(state, "OFFENSE_FIELD_GROUP", state.OffensiveFormationKey);
        QueueRuntimeRequest(state, "DEFENSE_FIELD_GROUP", state.DefensivePlayKey);
        state.RecordEvent($"Loaded play-selection script families for {possessionTeam} {playType}.");
    }

    public void ReassignForTurnoverOrReturn(OnFieldGameState state, string transitionKey)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        QueueRuntimeRequest(state, "TRANSITION_GROUP", transitionKey);
        state.RecordEvent($"Reassigned live-play scripts for transition '{transitionKey}'.");
    }

    public void ReassignForPuntCoverageAndReturn(OnFieldGameState state, OnFieldTeam puntingTeam)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        QueueRuntimeRequest(state, "PUNT_RETURN_GROUP", $"{puntingTeam}_PUNT_RETURN");
        state.RecordEvent($"Installed punt coverage/return script families after a {puntingTeam} punt.");
    }

    public void ReassignForInterceptionReturn(OnFieldGameState state, OnFieldTeam interceptingTeam)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        QueueRuntimeRequest(state, "INTERCEPTION_RETURN_GROUP", $"{interceptingTeam}_INTERCEPTION_RETURN");
        state.RecordEvent($"Installed interception-return script families for {interceptingTeam}.");
    }

    public void ReassignForOnsideRecovery(OnFieldGameState state, OnFieldTeam kickingTeam)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        QueueRuntimeRequest(state, "ONSIDE_RECOVERY_GROUP", $"{kickingTeam}_ONSIDE_RECOVERY");
        state.RecordEvent($"Installed onside-recovery pursuit script families for kickoff by {kickingTeam}.");
    }

    public void ReassignForOnsideReturn(OnFieldGameState state, OnFieldTeam recoveringTeam)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        QueueRuntimeRequest(state, "ONSIDE_RETURN_GROUP", $"{recoveringTeam}_ONSIDE_RETURN");
        state.RecordEvent($"Installed onside-return script families for recovery by {recoveringTeam}.");
    }

    public void ReassignForLooseBallRecovery(OnFieldGameState state)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        QueueRuntimeRequest(state, "LOOSE_BALL_GROUP", "LOOSE_BALL_RECOVERY");
        state.RecordEvent("Installed loose-ball recovery script families for both sides.");
    }

    public void ReassignForFumbleReturn(OnFieldGameState state, OnFieldTeam recoveringTeam)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        QueueRuntimeRequest(state, "FUMBLE_RETURN_GROUP", $"{recoveringTeam}_FUMBLE_RETURN");
        state.RecordEvent($"Installed post-fumble return script families for recovery by {recoveringTeam}.");
    }

    public void ReassignForTouchdownCelebration(OnFieldGameState state, OnFieldTeam scoringTeam, OnFieldTouchdownKind kind)
    {
        state.RecordRoutine(OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS);
        QueueRuntimeRequest(state, "TOUCHDOWN_GROUP", $"{scoringTeam}_{kind}_TOUCHDOWN");
        state.RecordEvent($"Installed touchdown celebration/cry script families for {scoringTeam} ({kind}).");
    }

    public void ApplyManControlledPlayerPolicy(OnFieldGameState state, bool includeManControlledPlayer)
    {
        string policy = includeManControlledPlayer ? "include-man" : "exclude-man";
        state.RecordEvent($"Applied play-assignment policy '{policy}' during Bank19_20 script installation.");
    }

    private static void QueueRuntimeRequest(OnFieldGameState state, string playerSlotKey, string scriptFamilyKey)
    {
        if (state.CommandRuntimeBoundary is null)
        {
            return;
        }

        foreach (PlayerCommandRuntimeHostRequest hostRequest in CommandRuntimeBoundaryHoldingArea.CreateHostRequests(state))
        {
            if (hostRequest.TriggerRoutine != OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS)
            {
                continue;
            }

            state.PendingCommandRuntimeRequests.Add(hostRequest);
            state.CommandRuntimeBoundary.PrimeExecutionContext(
                hostRequest,
                playerSlotKey,
                new PlayerCommandPointer
                {
                    ScriptFamilyKey = scriptFamilyKey,
                    InstructionOffset = 0,
                    ResumeLabel = hostRequest.BridgeSymbol,
                });
            return;
        }
    }

    private static OnFieldTeam GetOpposingTeam(OnFieldTeam team)
    {
        return team == OnFieldTeam.Player1 ? OnFieldTeam.Player2 : OnFieldTeam.Player1;
    }
}
