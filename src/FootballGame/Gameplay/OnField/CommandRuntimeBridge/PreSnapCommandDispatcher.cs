using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the bounded Bank21_22 pre-snap motion command family.
/// </summary>
public sealed class PreSnapCommandDispatcher
{
    private readonly IReadOnlyList<IPreSnapCommandHandler> handlers;

    public PreSnapCommandDispatcher(IReadOnlyList<IPreSnapCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new PreSnapMotionCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IPreSnapCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
