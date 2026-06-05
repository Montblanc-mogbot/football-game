using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches bounded Bank21_22 quarterback dropback/pass-control commands.
/// </summary>
public sealed class QuarterbackPassCommandDispatcher
{
    private readonly IReadOnlyList<IQuarterbackPassCommandHandler> handlers;

    public QuarterbackPassCommandDispatcher(IReadOnlyList<IQuarterbackPassCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new QuarterbackDropbackCommandHandler(),
            new CpuWaitToPassCommandHandler(),
            new CpuPassCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IQuarterbackPassCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
