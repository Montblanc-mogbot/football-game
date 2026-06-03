using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the first live packet-21C pass-contest edge-case family.
/// </summary>
public sealed class PassContestCommandDispatcher
{
    private readonly IReadOnlyList<IPassContestCommandHandler> handlers;

    public PassContestCommandDispatcher(IReadOnlyList<IPassContestCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new OffensiveJumpDiveCatchPassCommandHandler(),
            new ReceiverMissedBallInterceptionWindowCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IPassContestCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
