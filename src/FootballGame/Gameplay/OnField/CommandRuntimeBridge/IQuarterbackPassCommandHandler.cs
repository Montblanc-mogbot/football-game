using FootballGame.Gameplay.OnField.CommandRuntimeBridge;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

public interface IQuarterbackPassCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
