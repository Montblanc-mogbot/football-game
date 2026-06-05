using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the first bounded Bank21_22 player-control handoff seams.
/// </summary>
public sealed class PlayerControlCommandDispatcher
{
    private readonly IReadOnlyList<IPlayerControlCommandHandler> handlers;

    public PlayerControlCommandDispatcher(IReadOnlyList<IPlayerControlCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new CpuControlBallCarrierCommandHandler(),
            new ManualTakeControlCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IPlayerControlCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
