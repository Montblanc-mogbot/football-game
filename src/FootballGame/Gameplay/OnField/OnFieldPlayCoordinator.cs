using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns the host-level on-field play flow: entry, phase routing, possession changes, and play-over adjudication timing.
/// </summary>
public sealed class OnFieldPlayCoordinator
{
    public static IReadOnlyList<Bank19SectionName> CoveredSections { get; } =
    [
        Bank19SectionName.GAME_PLAY_START_CHECK_FOR_KICK_TEAM,
        Bank19SectionName.P2_KICKOFF,
        Bank19SectionName.P1_PLAY_SELECT_AND_PLAY_LOAD,
        Bank19SectionName.P1_RUN_PLAY,
        Bank19SectionName.P1_PLAY_OVER_NORMAL,
        Bank19SectionName.P1_PASS_PLAY,
        Bank19SectionName.P1_SACK_OR_SCRAMBLE,
        Bank19SectionName.P1_SPECIAL_TEAMS_PLAY_TYPE_CHECK,
        Bank19SectionName.P1_PUNT_PLAY,
        Bank19SectionName.P1_FG_PLAY,
        Bank19SectionName.P1_ONSIDES_RETURN,
        Bank19SectionName.P1_PASS_TIPPED_RESULT,
        Bank19SectionName.P1_SAFETIED,
        Bank19SectionName.P1_TD,
        Bank19SectionName.P1_INTERCEPTED,
        Bank19SectionName.P1_TO_P2_POSSESSION_CHANGE,
        Bank19SectionName.P1_KICKOFF,
        Bank19SectionName.P2_PLAY_SELECT_AND_PLAY_LOAD,
        Bank19SectionName.P2_RUN_PLAY,
        Bank19SectionName.P2_PLAY_OVER_NORMAL,
        Bank19SectionName.P2_PASS_PLAY,
        Bank19SectionName.P2_SACK_OR_SCRAMBLE,
        Bank19SectionName.P2_SPECIAL_TEAMS_PLAY_TYPE_CHECK,
        Bank19SectionName.P2_PUNT_PLAY,
        Bank19SectionName.P2_FG_PLAY,
        Bank19SectionName.P2_ONSIDES_RETURN,
        Bank19SectionName.P2_PASS_TIPPED_RESULT,
        Bank19SectionName.P2_SAFETIED,
        Bank19SectionName.P2_TD,
        Bank19SectionName.P2_INTERCEPTED,
        Bank19SectionName.P2_TO_P1_POSSESSION_CHANGE,
        Bank19SectionName.CHECK_FOR_FIRST_DOWN_OR_TOD,
        Bank19SectionName.UPDATE_HASHMARK_FOR_NEXT_SNAP,
        Bank19SectionName.CHECK_FOR_TD,
        Bank19SectionName.CHECK_FOR_TOUCHBACK,
        Bank19SectionName.CHECK_FOR_SAFETY,
        Bank19SectionName.CHECK_FOR_PLAY_OVER,
        Bank19SectionName.CHECK_FOR_FUMBLES_TOSS_AND_NORMAL,
        Bank19SectionName.ONSIDE_AND_FUMBLE_RECOVERY_LOGIC,
        Bank19SectionName.P1_RECOVERS_FUMBLE,
        Bank19SectionName.P2_RECOVERS_FUMBLE,
        Bank19SectionName.MISC_FUMBLE_FUNCTIONS,
        Bank19SectionName.CHECK_FOR_QTR_OVER,
        Bank19SectionName.CLEAR_VARIABLES_FOR_XP_KICKOFF,
    ];

    public void StartOnFieldGameplayLoop()
    {
    }

    public void AdvanceActivePlayPhase()
    {
    }

    public void HandlePossessionChange()
    {
    }

    public void ResolvePlayOverTransition()
    {
    }
}
