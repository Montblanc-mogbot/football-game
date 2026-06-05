using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the bounded Bank21_22 pass-target ordering command family.
/// </summary>
public sealed class TargetingCommandDispatcher
{
    private readonly IReadOnlyList<ITargetingCommandHandler> handlers;

    public TargetingCommandDispatcher(IReadOnlyList<ITargetingCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new SetTargetOrderCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (ITargetingCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
