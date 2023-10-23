using InkPad.Commands;
using Nums.CoreConcepts.Time;
using Nums.Primitives;

namespace InkPad.Commands.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InkPad.CommandEngine.BrainEnging;
    using InkPad.Tasks.Core;

    public class CreateDomainCommand : BrainCommandBase
    {
        public Domain Domain => DomainTask?.TaskDomain;

        // combine tasks that are in core for common commands that require multiple tasks
        private CreateFocalTask BasisTask;
        private CreateFocalTask MinMaxTask;
        private CreateDomainTask DomainTask;

        public Trait TaskTrait { get; }
        public Workspace TaskWorkspace { get; }
        public string Name { get; }

        public long BasisStart { get; }
        public long BasisEnd { get; }
        public long MinMaxStart { get; }
        public long MinMaxEnd { get; }

        public IFocal BasisFocal { get; private set; }
        public IFocal MinMaxFocal { get; private set; }

        public CreateDomainCommand(Workspace workspace, Trait trait, string name, long basisStart, long basisEnd, long minMaxStart, long minMaxEnd)
        {
            TaskWorkspace = workspace;
            TaskTrait = trait;
            Name = name;
            BasisStart = basisStart;
            BasisEnd = basisEnd;
            MinMaxStart = minMaxStart;
            MinMaxEnd = minMaxEnd;
        }
        public CreateDomainCommand(Workspace workspace, Trait trait, string name, IFocal basisFocal, IFocal minMax)
        {
            TaskWorkspace = workspace;
            TaskTrait = trait;
            Name = name;
            BasisFocal = basisFocal;
            MinMaxFocal = minMax;
        }

        public override void Execute()
        {
            base.Execute();
            if (BasisFocal == null)
            {
                BasisTask = new CreateFocalTask(BasisStart, BasisEnd);
                MinMaxTask = new CreateFocalTask(MinMaxStart, MinMaxEnd);
                AddTaskAndRun(BasisTask);
                AddTaskAndRun(MinMaxTask);
                BasisFocal = BasisTask.CreatedFocal;
                MinMaxFocal = MinMaxTask.CreatedFocal;
            }

            DomainTask = new CreateDomainTask(TaskWorkspace, TaskTrait, Name, BasisFocal, MinMaxFocal);
            AddTaskAndRun(DomainTask);
        }

        public override void Unexecute()
        {
            base.Unexecute();
            if (BasisTask != null) // revert to original state in case of redo.
            {
                BasisTask = null;
                MinMaxTask = null;
                BasisFocal = null;
                MinMaxFocal = null;
                // todo: undo should probably roll back all index counters as well.
            }
        }

        public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
        {
        }

        public override void Completed()
        {
        }
    }
}

