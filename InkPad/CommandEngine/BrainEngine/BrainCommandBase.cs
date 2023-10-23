namespace InkPad.CommandEngine.BrainEngine;

using InkPad.Agents;
using Nums.Primitives;

public abstract class BrainCommandBase : CommandBase, IBrainCommand
{
    public IAgent Agent { get; set; } // Agent is set by the command stack.
    public Brain Brain => Agent.Brain;
    public Workspace Workspace => Agent.Workspace;
}
