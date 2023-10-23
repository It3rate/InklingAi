using Nums.CoreConcepts.Time;
using Nums.Primitives;

namespace InkPad.CommandEngine
{
    using InkPad.Agents;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class TaskBase : ITask
    {
        private static int _idCounter = 1;
        public int Id { get; }
        public ICommand? Command { get; }
        public TaskTimer Timer { get; }
        public abstract bool IsValid { get; }

        protected TaskBase()
        {
	        Id = _idCounter++;
            Timer = new TaskTimer(new MillisecondNumber(0));
        }
        public virtual void Initialize() { }

        public virtual void RunTask() { }
        public virtual void UnRunTask() { }

    }
}
