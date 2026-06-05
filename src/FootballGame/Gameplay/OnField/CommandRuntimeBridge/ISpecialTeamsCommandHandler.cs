using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Handles a bounded Bank21_22 special-teams / return command family step.
/// </summary>
public interface ISpecialTeamsCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
