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
        state.BallKicked = false;
        state.BallReceivedByReturnTeam = false;
        state.BallRecovered = false;
        state.RecoveredByPossessingTeam = false;
        state.TouchbackTriggered = false;
        state.PlayOverTriggered = false;
        state.TurnoverReturnActive = false;
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
        state.BallKicked = false;
        state.BallReceivedByReturnTeam = false;
        state.BallRecovered = false;
        state.RecoveredByPossessingTeam = false;
        state.TouchbackTriggered = false;
        state.PlayOverTriggered = false;
        state.TurnoverReturnActive = false;
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

        if (state.TurnoverReturnActive)
        {
            RunInterceptionReturnUntilDeadBall(state);
            return;
        }

        if (state.PlayType == OnFieldPlayType.Regular && state.IsManualPassingAllowed)
        {
            RunPassPlayLoop(state);
            return;
        }

        if (state.PlayType == OnFieldPlayType.Kickoff)
        {
            RunKickoffFlow(state);
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

        if (state.BallRecovered)
        {
            RunLooseBallRecoveryPhase(state);
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
                HandleInterceptionResult(state);
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

    private void RunKickoffFlow(OnFieldGameState state)
    {
        if (!state.BallKicked)
        {
            state.RecordEvent($"Waiting for {state.PossessionTeam} to kick off the ball.");
            return;
        }

        OnFieldTeam receivingTeam = GetOpposingTeam(state.KickoffTeam);
        if (state.CpuKickoffStrategy == OnFieldKickoffStrategy.Onside)
        {
            HandleOnsideKickResolution(state);
            return;
        }

        if (state.TouchbackTriggered)
        {
            ApplyPossessionChange(state, receivingTeam, state.IsSafetyKickoff ? "SAFETY_KICK_TOUCHBACK" : "KICKOFF_TOUCHBACK");
            state.IsSafetyKickoff = false;
            state.RecordEvent($"Resolved kickoff touchback; {receivingTeam} offense takes over.");
            QueueNextPlayOrKickoffState(state, receivingTeam, kickoffRequired: false);
            return;
        }

        if (!state.BallReceivedByReturnTeam)
        {
            state.RecordEvent($"Waiting for {receivingTeam} to field the kickoff or trigger a touchback.");
            return;
        }

        RecordReturnOutcomeChecks(state, receivingTeam, "kickoff return");
        if (!state.PlayOverTriggered)
        {
            state.RecordEvent($"Waiting for {receivingTeam} kickoff-return dead-ball resolution.");
            return;
        }

        if (state.NextPlayRequiresKickoff)
        {
            state.IsSafetyKickoff = false;
            HandleTouchdown(state, receivingTeam, OnFieldTouchdownKind.SpecialTeamsReturn);
            return;
        }

        if (state.SafetyTriggered)
        {
            ResolveSafetyOutcome(state);
            return;
        }

        ApplyPossessionChange(state, receivingTeam, state.IsSafetyKickoff ? "SAFETY_KICK_RETURN" : "KICKOFF_RETURN");
        statAccountingService.CalculatePlayDistance(state);
        statAccountingService.UpdateInGameStats(state);
        injuryCutsceneService.ResolveNormalInjuryChecks(state, receivingTeam);
        state.IsSafetyKickoff = false;
        state.RecordEvent($"Resolved kickoff return flow; {receivingTeam} now starts the next offensive play.");
        QueueNextPlayOrKickoffState(state, receivingTeam, kickoffRequired: false);
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
            ApplyPossessionChange(state, returnTeam, "PUNT_TOUCHBACK");
            QueueNextPlayOrKickoffState(state, returnTeam, kickoffRequired: false);
            return;
        }

        if (!state.BallReceivedByReturnTeam)
        {
            state.RecordEvent($"Waiting for {returnTeam} to receive the punt or trigger a touchback.");
            return;
        }

        RecordReturnOutcomeChecks(state, returnTeam, "punt return");
        if (!state.PlayOverTriggered)
        {
            state.RecordEvent($"Waiting for {returnTeam} punt-return dead-ball resolution.");
            return;
        }

        if (state.NextPlayRequiresKickoff)
        {
            HandleTouchdown(state, returnTeam, OnFieldTouchdownKind.SpecialTeamsReturn);
            return;
        }

        if (state.SafetyTriggered)
        {
            ResolveSafetyOutcome(state);
            return;
        }

        ApplyPossessionChange(state, returnTeam, "PUNT_RETURN");
        statAccountingService.CalculatePlayDistance(state);
        statAccountingService.UpdateInGameStats(state);
        injuryCutsceneService.ResolveNormalInjuryChecks(state, returnTeam);
        state.RecordEvent($"Resolved punt return flow; {returnTeam} now starts the next offensive play.");
        QueueNextPlayOrKickoffState(state, returnTeam, kickoffRequired: false);
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
        statAccountingService.CalculatePlayDistance(state);
        statAccountingService.UpdateInGameStats(state);
        injuryCutsceneService.ResolveNormalInjuryChecks(state, state.PossessionTeam);
        state.RecordEvent($"Resolved normal play-over flow for {state.PossessionTeam} and returned to play selection.");
        QueueNextPlayOrKickoffState(state, state.PossessionTeam, kickoffRequired: false);
    }

    private void ResolveTurnoverOnDowns(OnFieldGameState state)
    {
        injuryCutsceneService.ResolveNormalInjuryChecks(state, state.PossessionTeam);

        OnFieldTeam newPossessionTeam = GetOpposingTeam(state.PossessionTeam);
        ApplyPossessionChange(state, newPossessionTeam, "TURNOVER_ON_DOWNS");
        state.RecordEvent($"Resolved turnover on downs from the previous {GetOpposingTeam(newPossessionTeam)} possession.");
        state.TurnoverOnDowns = false;
        QueueNextPlayOrKickoffState(state, newPossessionTeam, kickoffRequired: false);
    }

    private void ResolveSpecialTeamsPlayOver(OnFieldGameState state)
    {
        state.RecordEvent($"Resolved special-teams play-over for {state.PossessionTeam}; switching possession to the opposing team.");
        OnFieldTeam newPossessionTeam = GetOpposingTeam(state.PossessionTeam);
        ApplyPossessionChange(state, newPossessionTeam, "SPECIAL_TEAMS_PLAY_OVER");
        QueueNextPlayOrKickoffState(state, newPossessionTeam, kickoffRequired: false);
    }

    private void ResolveBlockedPunt(OnFieldGameState state)
    {
        presentationService.PrepareKickBlockPresentation(state, state.PossessionTeam);
        playAssignmentService.ReassignForLooseBallRecovery(state);
        presentationService.PrepareLooseBallPresentation(state);

        if (!state.BallRecovered)
        {
            state.RecordEvent($"Resolved blocked punt for {state.PossessionTeam} into live loose-ball recovery.");
            return;
        }

        OnFieldTeam recoveringTeam = state.RecoveredByPossessingTeam ? state.PossessionTeam : GetOpposingTeam(state.PossessionTeam);
        if (!state.PlayOverTriggered)
        {
            playAssignmentService.ReassignForFumbleReturn(state, recoveringTeam);
            state.RecordEvent($"Blocked punt recovered by {recoveringTeam}; live return continues.");
            return;
        }

        ResolvePostBlockedPuntDeadBall(state, recoveringTeam, state.RecoveredByPossessingTeam);
    }

    private void ResolveMadeFieldGoalOrExtraPoint(OnFieldGameState state)
    {
        if (state.PlayType == OnFieldPlayType.ExtraPoint)
        {
            ResolveExtraPointExitToKickoff(state, "made");
            return;
        }

        OnFieldTeam kickingTeam = state.PossessionTeam;
        OnFieldTeam receivingTeam = GetOpposingTeam(kickingTeam);
        ApplyPossessionChange(state, receivingTeam, "MADE_FIELD_GOAL");
        state.RecordEvent($"Resolved made field goal for {kickingTeam}; transitioning to kickoff.");
        QueueNextPlayOrKickoffState(state, receivingTeam, kickoffRequired: true);
    }

    private void ResolveMissedFieldGoalOrExtraPoint(OnFieldGameState state)
    {
        if (state.PlayType == OnFieldPlayType.ExtraPoint)
        {
            ResolveExtraPointExitToKickoff(state, "missed");
            return;
        }

        OnFieldTeam newPossessionTeam = GetOpposingTeam(state.PossessionTeam);
        ApplyMissedFieldGoalSpotAdjustment(state);
        ApplyPossessionChange(state, newPossessionTeam, "MISSED_FIELD_GOAL");
        state.RecordEvent($"Resolved missed field goal for the previous possession; ball turns over at the source-directed next snap spot.");
        QueueNextPlayOrKickoffState(state, newPossessionTeam, kickoffRequired: false);
    }

    private void ResolveBlockedFieldGoalOrExtraPoint(OnFieldGameState state)
    {
        presentationService.PrepareKickBlockPresentation(state, state.PossessionTeam);

        if (state.PlayType == OnFieldPlayType.ExtraPoint)
        {
            ResolveExtraPointExitToKickoff(state, "blocked");
            return;
        }

        playAssignmentService.ReassignForLooseBallRecovery(state);
        presentationService.PrepareLooseBallPresentation(state);

        if (!state.BallRecovered)
        {
            state.RecordEvent($"Resolved blocked field goal for {state.PossessionTeam} into live loose-ball recovery.");
            return;
        }

        OnFieldTeam recoveringTeam = state.RecoveredByPossessingTeam ? state.PossessionTeam : GetOpposingTeam(state.PossessionTeam);
        if (!state.PlayOverTriggered)
        {
            playAssignmentService.ReassignForFumbleReturn(state, recoveringTeam);
            state.RecordEvent($"Blocked field goal recovered by {recoveringTeam}; live return continues.");
            return;
        }

        ResolvePostFumbleDeadBall(state, recoveringTeam, state.RecoveredByPossessingTeam);
    }

    public void HandleInterceptionResult(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        OnFieldTeam interceptingTeam = GetOpposingTeam(state.PossessionTeam);
        state.RecordRoutine(interceptingTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_INTERCEPTED : OnFieldRoutine.P2_INTERCEPTED);
        state.TurnoverReturnActive = true;
        playAssignmentService.ReassignForInterceptionReturn(state, interceptingTeam);
        presentationService.PrepareInterceptionPresentation(state, interceptingTeam);
        injuryCutsceneService.ResolveRecoveryCutscene(state, "INTERCEPTION", interceptingTeam);

        if (state.TouchbackTriggered)
        {
            ApplyPossessionChangeAfterInterception(state, interceptingTeam);
            return;
        }

        state.RecordEvent($"Started interception return flow for {interceptingTeam}.");
    }

    public void RunInterceptionReturnUntilDeadBall(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!state.TurnoverReturnActive)
        {
            state.RecordEvent("Interception return loop skipped because no turnover return is active.");
            return;
        }

        RecordReturnOutcomeChecks(state, GetOpposingTeam(state.PossessionTeam), "interception return");
        if (!state.PlayOverTriggered)
        {
            state.RecordEvent("Waiting for interception return dead-ball resolution.");
            return;
        }

        ResolvePostInterceptionDeadBall(state);
    }

    public void HandleOnsideKickResolution(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        OnFieldTeam kickingTeam = state.KickoffTeam;
        state.RecordRoutine(kickingTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_ONSIDES_RETURN : OnFieldRoutine.P2_ONSIDES_RETURN);
        playAssignmentService.ReassignForOnsideRecovery(state, kickingTeam);
        state.RecordRoutine(OnFieldRoutine.ONSIDE_AND_FUMBLE_RECOVERY_LOGIC);
        presentationService.PrepareLooseBallPresentation(state);

        if (!state.BallRecovered)
        {
            state.RecordEvent($"Waiting for onside recovery resolution after {kickingTeam} onside kick.");
            return;
        }

        ResolveOnsideRecovery(state, kickingTeam);
    }

    public void CheckForFumbleAndEnterLooseBall(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_FUMBLES_TOSS_AND_NORMAL);
        if (!state.BallRecovered)
        {
            playAssignmentService.ReassignForLooseBallRecovery(state);
            presentationService.PrepareLooseBallPresentation(state);
            state.RecordEvent("Entered loose-ball recovery phase after fumble/toss check.");
        }
    }

    public void RunLooseBallRecoveryPhase(OnFieldGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.RecordRoutine(OnFieldRoutine.ONSIDE_AND_FUMBLE_RECOVERY_LOGIC);
        if (!state.BallRecovered)
        {
            state.RecordEvent("Waiting for loose-ball recovery resolution.");
            return;
        }

        OnFieldTeam recoveringTeam = state.RecoveredByPossessingTeam ? state.PossessionTeam : GetOpposingTeam(state.PossessionTeam);
        if (state.RecoveredByPossessingTeam)
        {
            HandleFumbleRecoveredByOffense(state, recoveringTeam);
            return;
        }

        HandleFumbleRecoveredByDefense(state, recoveringTeam);
    }

    public void HandleTouchdown(OnFieldGameState state, OnFieldTeam scoringTeam, OnFieldTouchdownKind touchdownKind)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.RecordRoutine(scoringTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_TD : OnFieldRoutine.P2_TD);
        playAssignmentService.ReassignForTouchdownCelebration(state, scoringTeam, touchdownKind);
        ApplyTouchdownScoreAndPresentation(state, scoringTeam, touchdownKind);
        PrepareExtraPointOrKickoffReset(state, scoringTeam);
    }

    private void ResolvePostInterceptionDeadBall(OnFieldGameState state)
    {
        OnFieldTeam interceptingTeam = GetOpposingTeam(state.PossessionTeam);

        if (state.SafetyTriggered)
        {
            state.TurnoverReturnActive = false;
            ResolveSafetyOutcome(state);
            return;
        }

        if (state.TouchbackTriggered)
        {
            ApplyPossessionChangeAfterInterception(state, interceptingTeam);
            return;
        }

        if (state.NextPlayRequiresKickoff)
        {
            state.TurnoverReturnActive = false;
            HandleTouchdown(state, interceptingTeam, OnFieldTouchdownKind.DefensiveReturn);
            return;
        }

        ApplyPossessionChangeAfterInterception(state, interceptingTeam);
    }

    private void ApplyPossessionChangeAfterInterception(OnFieldGameState state, OnFieldTeam interceptingTeam)
    {
        state.TurnoverReturnActive = false;
        ApplyPossessionChange(state, interceptingTeam, "INTERCEPTION");
        state.RecordEvent($"Resolved interception dead-ball outcome; {interceptingTeam} offense takes over.");
        QueueNextPlayOrKickoffState(state, interceptingTeam, kickoffRequired: false);
    }

    private void ResolveOnsideRecovery(OnFieldGameState state, OnFieldTeam kickingTeam)
    {
        OnFieldTeam recoveringTeam = state.RecoveredByPossessingTeam ? kickingTeam : GetOpposingTeam(kickingTeam);
        bool recoveredByKickingTeam = recoveringTeam == kickingTeam;
        presentationService.PrepareOnsideRecoveryPresentation(state, recoveringTeam, recoveredByKickingTeam);
        injuryCutsceneService.ResolveRecoveryCutscene(state, "ONSIDE", recoveringTeam);

        if (recoveredByKickingTeam)
        {
            playAssignmentService.ReassignForOnsideReturn(state, recoveringTeam);
            RecordReturnOutcomeChecks(state, recoveringTeam, "onside return");

            if (!state.PlayOverTriggered)
            {
                state.RecordEvent($"Started live onside return flow for kicking team {recoveringTeam}.");
                return;
            }
        }

        FinalizeOnsidePossessionAndSpot(state, recoveringTeam, recoveredByKickingTeam);
    }

    private void FinalizeOnsidePossessionAndSpot(OnFieldGameState state, OnFieldTeam recoveringTeam, bool recoveredByKickingTeam)
    {
        if (state.NextPlayRequiresKickoff)
        {
            HandleTouchdown(state, recoveringTeam, OnFieldTouchdownKind.SpecialTeamsReturn);
            return;
        }

        if (state.SafetyTriggered)
        {
            ResolveSafetyOutcome(state);
            return;
        }

        ApplyPossessionChange(state, recoveringTeam, recoveredByKickingTeam ? "ONSIDE_RECOVERED_BY_KICKING_TEAM" : "ONSIDE_RETURN_END");
        state.RecordEvent($"Resolved onside recovery; {recoveringTeam} takes the next snap.");
        QueueNextPlayOrKickoffState(state, recoveringTeam, kickoffRequired: false);
    }

    private void HandleFumbleRecoveredByOffense(OnFieldGameState state, OnFieldTeam recoveringTeam)
    {
        state.RecordRoutine(recoveringTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_RECOVERS_FUMBLE : OnFieldRoutine.P2_RECOVERS_FUMBLE);
        presentationService.PrepareFumbleRecoveryPresentation(state, recoveringTeam, recoveredByPossessingTeam: true);
        injuryCutsceneService.ResolveRecoveryCutscene(state, "FUMBLE", recoveringTeam);

        if (!state.PlayOverTriggered)
        {
            playAssignmentService.ReassignForFumbleReturn(state, recoveringTeam);
            RecordReturnOutcomeChecks(state, recoveringTeam, "same-team fumble return");
            state.RecordEvent($"Fumble recovered by {recoveringTeam}; live return continues under same possession.");
            return;
        }

        ResolvePostFumbleDeadBall(state, recoveringTeam, recoveredByPossessingTeam: true);
    }

    private void HandleFumbleRecoveredByDefense(OnFieldGameState state, OnFieldTeam recoveringTeam)
    {
        state.RecordRoutine(recoveringTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_RECOVERS_FUMBLE : OnFieldRoutine.P2_RECOVERS_FUMBLE);
        presentationService.PrepareFumbleRecoveryPresentation(state, recoveringTeam, recoveredByPossessingTeam: false);
        injuryCutsceneService.ResolveRecoveryCutscene(state, "FUMBLE", recoveringTeam);

        if (!state.PlayOverTriggered)
        {
            playAssignmentService.ReassignForFumbleReturn(state, recoveringTeam);
            RecordReturnOutcomeChecks(state, recoveringTeam, "turnover fumble return");
            state.RecordEvent($"Fumble recovered by {recoveringTeam}; live turnover return continues.");
            return;
        }

        ResolvePostFumbleDeadBall(state, recoveringTeam, recoveredByPossessingTeam: false);
    }

    private void ResolvePostFumbleDeadBall(OnFieldGameState state, OnFieldTeam recoveringTeam, bool recoveredByPossessingTeam)
    {
        state.RecordRoutine(OnFieldRoutine.MISC_FUMBLE_FUNCTIONS);

        if (state.NextPlayRequiresKickoff)
        {
            HandleTouchdown(state, recoveringTeam, recoveredByPossessingTeam ? OnFieldTouchdownKind.OffensiveRun : OnFieldTouchdownKind.DefensiveReturn);
            return;
        }

        if (state.SafetyTriggered)
        {
            ResolveSafetyOutcome(state);
            return;
        }

        if (recoveredByPossessingTeam)
        {
            statAccountingService.CalculatePlayDistance(state);
            statAccountingService.UpdateInGameStats(state);
            injuryCutsceneService.ResolveNormalInjuryChecks(state, recoveringTeam);
            state.RecordEvent($"Resolved dead-ball fumble recovery by {recoveringTeam}; possession stays put.");
            QueueNextPlayOrKickoffState(state, recoveringTeam, kickoffRequired: false);
            return;
        }

        ApplyPossessionChange(state, recoveringTeam, "FUMBLE_TURNOVER");
        state.RecordEvent($"Resolved dead-ball fumble turnover; {recoveringTeam} offense takes over.");
        QueueNextPlayOrKickoffState(state, recoveringTeam, kickoffRequired: false);
    }

    private void RecordReturnOutcomeChecks(OnFieldGameState state, OnFieldTeam returnTeam, string context)
    {
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_TD);
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_FUMBLES_TOSS_AND_NORMAL);
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_PLAY_OVER);
        state.RecordEvent($"Recorded Bank19_20 return outcome checks for {context} by {returnTeam}.");
    }

    private void ResolveExtraPointExitToKickoff(OnFieldGameState state, string outcomeKey)
    {
        OnFieldTeam kickingTeam = state.PossessionTeam;
        OnFieldTeam receivingTeam = GetOpposingTeam(kickingTeam);
        state.RecordEvent($"Resolved {outcomeKey} extra-point attempt for {kickingTeam}; transitioning back into kickoff setup without a normal possession-change branch.");
        QueueNextPlayOrKickoffState(state, receivingTeam, kickoffRequired: true);
    }

    private void ApplyTouchdownScoreAndPresentation(OnFieldGameState state, OnFieldTeam scoringTeam, OnFieldTouchdownKind touchdownKind)
    {
        presentationService.PrepareTouchdownPresentation(state, scoringTeam, touchdownKind);
        injuryCutsceneService.ResolveTouchdownCutscene(state, scoringTeam, touchdownKind);
        statAccountingService.UpdateInGameStats(state);
        state.RecordEvent($"Applied touchdown score/presentation flow for {scoringTeam} ({touchdownKind}).");
    }

    private void ApplyMissedFieldGoalSpotAdjustment(OnFieldGameState state)
    {
        const int TwentyYardLine = 20;
        const int SourceTwentyYardLine = 80;

        if (state.PendingNextSnapYardLine is null || state.PendingNextSnapYardLine <= SourceTwentyYardLine)
        {
            state.PendingNextSnapYardLine = SourceTwentyYardLine;
            state.RecordEvent($"Applied the Bank19_20 missed-field-goal inside-the-{TwentyYardLine} spot adjustment for the next snap.");
        }
    }

    private void PrepareExtraPointOrKickoffReset(OnFieldGameState state, OnFieldTeam scoringTeam)
    {
        state.RecordRoutine(OnFieldRoutine.CLEAR_VARIABLES_FOR_XP_KICKOFF);
        state.KickoffTeam = scoringTeam;
        state.NextPlayRequiresKickoff = true;

        if (state.PlayType != OnFieldPlayType.ExtraPoint)
        {
            state.PlayType = OnFieldPlayType.ExtraPoint;
            state.RecordEvent($"Prepared extra-point setup after touchdown by {scoringTeam}.");
            StartFieldGoalPlay(state);
            return;
        }

        OnFieldTeam receivingTeam = GetOpposingTeam(scoringTeam);
        state.RecordEvent($"Prepared kickoff reset after scoring sequence by {scoringTeam}.");
        QueueNextPlayOrKickoffState(state, receivingTeam, kickoffRequired: true);
    }

    private void ApplyPossessionChange(OnFieldGameState state, OnFieldTeam newOffenseTeam, string reason)
    {
        HandlePossessionChange(state, newOffenseTeam);
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_TOUCHBACK);
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_SAFETY);
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_QTR_OVER);
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_FIRST_DOWN_OR_TOD);
        state.RecordRoutine(OnFieldRoutine.UPDATE_HASHMARK_FOR_NEXT_SNAP);
        statAccountingService.ResetSeriesAfterTurnover(state, newOffenseTeam);
        statAccountingService.SpotBallAndUpdateHashForNextSnap(state, newOffenseTeam);
        presentationService.PrepareSideChangePresentation(state, newOffenseTeam);
        state.RecordEvent($"Applied possession change to {newOffenseTeam} ({reason}).");
    }

    private void QueueNextPlayOrKickoffState(OnFieldGameState state, OnFieldTeam newOffenseTeam, bool kickoffRequired)
    {
        FinalizeDeadBallTransition(state);

        if (kickoffRequired)
        {
            if (!state.IsSafetyKickoff)
            {
                state.KickoffTeam = GetOpposingTeam(newOffenseTeam);
            }

            state.NextPlayRequiresKickoff = true;
            StartOnFieldGameplayLoop(state);
            return;
        }

        StartPlaySelectionAndLoad(state, newOffenseTeam, OnFieldPlayType.Regular);
    }

    private void FinalizeDeadBallTransition(OnFieldGameState state)
    {
        taskCoordinationService.EndSpecificTasks(state);
        state.RecordRoutine(OnFieldRoutine.CHECK_FOR_QTR_OVER);
        state.PlayOverTriggered = false;
        state.BallRecovered = false;
        state.RecoveredByPossessingTeam = false;
        state.BallReceivedByReturnTeam = false;
        state.TouchbackTriggered = false;
        state.SpecialTeamsCutsceneReady = false;
        state.TurnoverReturnActive = false;
        state.RecordEvent("Finalized dead-ball transition state and checked for quarter-end before the next sequence.");
    }

    private void ResolveSafetyOutcome(OnFieldGameState state)
    {
        OnFieldTeam safetiedTeam = state.PossessionTeam;
        state.RecordRoutine(safetiedTeam == OnFieldTeam.Player1 ? OnFieldRoutine.P1_SAFETIED : OnFieldRoutine.P2_SAFETIED);
        injuryCutsceneService.ResolveCutsceneState(state, "QB_SACK_SAFETY");
        injuryCutsceneService.ResolveNormalInjuryChecks(state, safetiedTeam);

        OnFieldTeam scoringTeam = GetOpposingTeam(safetiedTeam);
        ApplyPossessionChange(state, scoringTeam, "SAFETY");
        state.KickoffTeam = safetiedTeam;
        state.IsSafetyKickoff = true;
        state.SafetyTriggered = false;
        state.RecordEvent($"Resolved safety outcome; {safetiedTeam} performs the free kick and {scoringTeam} receives it.");
        QueueNextPlayOrKickoffState(state, safetiedTeam, kickoffRequired: true);
    }

    private void ResolvePostBlockedPuntDeadBall(OnFieldGameState state, OnFieldTeam recoveringTeam, bool recoveredByPuntingTeam)
    {
        state.RecordRoutine(OnFieldRoutine.MISC_FUMBLE_FUNCTIONS);

        if (state.NextPlayRequiresKickoff)
        {
            HandleTouchdown(state, recoveringTeam, OnFieldTouchdownKind.SpecialTeamsReturn);
            return;
        }

        if (state.SafetyTriggered)
        {
            ResolveSafetyOutcome(state);
            return;
        }

        if (recoveredByPuntingTeam)
        {
            statAccountingService.CalculatePlayDistance(state);
            statAccountingService.UpdateInGameStats(state);
            injuryCutsceneService.ResolveNormalInjuryChecks(state, recoveringTeam);
            state.RecordEvent($"Resolved dead-ball blocked-punt recovery by punting team {recoveringTeam}; possession stays put.");
            QueueNextPlayOrKickoffState(state, recoveringTeam, kickoffRequired: false);
            return;
        }

        ApplyPossessionChange(state, recoveringTeam, "BLOCKED_PUNT_TURNOVER");
        state.RecordEvent($"Resolved dead-ball blocked-punt turnover; {recoveringTeam} offense takes over.");
        QueueNextPlayOrKickoffState(state, recoveringTeam, kickoffRequired: false);
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
