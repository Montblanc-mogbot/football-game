namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Production-facing handler contract for the first post-21A defensive reaction family.
/// </summary>
public interface IDefensiveReactionCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
