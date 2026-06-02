namespace FootballGame.Gameplay.OnField;

/// <summary>
/// High-level host phase for the live on-field loop.
/// </summary>
public enum OnFieldPhase
{
    OpeningKickoff,
    PlaySelection,
    PreSnap,
    LivePlay,
    PlayOver,
}
