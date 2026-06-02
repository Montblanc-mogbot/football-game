using System;
using System.Collections.Generic;
using System.Linq;

using FootballGame.Gameplay.OnField.CommandRuntimeBridge;
using FootballGame.Gameplay.OnField.Services;

namespace FootballGame.Gameplay.OnField;

/// <summary>
/// Complete runtime-facing placement map for all Bank19_20 sections.
/// This is the coordinator/service-side representation of the full bank.
/// </summary>
public static class OnFieldRoutineOwnershipMap
{
    public static IReadOnlyList<OnFieldRoutinePlacement> RoutinePlacements { get; } =
    [
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.GAME_PLAY_START_CHECK_FOR_KICK_TEAM,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Top-level on-field entry routing that decides which kickoff-side phase starts the live play host.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_KICKOFF,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_PLAY_SELECT_AND_PLAY_LOAD,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_RUN_PLAY,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_PLAY_OVER_NORMAL,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_PASS_PLAY,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_SACK_OR_SCRAMBLE,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_SPECIAL_TEAMS_PLAY_TYPE_CHECK,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_PUNT_PLAY,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_FG_PLAY,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_ONSIDES_RETURN,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_PASS_TIPPED_RESULT,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_SAFETIED,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_TD,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_INTERCEPTED,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_TO_P2_POSSESSION_CHANGE,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_KICKOFF,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_PLAY_SELECT_AND_PLAY_LOAD,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_RUN_PLAY,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_PLAY_OVER_NORMAL,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_PASS_PLAY,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_SACK_OR_SCRAMBLE,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_SPECIAL_TEAMS_PLAY_TYPE_CHECK,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_PUNT_PLAY,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_FG_PLAY,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_ONSIDES_RETURN,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_PASS_TIPPED_RESULT,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_SAFETIED,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_TD,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_INTERCEPTED,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_TO_P1_POSSESSION_CHANGE,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_FOR_FIRST_DOWN_OR_TOD,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.UPDATE_HASHMARK_FOR_NEXT_SNAP,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_FOR_TD,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_FOR_TOUCHBACK,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_FOR_SAFETY,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_FOR_PLAY_OVER,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_FOR_FUMBLES_TOSS_AND_NORMAL,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.ONSIDE_AND_FUMBLE_RECOVERY_LOGIC,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P1_RECOVERS_FUMBLE,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.P2_RECOVERS_FUMBLE,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.MISC_FUMBLE_FUNCTIONS,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_FOR_QTR_OVER,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CLEAR_VARIABLES_FOR_XP_KICKOFF,
            OwnerKind = OnFieldOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.END_SPECIFIC_TASKS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(TaskCoordinationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'task-coordination'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_FOR_UPDATE_BANNER,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.UPDATE_SCORE_FUNCTIONS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.DRAW_RECOVER,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.SET_GAME_STATUS_ON_FIELD_START_PLAYER_TASK,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(TaskCoordinationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'task-coordination'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PreSnapControlService),
            Notes = "Pre-snap defender-selection and snap-gating logic that also primes the active player to re-enter Bank21_22 command execution when the ball is snapped.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_SNAP_PUNT,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PreSnapControlService),
            Notes = "Punt snap-gating logic that shares the same pre-snap/control-handoff boundary as the broader defender-change flow.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.SET_ONFIELD_SONG,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.LOAD_P1_OR_P2_OFF_PLAY_INFO,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PlayAssignmentService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-assignment'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.LOAD_OFF_FORMATIONS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PlayAssignmentService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-assignment'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.LOAD_DEF_PLAY_INFO,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PlayAssignmentService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-assignment'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PlayAssignmentService),
            Notes = "Bulk script-assignment and reassignment helpers that copy Bank5_6 reaction pointers into player RAM and seed the per-player command runner.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.LOAD_SKILLS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PlayerSkillHydrationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'roster-skill-hydration'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.STOP_CURRENT_SONG,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.MAN_CONTROLLED_PLAYER_FUNCTIONS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PreSnapControlService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'pre-snap-control'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CPU_PLAY_LOGIC,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(CpuPlayDecisionService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'cpu-decision-support'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.SIDE_CHANGE_BANNER_AND_SONG,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PassTargetingService),
            Notes = "Pass-target and nearby-defender prioritization plus one-shot command priming for the jump/dive pass-contest handlers in Bank21_22.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.UPDATE_SCROLL_LIMITS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.START_DRAW_GAME_FIELD,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.UPDATE_STATS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(StatAccountingService),
            Notes = "Post-play stat-accounting family that should stay represented in the bank conversion but move into a dedicated accounting service in modern code.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CALCULATE_PLAY_DISTANCE,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(StatAccountingService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'stats-and-distance'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.INJURY_CHECK_NORMAL_AND_SKIP,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CHECK_IF_PLAYER_CAN_BE_INJURED,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.PLAYER_CHANGE_INJURY,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CUTSCENE,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.GENERATE_CUTSCENE_RANDOM,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(PassTargetingService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'pass-targeting'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.INJURY_ANIMATION,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.UPDATE_LOS_MARKERS,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new OnFieldRoutinePlacement
        {
            Routine = OnFieldRoutine.CUTSCENE_SEQUENCE_PTRS_AND_SEQUENCES,
            OwnerKind = OnFieldOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Large cutscene-sequence table block that is semantically part of on-field outcome presentation, not controller flow.",
        },
    ];

    public static IReadOnlyList<OnFieldRoutine> GetSectionsOwnedBy(string ownerTypeName)
    {
        return RoutinePlacements
            .Where(placement => string.Equals(placement.OwnerTypeName, ownerTypeName, StringComparison.Ordinal))
            .Select(placement => placement.Routine)
            .ToArray();
    }
}
