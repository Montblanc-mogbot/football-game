using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3421-3538.
/// Handles the bounded kickoff command family.
/// </summary>
public sealed class KickoffCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "KickoffCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool onsideAware = context.CommandDefinition.OperandValues.TryGetValue("onsideAware", out string? onsideValue)
            && bool.TryParse(onsideValue, out bool parsed)
            && parsed;

        SpecialTeamsCommandState state = new()
        {
            CommandKind = "Kickoff",
            UpdatedManualControlAndDisplay = true,
            KickMeterStarted = true,
            KickDirectionCalculated = true,
            KickPowerCalculated = true,
            BallAnimationStarted = true,
            BallCarrierReleased = true,
            KickCutsceneQueued = false,
            FakeOrOnsideAware = onsideAware,
            KickType = onsideAware ? "OnsideOrKickoff" : "Kickoff",
            ContinuationStage = "KickReleased",
            WaitFrames = 16,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = onsideAware
                ? "Set the kicker presentation state, ran the kickoff power-bar timing, honored the source onside-vs-normal meter branch, and released the kickoff with a runtime-owned direction/power decision."
                : "Set the kicker presentation state, ran the kickoff power-bar timing, and released the kickoff with a runtime-owned direction/power decision.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = state,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
