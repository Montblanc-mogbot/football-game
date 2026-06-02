using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.Services;

/// <summary>
/// Source: Bank19_20_on_field_gameplay_loop.asm.
/// Owns the narrow CPU kickoff/special-teams decision logic that supports Bank19_20 host flow.
/// </summary>
public sealed class CpuPlayDecisionService
{
    public static IReadOnlyList<OnFieldRoutine> CoveredRoutines { get; } =
    [
        OnFieldRoutine.CPU_PLAY_LOGIC,
    ];

    public OnFieldKickoffStrategy ChooseKickoffStrategy(OnFieldGameState state, OnFieldTeam kickingTeam)
    {
        OnFieldKickoffStrategy strategy = state.CpuKickoffStrategy;
        state.RecordEvent($"CPU kickoff strategy evaluated for {kickingTeam}: {strategy}.");
        return strategy;
    }
}
