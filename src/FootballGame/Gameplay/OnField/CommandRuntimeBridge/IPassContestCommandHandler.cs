using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Handles a bounded Bank21_22 pass-contest command family without absorbing Bank19_20 host ownership.
/// </summary>
public interface IPassContestCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
