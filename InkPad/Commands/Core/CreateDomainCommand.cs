using InkPad.CommandEngine;
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
    using InkPad.Tasks.Core;

    public class CreateDomainCommand : CommandBase
    {
        public Domain Domain => DomainTask?.Domain;

        // combine tasks that are in core for common commands that require multiple tasks
        private CreateFocalTask BasisTask;
        private CreateFocalTask MinMaxTask;
        private CreateDomainTask DomainTask;

        public Trait Trait { get; }

        public long BasisStart { get; }
        public long BasisEnd { get; }
        public long MinMaxStart { get; }
        public long MinMaxEnd { get; }

        public IFocal BasisFocal { get; private set; }
        public IFocal MinMaxFocal { get; private set; }

        public CreateDomainCommand(Trait trait, long basisStart, long basisEnd, long minMaxStart, long minMaxEnd)
        {
            Trait = trait;
            BasisStart = basisStart;
            BasisEnd = basisEnd;
            MinMaxStart = minMaxStart;
            MinMaxEnd = minMaxEnd;
        }
        public CreateDomainCommand(Trait trait, IFocal basisFocal, IFocal minMax)
        {
            Trait = trait;
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

            DomainTask = new CreateDomainTask(Trait, BasisFocal, MinMaxFocal);
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

