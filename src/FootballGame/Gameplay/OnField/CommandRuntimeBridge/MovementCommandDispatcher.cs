using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the first bounded Bank21_22 movement-target command seam.
/// </summary>
public sealed class MovementCommandDispatcher
{
    private readonly IReadOnlyList<IMovementCommandHandler> handlers;

    public MovementCommandDispatcher(IReadOnlyList<IMovementCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new MoveRelativeCommandHandler(),
            new MoveAbsoluteVsSnapLocationCommandHandler(),
            new MoveAbsoluteVsMiddleCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IMovementCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
