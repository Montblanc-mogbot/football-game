using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField;

/// <summary>
/// Minimal runtime state carrier for the first Bank19_20 coordinator implementation slices.
/// This is intentionally host-oriented and source-traceable, not a final full simulation state model.
/// </summary>
public sealed class OnFieldGameState
{
    public OnFieldTeam KickoffTeam { get; set; }

    public OnFieldTeam PossessionTeam { get; set; }

    public OnFieldPhase Phase { get; set; }

    public OnFieldPlayType PlayType { get; set; }

    public bool IsSafetyKickoff { get; set; }

    public bool SpecialBallStatusActive { get; private set; }

    public OnFieldKickoffStrategy CpuKickoffStrategy { get; set; }

    public string? OffensiveFormationKey { get; set; }

    public string? DefensivePlayKey { get; set; }

    public string? CurrentBannerKey { get; set; }

    public string? CurrentSongSide { get; set; }

    public bool OpensAsPassPlay { get; set; }

    public bool TurnoverOnDowns { get; set; }

    public bool SafetyTriggered { get; set; }

    public bool NextPlayRequiresKickoff { get; set; }

    public bool BallKicked { get; set; }

    public bool BallReceivedByReturnTeam { get; set; }

    public bool TouchbackTriggered { get; set; }

    public bool SpecialTeamsCutsceneReady { get; set; }

    public OnFieldKickOutcome KickOutcome { get; set; }

    public bool PassAttempted { get; set; }

    public bool BallCarrierPastLineOfScrimmage { get; set; }

    public bool BallOutOfBoundsOrRecovered { get; set; }

    public bool QuarterbackSacked { get; set; }

    public bool QuarterPassFlightComplete { get; set; }

    public bool PlayOverTriggered { get; set; }

    public OnFieldPassOutcome PassOutcome { get; set; }

    public bool IsManualPassingAllowed { get; set; }

    public bool IsSpecialTeamsPlay => PlayType is OnFieldPlayType.Kickoff or OnFieldPlayType.Punt or OnFieldPlayType.FieldGoal or OnFieldPlayType.ExtraPoint;

    public IList<OnFieldRoutine> RoutineHistory { get; } = new List<OnFieldRoutine>();

    public IList<string> EventLog { get; } = new List<string>();

    public void SetSpecialBallStatusActive(bool isActive)
    {
        SpecialBallStatusActive = isActive;
    }

    public void RecordRoutine(OnFieldRoutine routine)
    {
        RoutineHistory.Add(routine);
    }

    public void RecordEvent(string description)
    {
        EventLog.Add(description);
    }
}
