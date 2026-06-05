using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the bounded Bank21_22 posture / wait / per-player stat-mutator family.
/// </summary>
public sealed class PlayerPresentationCommandDispatcher
{
    private readonly IReadOnlyList<IPlayerPresentationCommandHandler> handlers;

    public PlayerPresentationCommandDispatcher(IReadOnlyList<IPlayerPresentationCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new SnapStanceCommandHandler(),
            new PreSnapStanceCommandHandler(),
            new FacingAndWaitCommandHandler(),
            new PlayerStatMutationCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IPlayerPresentationCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
