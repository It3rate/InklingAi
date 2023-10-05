using InkPad.CommandEngine;
using Nums.Primitives;
using Nums.Utils;

namespace InkPad.CoreTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ICreateNumberTask : ICreateTask
    {
	    Domain Domain { get; }
	    Number Number { get; }
    }

    public class CreateNumberByFocalIdTask : TaskBase, ICreateNumberTask
    {
	    public Domain Domain { get; }
	    public Number Number { get; private set; }
	    public IFocal Focal { get; }

	    public override bool IsValid => true;

	    public CreateNumberByFocalIdTask(Domain domain, IFocal focal) : base()
	    {
		    Domain = domain;
		    Focal = focal;
	    }
	    public override void RunTask()
	    {
		    if (Number == null)
		    {
			    Number = Domain.CreateNumber(Focal, false);
		    }
		    Domain.AddNumber(Number);
	    }

	    public override void UnRunTask()
	    {
		    Domain.RemoveNumber(Number);
	    }
    }
    public class CreateNumberByRangeTask : TaskBase, ICreateNumberTask
    {
	    public Domain Domain { get; }
	    public Number Number { get; private set; }

	    public NumRange NumRange { get; }

	    public override bool IsValid => true;

	    public CreateNumberByRangeTask(Domain domain, NumRange range) : base()
	    {
		    Domain = domain;
		    NumRange = range;
	    }
	    public override void RunTask()
	    {
		    if (Number == null)
		    {
			    Number = Domain.CreateNumber(NumRange, false);
		    }
		    Domain.AddNumber(Number);
	    }

	    public override void UnRunTask()
	    {
		    Domain.RemoveNumber(Number);
        }
    }
    public class CreateNumberByPositionsTask : TaskBase, ICreateNumberTask
    {
	    public Domain Domain { get; }
	    public Number Number { get; private set; }

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
		    if (Number == null)
		    {
			    Number = Domain.CreateNumber(StartPosition, EndPosition, false);
		    }
		    Domain.AddNumber(Number);
	    }

	    public override void UnRunTask()
	    {
		    Domain.RemoveNumber(Number);
	    }
    }
}
