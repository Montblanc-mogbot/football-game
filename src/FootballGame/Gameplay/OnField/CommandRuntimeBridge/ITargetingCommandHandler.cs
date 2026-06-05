using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

public interface ITargetingCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
