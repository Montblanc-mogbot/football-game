using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the bounded Bank21_22 special-teams kick/return command family.
/// </summary>
public sealed class SpecialTeamsCommandDispatcher
{
    private readonly IReadOnlyList<ISpecialTeamsCommandHandler> handlers;

    public SpecialTeamsCommandDispatcher(IReadOnlyList<ISpecialTeamsCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new SetAndMoveKickoffCommandHandler(),
            new KickoffCommandHandler(),
            new PuntCommandHandler(),
            new FieldGoalKickCommandHandler(),
            new ExtraPointKickCommandHandler(),
            new ReturnKickPuntCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (ISpecialTeamsCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
