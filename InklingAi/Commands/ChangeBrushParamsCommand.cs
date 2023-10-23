using InklingAi.Tasks;
using InklingAi.Views;
using InkPad.Adapters;
using InkPad.CommandEngine;
using Nums.CoreConcepts.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InklingAi.Commands;

public class ChangeBrushParamsCommand : CommandBase
{
	public ChangeBrushParamsTask ChangeTask;
	private BrushToolbar _target;

	public ChangeBrushParamsCommand(ChangeBrushParamsTask changeTask, BrushToolbar target)
	{
		ChangeTask = changeTask;
		_target = target;
	}

	public override void Execute()
	{
		base.Execute();
		_target.GenCommandsOnChange = false;
		AddTaskAndRun(ChangeTask);
		_target.GenCommandsOnChange = true;
	}

	public override void Unexecute()
	{
		_target.GenCommandsOnChange = false;
		base.Unexecute();
		_target.GenCommandsOnChange = true;
	}

	public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
	{
	}

	public override void Completed()
	{
	}
}