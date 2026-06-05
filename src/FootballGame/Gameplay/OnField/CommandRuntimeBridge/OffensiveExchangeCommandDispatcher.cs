using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the first live packet-21A snap-exchange receive family.
/// </summary>
public sealed class OffensiveExchangeCommandDispatcher
{
    private readonly IReadOnlyList<IOffensiveExchangeCommandHandler> handlers;

    public OffensiveExchangeCommandDispatcher(IReadOnlyList<IOffensiveExchangeCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new CenterSnapInitiatorCommandHandler(),
            new ShotgunSnapInitiatorCommandHandler(),
            new UnderCenterSnapReceiveCommandHandler(),
            new ShotgunSnapReceiveCommandHandler(),
            new FieldGoalSnapReceiveCommandHandler(),
            new HandoffExchangeCommandHandler(),
            new PitchExchangeCommandHandler(),
            new RunnerReceiveHandoffCommandHandler(),
            new RunnerFakeHandoffAnimationCommandHandler(),
            new ReceivePitchContinuationCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IOffensiveExchangeCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
