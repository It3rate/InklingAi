using System;
using System.Collections.Generic;
using InkPad.Agents;
using Nums.CoreConcepts.Time;
using Nums.Primitives;

namespace InkPad.CommandEngine.BrainEngine
{
    public interface IBrainCommandStack : ICommandStack
    {
        IAgent Agent { get; }
        Brain Brain { get; }
        Workspace Workspace { get; }

    }

    public class BrainCommandStack : CommandStack, IBrainCommandStack
    {
        public IAgent Agent { get; }
        public Brain Brain => Agent.Brain;
        public Workspace Workspace => Agent.Workspace;

        private int _stackIndex = 0;
        private readonly List<IBrainCommand> _stack = new List<IBrainCommand>(4096);

        private readonly List<IBrainCommand> _toAdd = new List<IBrainCommand>();
        private readonly List<IBrainCommand> _toAddAfterDelay = new List<IBrainCommand>();
        private readonly List<IBrainCommand> _liveCommands = new List<IBrainCommand>();
        private readonly List<IBrainCommand> _toCommit = new List<IBrainCommand>(); // changes
        private readonly List<IBrainCommand> _toDelete = new List<IBrainCommand>(); // commands that natrually end
        private readonly List<IBrainCommand> _toTerminate = new List<IBrainCommand>(); // commands that didn't naturally finish

        public BrainCommandStack(IAgent agent)
        {
            Agent = agent;
        }
    }
}