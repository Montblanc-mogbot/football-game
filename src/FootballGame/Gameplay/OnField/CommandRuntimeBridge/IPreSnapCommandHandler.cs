using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

public interface IPreSnapCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
