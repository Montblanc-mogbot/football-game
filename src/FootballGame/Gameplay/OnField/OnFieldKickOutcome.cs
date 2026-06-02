namespace FootballGame.Gameplay.OnField;

/// <summary>
/// High-level host-side outcome for punt, field-goal, and extra-point kicks.
/// </summary>
public enum OnFieldKickOutcome
{
    None,
    InFlight,
    Blocked,
    Made,
    Missed,
}
