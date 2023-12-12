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
using SkiaSharp;

public class TransformElementCommand : CommandBase
{
    public TransformElementTask TransformTask;
    public SKMatrix NewMatrix => TransformTask.NewMatrix;
    private VectorImage _target;

    private TransformElementCommand(VectorImage target)
    {
        _target = target;
	}
	public static TransformElementCommand CreateTranslationCommand(VectorImage target, float dx, float dy, SKPoint? anchor = null, bool isIncremental = false)
	{
		var result = new TransformElementCommand(target);
		result.TransformTask = TransformElementTask.CreateTranslationTask(target, dx, dy, anchor);
        result.TransformTask.IsIncremental = isIncremental;
		return result;
	}
	public static TransformElementCommand CreateRotationCommand(VectorImage target, float degrees, SKPoint? anchor = null, bool isIncremental = false)
	{
		var result = new TransformElementCommand(target);
		result.TransformTask = TransformElementTask.CreateRotationTask(target, degrees, anchor);
		result.TransformTask.IsIncremental = isIncremental;
		return result;
	}
	public static TransformElementCommand CreateScaleCommand(VectorImage target, float scaleX, float scaleY, SKPoint? anchor = null, bool isIncremental = false)
	{
		var result = new TransformElementCommand(target);
		result.TransformTask = TransformElementTask.CreateScaleTask(target, scaleX, scaleY, anchor);
		result.TransformTask.IsIncremental = isIncremental;
		return result;
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
        if (!IsSealed && newCommand is TransformElementCommand newCmd)
        {
            result = TransformTask.MergeDataFrom(newCmd.TransformTask);
        }
        return result;
    }

    public override void Completed()
    {
    }
}
