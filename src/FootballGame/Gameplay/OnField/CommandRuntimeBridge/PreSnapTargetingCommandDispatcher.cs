using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the bounded Bank21_22 pre-snap motion and pass-target ordering seam.
/// </summary>
public sealed class PreSnapTargetingCommandDispatcher
{
    private readonly IReadOnlyList<IPreSnapTargetingCommandHandler> handlers;

    public PreSnapTargetingCommandDispatcher(IReadOnlyList<IPreSnapTargetingCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new PreSnapMotionCommandHandler(),
            new SetTargetOrderCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IPreSnapTargetingCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
