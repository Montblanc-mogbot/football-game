using System;
using System.Collections.Generic;

using FootballGame.Gameplay.OnField.Services;

namespace FootballGame.Gameplay.OnField;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns the host-level on-field play flow: entry, phase routing, possession changes, and play-over adjudication timing.
/// </summary>
public sealed class OnFieldPlayCoordinator
{
    private readonly PlayAssignmentService playAssignmentService;
    private readonly PlayerSkillHydrationService playerSkillHydrationService;
    private readonly TaskCoordinationService taskCoordinationService;
    private readonly OnFieldPresentationService presentationService;
    private readonly CpuPlayDecisionService cpuPlayDecisionService;
    private readonly PreSnapControlService preSnapControlService;

    public OnFieldPlayCoordinator(
        PlayAssignmentService playAssignmentService,
        PlayerSkillHydrationService playerSkillHydrationService,
        TaskCoordinationService taskCoordinationService,
        OnFieldPresentationService presentationService,
        CpuPlayDecisionService cpuPlayDecisionService,
        PreSnapControlService preSnapControlService)
    {
        this.playAssignmentService = playAssignmentService;
        this.playerSkillHydrationService = playerSkillHydrationService;
        this.taskCoordinationService = taskCoordinationService;
        this.presentationService = presentationService;
        this.cpuPlayDecisionService = cpuPlayDecisionService;
        this.preSnapControlService = preSnapControlService;
    }

    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.GAME_PLAY_START_CHECK_FOR_KICK_TEAM,
        OnFieldRoutine.P2_KICKOFF,
        OnFieldRoutine.P1_PLAY_SELECT_AND_PLAY_LOAD,
        OnFieldRoutine.P1_RUN_PLAY,
        OnFieldRoutine.P1_PLAY_OVER_NORMAL,
        OnFieldRoutine.P1_PASS_PLAY,
        OnFieldRoutine.P1_SACK_OR_SCRAMBLE,
        OnFieldRoutine.P1_SPECIAL_TEAMS_PLAY_TYPE_CHECK,
        OnFieldRoutine.P1_PUNT_PLAY,
        OnFieldRoutine.P1_FG_PLAY,
        OnFieldRoutine.P1_ONSIDES_RETURN,
        OnFieldRoutine.P1_PASS_TIPPED_RESULT,
        OnFieldRoutine.P1_SAFETIED,
        OnFieldRoutine.P1_TD,
        OnFieldRoutine.P1_INTERCEPTED,
        OnFieldRoutine.P1_TO_P2_POSSESSION_CHANGE,
        OnFieldRoutine.P1_KICKOFF,
        OnFieldRoutine.P2_PLAY_SELECT_AND_PLAY_LOAD,
        OnFieldRoutine.P2_RUN_PLAY,
        OnFieldRoutine.P2_PLAY_OVER_NORMAL,
        OnFieldRoutine.P2_PASS_PLAY,
        OnFieldRoutine.P2_SACK_OR_SCRAMBLE,
        OnFieldRoutine.P2_SPECIAL_TEAMS_PLAY_TYPE_CHECK,
        OnFieldRoutine.P2_PUNT_PLAY,
        OnFieldRoutine.P2_FG_PLAY,
        OnFieldRoutine.P2_ONSIDES_RETURN,
        OnFieldRoutine.P2_PASS_TIPPED_RESULT,
        OnFieldRoutine.P2_SAFETIED,
        OnFieldRoutine.P2_TD,
        OnFieldRoutine.P2_INTERCEPTED,
        OnFieldRoutine.P2_TO_P1_POSSESSION_CHANGE,
        OnFieldRoutine.CHECK_FOR_FIRST_DOWN_OR_TOD,
        OnFieldRoutine.UPDATE_HASHMARK_FOR_NEXT_SNAP,
        OnFieldRoutine.CHECK_FOR_TD,
        OnFieldRoutine.CHECK_FOR_TOUCHBACK,
        OnFieldRoutine.CHECK_FOR_SAFETY,
        OnFieldRoutine.CHECK_FOR_PLAY_OVER,
        OnFieldRoutine.CHECK_FOR_FUMBLES_TOSS_AND_NORMAL,
        OnFieldRoutine.ONSIDE_AND_FUMBLE_RECOVERY_LOGIC,
        OnFieldRoutine.P1_RECOVERS_FUMBLE,
        OnFieldRoutine.P2_RECOVERS_FUMBLE,
        OnFieldRoutine.MISC_FUMBLE_FUNCTIONS,
        OnFieldRoutine.CHECK_FOR_QTR_OVER,
        OnFieldRoutine.CLEAR_VARIABLES_FOR_XP_KICKOFF,
    ];

    public void StartOnFieldGameplayLoop(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.RecordRoutine(OnFieldRoutine.GAME_PLAY_START_CHECK_FOR_KICK_TEAM);
        state.SetSpecialBallStatusActive(false);
        state.RecordEvent("Entered the on-field gameplay loop and cleared special ball status.");

        if (state.KickoffTeam == OnFieldTeam.Player2)
        {
            StartPlayer2Kickoff(state);
            return;
        }

        StartPlayer1Kickoff(state);
    }

    public void StartPlayer1Kickoff(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.RecordRoutine(OnFieldRoutine.P1_KICKOFF);
        state.Phase = OnFieldPhase.OpeningKickoff;
        state.PlayType = OnFieldPlayType.Kickoff;
        state.PossessionTeam = OnFieldTeam.Player1;

        cpuPlayDecisionService.ChooseKickoffStrategy(state, OnFieldTeam.Player1);
        playAssignmentService.LoadKickoffScripts(state, OnFieldTeam.Player2);
        playerSkillHydrationService.LoadAllSkillsIntoPlayerState(state);
        playerSkillHydrationService.LoadSinglePlayerSkillsIntoPlayerState(state, OnFieldTeam.Player1, "KICKER_STARTER_ID");
        playerSkillHydrationService.LoadSinglePlayerSkillsIntoPlayerState(state, OnFieldTeam.Player2, "KR_STARTER_ID");
        taskCoordinationService.StartOnFieldTasks(state);
        presentationService.PrepareKickoffPresentation(state, OnFieldTeam.Player1, state.IsSafetyKickoff);
    }

    public void StartPlayer2Kickoff(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.RecordRoutine(OnFieldRoutine.P2_KICKOFF);
        state.Phase = OnFieldPhase.OpeningKickoff;
        state.PlayType = OnFieldPlayType.Kickoff;
        state.PossessionTeam = OnFieldTeam.Player2;

        cpuPlayDecisionService.ChooseKickoffStrategy(state, OnFieldTeam.Player2);
        playAssignmentService.LoadKickoffScripts(state, OnFieldTeam.Player1);
        playerSkillHydrationService.LoadAllSkillsIntoPlayerState(state);
        playerSkillHydrationService.LoadSinglePlayerSkillsIntoPlayerState(state, OnFieldTeam.Player2, "KICKER_STARTER_ID");
        playerSkillHydrationService.LoadSinglePlayerSkillsIntoPlayerState(state, OnFieldTeam.Player1, "KR_STARTER_ID");
        taskCoordinationService.StartOnFieldTasks(state);
        presentationService.PrepareKickoffPresentation(state, OnFieldTeam.Player2, state.IsSafetyKickoff);
    }

    public void StartPlaySelectionAndLoad(OnFieldGameState state, OnFieldTeam possessionTeam, OnFieldPlayType playType)
    {
        ArgumentNullException.ThrowIfNull(state);

        OnFieldRoutine routine = possessionTeam == OnFieldTeam.Player1
            ? OnFieldRoutine.P1_PLAY_SELECT_AND_PLAY_LOAD
            : OnFieldRoutine.P2_PLAY_SELECT_AND_PLAY_LOAD;

        state.RecordRoutine(routine);
        state.Phase = OnFieldPhase.PlaySelection;
        state.PossessionTeam = possessionTeam;
        state.PlayType = playType;

        playAssignmentService.LoadPlaySelectionScripts(state, possessionTeam, playType);
        playerSkillHydrationService.LoadAllSkillsIntoPlayerState(state);
        taskCoordinationService.StartOnFieldTasks(state);
        presentationService.PreparePlaySelectionPresentation(state, possessionTeam);
    }

    public void TransitionFromPlaySelection(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.IsSpecialTeamsPlay && state.PlayType != OnFieldPlayType.Kickoff)
        {
            StartSpecialTeamsPlay(state);
            return;
        }

        StartRegularPlay(state);
    }

    public void AdvanceActivePlayPhase(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.Phase == OnFieldPhase.PlaySelection)
        {
            TransitionFromPlaySelection(state);
            return;
        }

        state.RecordEvent($"AdvanceActivePlayPhase placeholder reached for {state.PlayType} in phase {state.Phase}.");
    }

    public void HandlePossessionChange(OnFieldGameState state, OnFieldTeam newPossessionTeam)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.PossessionTeam = newPossessionTeam;
        state.RecordEvent($"Handled a possession change to {newPossessionTeam}.");
    }

    public void ResolvePlayOverTransition(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.Phase = OnFieldPhase.PlayOver;
        taskCoordinationService.EndSpecificTasks(state);
        state.RecordEvent("Resolved the current play-over transition.");
    }

    private void StartRegularPlay(OnFieldGameState state)
    {
        state.Phase = OnFieldPhase.PreSnap;
        state.IsManualPassingAllowed = false;
        presentationService.PrepareRegularPlayPresentation(state, state.PossessionTeam);
        preSnapControlService.PrepareRegularPlayForSnap(state, state.PossessionTeam);
        playAssignmentService.ApplyManControlledPlayerPolicy(state, includeManControlledPlayer: false);

        if (state.PlayType == OnFieldPlayType.Regular)
        {
            StartRunOrPassPlay(state);
            return;
        }

        state.RecordEvent($"Regular-play entry received unsupported play type {state.PlayType}; defaulting to run/pass handling.");
        StartRunOrPassPlay(state);
    }

    private void StartRunOrPassPlay(OnFieldGameState state)
    {
        if (ShouldOpenAsPassPlay(state))
        {
            StartPassPlay(state);
            return;
        }

        StartRunPlay(state);
    }

    private void StartRunPlay(OnFieldGameState state)
    {
        state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_RUN_PLAY : OnFieldRoutine.P2_RUN_PLAY);
        state.Phase = OnFieldPhase.LivePlay;
        state.IsManualPassingAllowed = false;
        state.RecordEvent($"Started {state.PossessionTeam} run-play host flow.");
    }

    private void StartPassPlay(OnFieldGameState state)
    {
        state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_PASS_PLAY : OnFieldRoutine.P2_PASS_PLAY);
        state.Phase = OnFieldPhase.LivePlay;
        state.IsManualPassingAllowed = true;
        state.RecordEvent($"Started {state.PossessionTeam} pass-play host flow with manual passing enabled.");
    }

    private void StartSpecialTeamsPlay(OnFieldGameState state)
    {
        state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_SPECIAL_TEAMS_PLAY_TYPE_CHECK : OnFieldRoutine.P2_SPECIAL_TEAMS_PLAY_TYPE_CHECK);
        presentationService.PrepareSpecialTeamsPresentation(state, state.PossessionTeam, state.PlayType);

        switch (state.PlayType)
        {
            case OnFieldPlayType.Punt:
                StartPuntPlay(state);
                break;
            case OnFieldPlayType.FieldGoal:
            case OnFieldPlayType.ExtraPoint:
                StartFieldGoalPlay(state);
                break;
            default:
                state.RecordEvent($"Special-teams routing received unsupported play type {state.PlayType}.");
                break;
        }
    }

    private void StartPuntPlay(OnFieldGameState state)
    {
        state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_PUNT_PLAY : OnFieldRoutine.P2_PUNT_PLAY);
        state.Phase = OnFieldPhase.PreSnap;
        preSnapControlService.PreparePuntForSnap(state, state.PossessionTeam);
        playAssignmentService.ApplyManControlledPlayerPolicy(state, includeManControlledPlayer: false);
        state.RecordEvent($"Started {state.PossessionTeam} punt-play host flow.");
    }

    private void StartFieldGoalPlay(OnFieldGameState state)
    {
        state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_FG_PLAY : OnFieldRoutine.P2_FG_PLAY);
        state.Phase = OnFieldPhase.PreSnap;
        playAssignmentService.ApplyManControlledPlayerPolicy(state, includeManControlledPlayer: false);
        state.RecordEvent($"Started {state.PossessionTeam} {state.PlayType} host flow.");
    }

    private static bool ShouldOpenAsPassPlay(OnFieldGameState state)
    {
        return state.OpensAsPassPlay;
    }
}
