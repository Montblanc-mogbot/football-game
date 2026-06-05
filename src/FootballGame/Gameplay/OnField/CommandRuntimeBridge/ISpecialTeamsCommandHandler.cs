using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Handles a bounded Bank21_22 special-teams command family without collapsing Bank19_20 host ownership.
/// </summary>
public interface ISpecialTeamsCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
