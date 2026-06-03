using System.Collections.Generic;

using FootballGame.Gameplay.OnField;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Minimal decoded Bank21_22 command identity used by the first runtime skeleton.
/// </summary>
public sealed record PlayerCommandDefinition
{
    public required string CommandName { get; init; }

    public required string SourceLabel { get; init; }

    public required int ByteLength { get; init; }

    public required bool RequiresContinuation { get; init; }

    public required IReadOnlyList<string> SourceNotes { get; init; }

    public required IReadOnlyDictionary<string, string> OperandValues { get; init; }

    public OnFieldRoutine? TriggerRoutine { get; init; }
}
