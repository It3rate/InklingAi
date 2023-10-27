namespace InklingAi.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InklingAi.Tasks;
using InklingAi.Views;
using InkPad.CommandEngine;
using Nums.CoreConcepts.Time;

public class TransformElementCommand : CommandBase
{
    public TransformElementTask TransformTask;
    private PaintableImage _target;
    public TransformElementCommand(PaintableImage target, float dx, float dy)
    {
        _target = target;
        TransformTask = new TransformElementTask(_target, dx, dy);
    }

	public TransformElementCommand(PaintableImage target, float rotate)
	{
		_target = target;
		TransformTask = new TransformElementTask(_target, rotate);
	}
	public override void Execute()
    {
        base.Execute();
        AddTaskAndRun(TransformTask);
    }

    public override void Unexecute()
    {
        base.Unexecute();
    }

    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
    {
    }
    public override bool TryMergeWith(IStackCommand newCommand)
    {
        var result = false;
        if (newCommand is TransformElementCommand newCmd)
        {
            result = TransformTask.MergeDataFrom(newCmd.TransformTask);
        }
        return result;
    }

    public override void Completed()
    {
    }
}
