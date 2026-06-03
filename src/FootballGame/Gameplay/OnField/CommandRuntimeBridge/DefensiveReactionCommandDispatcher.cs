using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the first live post-21A defensive reaction family without collapsing Bank19_20 host ownership.
/// </summary>
public sealed class DefensiveReactionCommandDispatcher
{
    private readonly IReadOnlyList<IDefensiveReactionCommandHandler> handlers;

    public DefensiveReactionCommandDispatcher(IReadOnlyList<IDefensiveReactionCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new ManCoverageAssignmentCommandHandler(),
            new MirrorBallCarrierBehindLineCommandHandler(),
            new AggressiveBallCarrierChaseCommandHandler(),
            new ConservativeBallCarrierChaseCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IDefensiveReactionCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return new PlayerCommandHandlerResult
        {
            Summary = $"No defensive-reaction handler matched '{context.CommandDefinition.CommandName}'.",
            AwaitingContinuation = context.CommandDefinition.RequiresContinuation,
            DefensiveReactionState = null,
            SourceNotes = context.CommandDefinition.SourceNotes,
        };
    }
}
