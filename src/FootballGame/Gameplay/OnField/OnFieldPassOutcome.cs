namespace FootballGame.Gameplay.OnField;

/// <summary>
/// High-level host-side outcome for an in-flight Bank19_20 pass.
/// </summary>
public enum OnFieldPassOutcome
{
    None,
    InFlight,
    Complete,
    Tipped,
    Intercepted,
    Incomplete,
}
