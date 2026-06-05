using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Handles a bounded Bank21_22 control-flow command family without moving script-pointer semantics into Bank19_20 host code.
/// </summary>
public interface IControlFlowCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
