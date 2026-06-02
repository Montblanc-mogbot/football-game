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
    private readonly StatAccountingService statAccountingService;
    private readonly InjuryCutsceneService injuryCutsceneService;
    private readonly PassTargetingService passTargetingService;

    public OnFieldPlayCoordinator(
        PlayAssignmentService playAssignmentService,
        PlayerSkillHydrationService playerSkillHydrationService,
        TaskCoordinationService taskCoordinationService,
        OnFieldPresentationService presentationService,
        CpuPlayDecisionService cpuPlayDecisionService,
        PreSnapControlService preSnapControlService,
        StatAccountingService statAccountingService,
        InjuryCutsceneService injuryCutsceneService,
        PassTargetingService passTargetingService)
    {
        this.playAssignmentService = playAssignmentService;
        this.playerSkillHydrationService = playerSkillHydrationService;
        this.taskCoordinationService = taskCoordinationService;
        this.presentationService = presentationService;
        this.cpuPlayDecisionService = cpuPlayDecisionService;
        this.preSnapControlService = preSnapControlService;
        this.statAccountingService = statAccountingService;
        this.injuryCutsceneService = injuryCutsceneService;
        this.passTargetingService = passTargetingService;
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

        if (state.PlayType == OnFieldPlayType.Regular && state.IsManualPassingAllowed)
        {
            RunPassPlayLoop(state);
            return;
        }

        if (state.PlayType == OnFieldPlayType.Punt)
        {
            RunPuntFlow(state);
            return;
        }

        if (state.PlayType is OnFieldPlayType.FieldGoal or OnFieldPlayType.ExtraPoint)
        {
            RunFieldGoalOrExtraPointFlow(state);
            return;
        }

        state.RecordEvent($"AdvanceActivePlayPhase placeholder reached for {state.PlayType} in phase {state.Phase}.");
    }

    public void HandlePossessionChange(OnFieldGameState state, OnFieldTeam newPossessionTeam)
    {
        ArgumentNullException.ThrowIfNull(state);

        OnFieldRoutine routine = state.PossessionTeam == OnFieldTeam.Player1
            ? OnFieldRoutine.P1_TO_P2_POSSESSION_CHANGE
            : OnFieldRoutine.P2_TO_P1_POSSESSION_CHANGE;

        state.RecordRoutine(routine);
        state.PossessionTeam = newPossessionTeam;
        state.RecordEvent($"Handled a possession change to {newPossessionTeam}.");
    }

    public void ResolvePlayOverTransition(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_PLAY_OVER_NORMAL : OnFieldRoutine.P2_PLAY_OVER_NORMAL);
        state.Phase = OnFieldPhase.PlayOver;

        if (state.SafetyTriggered)
        {
            ResolveSafetyOutcome(state);
            return;
        }

        if (state.TurnoverOnDowns)
        {
            ResolveTurnoverOnDowns(state);
            return;
        }

        ResolveNormalPlayOver(state);
    }

    private void StartRegularPlay(OnFieldGameState state)
    {
        state.Phase = OnFieldPhase.PreSnap;
        state.IsManualPassingAllowed = false;
        state.TurnoverOnDowns = false;
        state.SafetyTriggered = false;
        state.NextPlayRequiresKickoff = false;
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
        state.PassAttempted = false;
        state.BallCarrierPastLineOfScrimmage = false;
        state.BallOutOfBoundsOrRecovered = false;
        state.QuarterbackSacked = false;
        state.QuarterPassFlightComplete = false;
        state.PlayOverTriggered = false;
        state.PassOutcome = OnFieldPassOutcome.None;
        injuryCutsceneService.ClearCutsceneStateForPassStart(state);
        state.RecordEvent($"Started {state.PossessionTeam} pass-play host flow with manual passing enabled.");
    }

    private void RunPassPlayLoop(OnFieldGameState state)
    {
        passTargetingService.UpdatePassTargetIndicator(state);

        if (state.BallCarrierPastLineOfScrimmage && !state.PassAttempted)
        {
            TransitionPassPlayToScramble(state);
            return;
        }

        if (!state.PassAttempted)
        {
            if (state.PlayOverTriggered || state.BallOutOfBoundsOrRecovered)
            {
                ResolvePassPlayOverNoThrow(state);
                return;
            }

            state.RecordEvent($"Waiting for {state.PossessionTeam} to attempt the pass while still behind the LOS.");
            return;
        }

        WaitForQuarterPassFlight(state);

        if (state.PlayOverTriggered)
        {
            ResolvePlayOverTransition(state);
            return;
        }

        if (state.PassOutcome == OnFieldPassOutcome.None || state.PassOutcome == OnFieldPassOutcome.InFlight)
        {
            state.RecordEvent($"Waiting for the in-flight pass outcome for {state.PossessionTeam}.");
            return;
        }

        ResolvePassOutcome(state);
    }

    private void WaitForQuarterPassFlight(OnFieldGameState state)
    {
        if (state.QuarterPassFlightComplete)
        {
            passTargetingService.OrderPassCollisionPlayers(state);
            return;
        }

        state.RecordEvent("Waiting for the pass to travel one quarter of the way before ordering collision players.");
    }

    private void TransitionPassPlayToScramble(OnFieldGameState state)
    {
        state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_SACK_OR_SCRAMBLE : OnFieldRoutine.P2_SACK_OR_SCRAMBLE);
        state.IsManualPassingAllowed = false;
        playAssignmentService.ApplyManControlledPlayerPolicy(state, includeManControlledPlayer: false);
        state.RecordEvent($"Transitioned {state.PossessionTeam} pass flow into scramble/chase behavior after crossing the LOS.");
    }

    private void ResolvePassOutcome(OnFieldGameState state)
    {
        switch (state.PassOutcome)
        {
            case OnFieldPassOutcome.Complete:
                state.RecordEvent($"Resolved completed pass outcome for {state.PossessionTeam}; live play continues.");
                break;
            case OnFieldPassOutcome.Tipped:
                state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_PASS_TIPPED_RESULT : OnFieldRoutine.P2_PASS_TIPPED_RESULT);
                state.RecordEvent($"Resolved tipped-pass host routing for {state.PossessionTeam}.");
                break;
            case OnFieldPassOutcome.Intercepted:
                state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_INTERCEPTED : OnFieldRoutine.P2_INTERCEPTED);
                HandlePossessionChange(state, GetOpposingTeam(state.PossessionTeam));
                playAssignmentService.ReassignForTurnoverOrReturn(state, "interception-return");
                state.RecordEvent("Resolved interception outcome and installed interception-return host context.");
                break;
            case OnFieldPassOutcome.Incomplete:
                ResolveIncompletePass(state);
                break;
        }
    }

    private void ResolveIncompletePass(OnFieldGameState state)
    {
        state.IsManualPassingAllowed = false;
        state.PlayOverTriggered = true;
        presentationService.PrepareIncompletePassPresentation(state, state.PossessionTeam);

        if (state.TurnoverOnDowns)
        {
            ResolveTurnoverOnDowns(state);
            return;
        }

        ResolveNormalPlayOver(state);
    }

    private void ResolvePassPlayOverNoThrow(OnFieldGameState state)
    {
        state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_SACK_OR_SCRAMBLE : OnFieldRoutine.P2_SACK_OR_SCRAMBLE);

        if (!state.QuarterbackSacked)
        {
            state.RecordEvent($"Resolved no-throw pass outcome for {state.PossessionTeam} without a sack; falling back to normal play-over logic.");
            ResolvePlayOverTransition(state);
            return;
        }

        ResolveQuarterbackSackOutcome(state);
    }

    private void ResolveQuarterbackSackOutcome(OnFieldGameState state)
    {
        bool sideChange = state.TurnoverOnDowns;
        bool safety = state.SafetyTriggered;
        presentationService.PrepareQuarterbackSackPresentation(state, state.PossessionTeam, sideChange, safety);
        injuryCutsceneService.ResolveCutsceneState(state, safety ? "QB_SACK_SAFETY" : sideChange ? "QB_SACK_SIDE_CHANGE" : "QB_SACK");

        if (safety)
        {
            ResolveSafetyOutcome(state);
            return;
        }

        if (sideChange)
        {
            ResolveTurnoverOnDowns(state);
            return;
        }

        ResolveNormalPlayOver(state);
    }

    private void RunPuntFlow(OnFieldGameState state)
    {
        if (!state.BallKicked)
        {
            if (state.PlayOverTriggered)
            {
                ResolveSpecialTeamsPlayOver(state);
                return;
            }

            state.RecordEvent($"Waiting for {state.PossessionTeam} to punt the ball.");
            return;
        }

        if (!state.SpecialTeamsCutsceneReady)
        {
            state.RecordEvent("Waiting for the punt cutscene/flight staging to finish before installing return scripts.");
            return;
        }

        if (state.KickOutcome == OnFieldKickOutcome.Blocked)
        {
            ResolveBlockedPunt(state);
            return;
        }

        OnFieldTeam returnTeam = GetOpposingTeam(state.PossessionTeam);
        playAssignmentService.ReassignForPuntCoverageAndReturn(state, state.PossessionTeam);
        presentationService.PreparePuntReturnPresentation(state, returnTeam);

        if (state.TouchbackTriggered)
        {
            HandlePossessionChange(state, returnTeam);
            presentationService.PrepareSideChangePresentation(state, returnTeam);
            StartPlaySelectionAndLoad(state, returnTeam, OnFieldPlayType.Regular);
            return;
        }

        if (!state.BallReceivedByReturnTeam)
        {
            state.RecordEvent($"Waiting for {returnTeam} to receive the punt or trigger a touchback.");
            return;
        }

        HandlePossessionChange(state, returnTeam);
        presentationService.PrepareSideChangePresentation(state, returnTeam);
        statAccountingService.CalculatePlayDistance(state);
        statAccountingService.UpdateInGameStats(state);
        injuryCutsceneService.ResolveNormalInjuryChecks(state, returnTeam);
        state.RecordEvent($"Resolved punt return flow; {returnTeam} now starts the next offensive play.");
        StartPlaySelectionAndLoad(state, returnTeam, OnFieldPlayType.Regular);
    }

    private void RunFieldGoalOrExtraPointFlow(OnFieldGameState state)
    {
        if (!state.BallKicked)
        {
            if (state.PlayOverTriggered)
            {
                ResolveSpecialTeamsPlayOver(state);
                return;
            }

            state.RecordEvent($"Waiting for {state.PossessionTeam} to kick the {state.PlayType}.");
            return;
        }

        if (!state.SpecialTeamsCutsceneReady)
        {
            state.RecordEvent($"Waiting for the {state.PlayType} cutscene/flight resolution to finish.");
            return;
        }

        switch (state.KickOutcome)
        {
            case OnFieldKickOutcome.Blocked:
                ResolveBlockedFieldGoalOrExtraPoint(state);
                return;
            case OnFieldKickOutcome.Made:
                ResolveMadeFieldGoalOrExtraPoint(state);
                return;
            case OnFieldKickOutcome.Missed:
                ResolveMissedFieldGoalOrExtraPoint(state);
                return;
            default:
                state.RecordEvent($"No final {state.PlayType} outcome is available yet for {state.PossessionTeam}.");
                return;
        }
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
        state.TurnoverOnDowns = false;
        state.SafetyTriggered = false;
        state.NextPlayRequiresKickoff = false;
        state.BallKicked = false;
        state.BallReceivedByReturnTeam = false;
        state.TouchbackTriggered = false;
        state.SpecialTeamsCutsceneReady = false;
        state.KickOutcome = OnFieldKickOutcome.None;
        playerSkillHydrationService.LoadSpecialTeamsSkillOverrides(state, state.PossessionTeam, OnFieldPlayType.Punt);
        preSnapControlService.PreparePuntForSnap(state, state.PossessionTeam);
        playAssignmentService.ApplyManControlledPlayerPolicy(state, includeManControlledPlayer: false);
        state.RecordEvent($"Started {state.PossessionTeam} punt-play host flow.");
    }

    private void StartFieldGoalPlay(OnFieldGameState state)
    {
        state.RecordRoutine(state.PossessionTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_FG_PLAY : OnFieldRoutine.P2_FG_PLAY);
        state.Phase = OnFieldPhase.PreSnap;
        state.TurnoverOnDowns = false;
        state.SafetyTriggered = false;
        state.NextPlayRequiresKickoff = false;
        state.BallKicked = false;
        state.BallReceivedByReturnTeam = false;
        state.TouchbackTriggered = false;
        state.SpecialTeamsCutsceneReady = false;
        state.KickOutcome = OnFieldKickOutcome.None;
        playerSkillHydrationService.LoadSpecialTeamsSkillOverrides(state, state.PossessionTeam, state.PlayType);
        presentationService.PrepareFieldGoalPresentation(state, state.PossessionTeam, state.PlayType);
        playAssignmentService.ApplyManControlledPlayerPolicy(state, includeManControlledPlayer: false);
        state.RecordEvent($"Started {state.PossessionTeam} {state.PlayType} host flow.");
    }

    private void ResolveNormalPlayOver(OnFieldGameState state)
    {
        taskCoordinationService.EndSpecificTasks(state);
        statAccountingService.CalculatePlayDistance(state);
        statAccountingService.UpdateInGameStats(state);
        injuryCutsceneService.ResolveNormalInjuryChecks(state, state.PossessionTeam);
        state.RecordEvent($"Resolved normal play-over flow for {state.PossessionTeam} and returned to play selection.");
        StartPlaySelectionAndLoad(state, state.PossessionTeam, OnFieldPlayType.Regular);
    }

    private void ResolveTurnoverOnDowns(OnFieldGameState state)
    {
        taskCoordinationService.EndSpecificTasks(state);
        injuryCutsceneService.ResolveNormalInjuryChecks(state, state.PossessionTeam);

        OnFieldTeam newPossessionTeam = GetOpposingTeam(state.PossessionTeam);
        HandlePossessionChange(state, newPossessionTeam);
        state.RecordEvent($"Resolved turnover on downs from the previous {GetOpposingTeam(newPossessionTeam)} possession.");
        state.TurnoverOnDowns = false;
        StartPlaySelectionAndLoad(state, newPossessionTeam, OnFieldPlayType.Regular);
    }

    private void ResolveSpecialTeamsPlayOver(OnFieldGameState state)
    {
        state.RecordEvent($"Resolved special-teams play-over for {state.PossessionTeam}; switching possession to the opposing team.");
        OnFieldTeam newPossessionTeam = GetOpposingTeam(state.PossessionTeam);
        HandlePossessionChange(state, newPossessionTeam);
        StartPlaySelectionAndLoad(state, newPossessionTeam, OnFieldPlayType.Regular);
    }

    private void ResolveBlockedPunt(OnFieldGameState state)
    {
        state.RecordEvent($"Resolved blocked punt outcome for {state.PossessionTeam}; treating it as an immediate side change.");
        OnFieldTeam newPossessionTeam = GetOpposingTeam(state.PossessionTeam);
        HandlePossessionChange(state, newPossessionTeam);
        StartPlaySelectionAndLoad(state, newPossessionTeam, OnFieldPlayType.Regular);
    }

    private void ResolveMadeFieldGoalOrExtraPoint(OnFieldGameState state)
    {
        if (state.PlayType == OnFieldPlayType.ExtraPoint)
        {
            state.RecordEvent($"Resolved made extra-point attempt for {state.PossessionTeam}; special-teams phase exits without a possession flip.");
            return;
        }

        OnFieldTeam kickingTeam = state.PossessionTeam;
        HandlePossessionChange(state, GetOpposingTeam(kickingTeam));
        state.KickoffTeam = kickingTeam;
        state.NextPlayRequiresKickoff = true;
        state.RecordEvent($"Resolved made field goal for {kickingTeam}; transitioning to kickoff.");
        StartOnFieldGameplayLoop(state);
    }

    private void ResolveMissedFieldGoalOrExtraPoint(OnFieldGameState state)
    {
        if (state.PlayType == OnFieldPlayType.ExtraPoint)
        {
            state.RecordEvent($"Resolved missed extra-point attempt for {state.PossessionTeam}; special-teams phase exits without a possession flip.");
            return;
        }

        HandlePossessionChange(state, GetOpposingTeam(state.PossessionTeam));
        state.RecordEvent($"Resolved missed field goal for the previous possession; ball turns over at the new LOS.");
        StartPlaySelectionAndLoad(state, state.PossessionTeam, OnFieldPlayType.Regular);
    }

    private void ResolveBlockedFieldGoalOrExtraPoint(OnFieldGameState state)
    {
        presentationService.PrepareKickBlockPresentation(state, state.PossessionTeam);

        if (state.PlayType == OnFieldPlayType.ExtraPoint)
        {
            state.RecordEvent($"Resolved blocked extra-point attempt for {state.PossessionTeam}; play converts into live recovery/run logic outside this slice.");
            return;
        }

        state.RecordEvent($"Resolved blocked field goal for {state.PossessionTeam}; play converts into live recovery/run logic outside this slice.");
    }

    private void ResolveSafetyOutcome(OnFieldGameState state)
    {
        taskCoordinationService.EndSpecificTasks(state);
        injuryCutsceneService.ResolveCutsceneState(state, "QB_SACK_SAFETY");
        injuryCutsceneService.ResolveNormalInjuryChecks(state, state.PossessionTeam);

        OnFieldTeam kickingTeam = state.PossessionTeam;
        OnFieldTeam scoringTeam = GetOpposingTeam(state.PossessionTeam);
        HandlePossessionChange(state, scoringTeam);
        state.KickoffTeam = kickingTeam;
        state.IsSafetyKickoff = true;
        state.NextPlayRequiresKickoff = true;
        state.SafetyTriggered = false;
        state.RecordEvent($"Resolved safety outcome; {scoringTeam} now receives the next kickoff.");
        StartOnFieldGameplayLoop(state);
    }

    private static OnFieldTeam GetOpposingTeam(OnFieldTeam team)
    {
        return team == OnFieldTeam.Player1 ? OnFieldTeam.Player2 : OnFieldTeam.Player1;
    }

    private static bool ShouldOpenAsPassPlay(OnFieldGameState state)
    {
        return state.OpensAsPassPlay;
    }
}
