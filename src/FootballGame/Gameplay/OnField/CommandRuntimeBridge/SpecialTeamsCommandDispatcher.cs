using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the bounded Bank21_22 special-teams / return command family.
/// </summary>
public sealed class SpecialTeamsCommandDispatcher
{
    private readonly IReadOnlyList<ISpecialTeamsCommandHandler> handlers;

    public SpecialTeamsCommandDispatcher(IReadOnlyList<ISpecialTeamsCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new SetAndMoveKickoffCoverageCommandHandler(),
            new KickoffCommandHandler(),
            new PuntCommandHandler(),
            new KickFieldGoalCommandHandler(),
            new KickExtraPointCommandHandler(),
            new ReturnKickOrPuntCommandHandler(),
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
