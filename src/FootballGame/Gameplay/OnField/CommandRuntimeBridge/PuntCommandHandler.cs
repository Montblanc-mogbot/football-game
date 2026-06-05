using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3540-3714.
/// Handles the bounded punt command family.
/// </summary>
public sealed class PuntCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "PuntCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        SpecialTeamsCommandState state = new()
        {
            CommandKind = "Punt",
            WaitedForHostSnapGate = true,
            UpdatedManualControlAndDisplay = true,
            BallCarrierAssigned = true,
            BallCarrierReleased = true,
            BallAnimationStarted = true,
            BallAnimationResolved = true,
            KickMeterStarted = true,
            KickPowerCalculated = true,
            KickDirectionCalculated = false,
            KickCutsceneQueued = true,
            TouchbackEligible = true,
            KickType = "Punt",
            ContinuationStage = "PuntFlightStarted",
            MinimumDistanceYards = 15,
            MaximumDistanceYards = 78,
        };

        return new PlayerCommandHandlerResult
        {
            Summary = "Waited for the punt snap gate, resolved the long-snap receive into punter ball-carrier ownership, ran the punt power-bar timing, calculated the source-bounded punt distance window, and released the punt into the cutscene/flight state.",
            AwaitingContinuation = true,
            RetargetRequests = Array.Empty<PlayerCommandRetargetRequest>(),
            SpecialTeamsCommandState = state,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
