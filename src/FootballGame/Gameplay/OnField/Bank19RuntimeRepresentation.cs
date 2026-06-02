using System;
using System.Collections.Generic;
using System.Linq;

using FootballGame.Gameplay.OnField.Bank21Bridge;
using FootballGame.Gameplay.OnField.Services;

namespace FootballGame.Gameplay.OnField;

/// <summary>
/// Complete runtime-facing placement map for all Bank19_20 sections.
/// This is the coordinator/service-side representation of the full bank.
/// </summary>
public static class Bank19RuntimeRepresentation
{
    public static IReadOnlyList<Bank19RuntimeSectionPlacement> SectionPlacements { get; } =
    [
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.GAME_PLAY_START_CHECK_FOR_KICK_TEAM,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Top-level on-field entry routing that decides which kickoff-side phase starts the live play host.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_KICKOFF,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_PLAY_SELECT_AND_PLAY_LOAD,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_RUN_PLAY,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_PLAY_OVER_NORMAL,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_PASS_PLAY,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_SACK_OR_SCRAMBLE,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_SPECIAL_TEAMS_PLAY_TYPE_CHECK,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_PUNT_PLAY,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_FG_PLAY,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_ONSIDES_RETURN,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_PASS_TIPPED_RESULT,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_SAFETIED,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_TD,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_INTERCEPTED,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_TO_P2_POSSESSION_CHANGE,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_KICKOFF,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_PLAY_SELECT_AND_PLAY_LOAD,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_RUN_PLAY,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_PLAY_OVER_NORMAL,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_PASS_PLAY,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_SACK_OR_SCRAMBLE,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_SPECIAL_TEAMS_PLAY_TYPE_CHECK,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_PUNT_PLAY,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_FG_PLAY,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_ONSIDES_RETURN,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_PASS_TIPPED_RESULT,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_SAFETIED,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_TD,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_INTERCEPTED,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_TO_P1_POSSESSION_CHANGE,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-phase-routing'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_FOR_FIRST_DOWN_OR_TOD,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.UPDATE_HASHMARK_FOR_NEXT_SNAP,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_FOR_TD,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_FOR_TOUCHBACK,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_FOR_SAFETY,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_FOR_PLAY_OVER,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_FOR_FUMBLES_TOSS_AND_NORMAL,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.ONSIDE_AND_FUMBLE_RECOVERY_LOGIC,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P1_RECOVERS_FUMBLE,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.P2_RECOVERS_FUMBLE,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.MISC_FUMBLE_FUNCTIONS,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_FOR_QTR_OVER,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CLEAR_VARIABLES_FOR_XP_KICKOFF,
            OwnerKind = Bank19RuntimeOwnerKind.Coordinator,
            OwnerTypeName = nameof(OnFieldPlayCoordinator),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-outcome'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.END_SPECIFIC_TASKS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(TaskCoordinationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'task-coordination'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_FOR_UPDATE_BANNER,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.UPDATE_SCORE_FUNCTIONS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.DRAW_RECOVER,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.SET_GAME_STATUS_ON_FIELD_START_PLAYER_TASK,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(TaskCoordinationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'task-coordination'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.DEFENDER_CHANGE_BEFORE_HIKE,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PreSnapControlService),
            Notes = "Pre-snap defender-selection and snap-gating logic that also primes the active player to re-enter Bank21_22 command execution when the ball is snapped.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_SNAP_PUNT,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PreSnapControlService),
            Notes = "Punt snap-gating logic that shares the same pre-snap/control-handoff boundary as the broader defender-change flow.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.SET_ONFIELD_SONG,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.LOAD_P1_OR_P2_OFF_PLAY_INFO,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PlayAssignmentService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-assignment'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.LOAD_OFF_FORMATIONS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PlayAssignmentService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-assignment'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.LOAD_DEF_PLAY_INFO,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PlayAssignmentService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'play-assignment'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.LOAD_UPDATE_PLAY_CODE_FUNCTIONS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PlayAssignmentService),
            Notes = "Bulk script-assignment and reassignment helpers that copy Bank5_6 reaction pointers into player RAM and seed the per-player command runner.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.LOAD_SKILLS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PlayerSkillHydrationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'roster-skill-hydration'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.STOP_CURRENT_SONG,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.MAN_CONTROLLED_PLAYER_FUNCTIONS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PreSnapControlService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'pre-snap-control'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CPU_PLAY_LOGIC,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(CpuPlayDecisionService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'cpu-decision-support'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.SIDE_CHANGE_BANNER_AND_SONG,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.SET_PLAYERS_CLOSE_TO_PASS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PassTargetingService),
            Notes = "Pass-target and nearby-defender prioritization plus one-shot command priming for the jump/dive pass-contest handlers in Bank21_22.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.UPDATE_SCROLL_LIMITS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.START_DRAW_GAME_FIELD,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.UPDATE_STATS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(StatAccountingService),
            Notes = "Post-play stat-accounting family that should stay represented in the bank conversion but move into a dedicated accounting service in modern code.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CALCULATE_PLAY_DISTANCE,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(StatAccountingService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'stats-and-distance'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.INJURY_CHECK_NORMAL_AND_SKIP,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CHECK_IF_PLAYER_CAN_BE_INJURED,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.PLAYER_CHANGE_INJURY,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CUTSCENE,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.GENERATE_CUTSCENE_RANDOM,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.UPDATE_PASS_TARGET_AND_INDICATOR_ON_PRESS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(PassTargetingService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'pass-targeting'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.INJURY_ANIMATION,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'injury-and-cutscene'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.UPDATE_LOS_MARKERS,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(OnFieldPresentationService),
            Notes = "Runtime-owned Bank19_20 responsibility group 'presentation'.",
        },
        new Bank19RuntimeSectionPlacement
        {
            Section = Bank19SectionName.CUTSCENE_SEQUENCE_PTRS_AND_SEQUENCES,
            OwnerKind = Bank19RuntimeOwnerKind.Service,
            OwnerTypeName = nameof(InjuryCutsceneService),
            Notes = "Large cutscene-sequence table block that is semantically part of on-field outcome presentation, not controller flow.",
        },
    ];

    public static IReadOnlyList<Bank19SectionName> GetSectionsOwnedBy(string ownerTypeName)
    {
        return SectionPlacements
            .Where(placement => string.Equals(placement.OwnerTypeName, ownerTypeName, StringComparison.Ordinal))
            .Select(placement => placement.Section)
            .ToArray();
    }
}
