using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Source: Bank21_22_play_commands_on_field_logic.asm:3613-3760.
/// Handles the bounded extra-point kick command family via the shared FG/XP start path.
/// </summary>
public sealed class KickExtraPointCommandHandler : ISpecialTeamsCommandHandler
{
    public bool CanHandle(PlayerCommandDefinition commandDefinition)
    {
        ArgumentNullException.ThrowIfNull(commandDefinition);
        return commandDefinition.CommandName is "KickExtraPointCommand";
    }

    public PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return KickFieldGoalCommandHandler.CreateKickResult(context, kickType: "ExtraPoint", manualKickDefault: false);
    }
}
