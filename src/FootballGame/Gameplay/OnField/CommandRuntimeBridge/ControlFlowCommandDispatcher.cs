using System;
using System.Collections.Generic;

namespace FootballGame.Gameplay.OnField.CommandRuntimeBridge;

/// <summary>
/// Dispatches the first live Bank21_22 control-flow command family.
/// </summary>
public sealed class ControlFlowCommandDispatcher
{
    private readonly IReadOnlyList<IControlFlowCommandHandler> handlers;

    public ControlFlowCommandDispatcher(IReadOnlyList<IControlFlowCommandHandler>? handlers = null)
    {
        this.handlers = handlers ??
        [
            new DoActionIfCpuJumpCommandHandler(),
            new CpuJumpBasedOnJuiceCommandHandler(),
            new IfCpuJumpCommandHandler(),
            new BranchCommandHandler(),
            new JumpCommandHandler(),
        ];
    }

    public PlayerCommandHandlerResult? Dispatch(PlayerCommandHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (IControlFlowCommandHandler handler in handlers)
        {
            if (handler.CanHandle(context.CommandDefinition))
            {
                return handler.Handle(context);
            }
        }

        return null;
    }
}
