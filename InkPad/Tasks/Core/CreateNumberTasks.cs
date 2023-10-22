using InkPad.CommandEngine;
using Nums.Primitives;
using Nums.Utils;

namespace InkPad.Tasks.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ICreateNumberTask : ICreateTask
    {
        Domain Domain { get; }
        Number? Number { get; }
    }

    public class CreateNumberByFocalIdTask : TaskBase, ICreateNumberTask
    {
        public Domain Domain { get; }
        public Number? Number { get; private set; }
        public IFocal Focal { get; }

        public override bool IsValid => true;

        public CreateNumberByFocalIdTask(Domain domain, IFocal focal) : base()
        {
            Domain = domain;
            Focal = focal;
        }
        public override void RunTask()
        {
            Number ??= Domain.CreateNumber(Focal);
        }

        public override void UnRunTask()
        {
        }
    }
    public class CreateNumberByRangeTask : TaskBase, ICreateNumberTask
    {
        public Domain Domain { get; }
        public Number? Number { get; private set; }

        public Seg NumRange { get; }

        public override bool IsValid => true;

        public CreateNumberByRangeTask(Domain domain, Seg range) : base()
        {
            Domain = domain;
            NumRange = range;
        }
        public override void RunTask()
        {
            Number ??= Domain.CreateNumber(NumRange);
        }

        public override void UnRunTask()
        {
        }
    }
    public class CreateNumberByPositionsTask : TaskBase, ICreateNumberTask
    {
        public Domain Domain { get; }
        public Number? Number { get; private set; }

        public long StartPosition { get; set; }
        public long EndPosition { get; set; }

        public override bool IsValid => true;

        public CreateNumberByPositionsTask(Domain domain, long startPosition, long endPosition) : base()
        {
            Domain = domain;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
        public override void RunTask()
        {
            Number ??= Domain.CreateNumber(StartPosition, EndPosition);
        }

        public override void UnRunTask()
        {
        }
    }
}
