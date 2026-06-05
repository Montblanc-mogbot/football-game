using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Handles bounded Bank21_22 quarterback dropback/pass-control commands.
/// </summary>
public interface IQuarterbackPassCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
