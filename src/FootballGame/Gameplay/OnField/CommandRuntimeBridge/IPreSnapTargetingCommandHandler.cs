namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

public interface IPreSnapTargetingCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
