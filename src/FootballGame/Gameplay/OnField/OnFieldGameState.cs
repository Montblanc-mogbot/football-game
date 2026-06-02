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
