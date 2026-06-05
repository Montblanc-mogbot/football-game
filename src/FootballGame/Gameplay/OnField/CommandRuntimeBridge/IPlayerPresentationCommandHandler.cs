using System;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Handles bounded Bank21_22 posture / wait / stat-mutation commands through the existing host/runtime seam.
/// </summary>
public interface IPlayerPresentationCommandHandler
{
    bool CanHandle(PlayerCommandDefinition commandDefinition);

    PlayerCommandHandlerResult Handle(PlayerCommandHandlerContext context);
}
