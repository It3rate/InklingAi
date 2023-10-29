namespace InklingAi.Tasks;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Styling;
using HarfBuzzSharp;
using InklingAi.Views;
using InkPad.Adapters;
using InkPad.CommandEngine;
using SkiaSharp;

public class TransformElementTask : TaskBase
{
    public SKMatrix NewMatrix { get; private set; }
    public List<ISelectableElement> PreviousSelection { get; } = new();
    public List<SKMatrix> PreviousMatricies { get; } = new();
    public PaintableImage Target { get; }
	public bool IsIncremental { get; set; } = true;
    public override bool IsValid => true;
    private TransformElementTask(PaintableImage target)
	{
		Target = target;
	}
	public TransformElementTask(PaintableImage target, float dx, float dy, float rotate, float scale)
	{
		Target = target;
		var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
		NewMatrix = SKMatrix.Identity;
		var bnds = selected.Bounds;
		if (rotate != 0)
		{
			NewMatrix.PostConcat(SKMatrix.CreateRotation(rotate * (MathF.PI / 180f), bnds.MidX, bnds.MidY));
		}
		if (scale != 1)
		{
			NewMatrix.PostConcat(SKMatrix.CreateTranslation(-bnds.MidX, -bnds.MidY));
			NewMatrix.PostConcat(SKMatrix.CreateScale(scale, scale));
			NewMatrix.PostConcat(SKMatrix.CreateTranslation(bnds.MidX, bnds.MidY));
		}
		if (dx != 0 || dy != 0)
		{
			NewMatrix.PostConcat(SKMatrix.CreateTranslation(dx, dy));
		}
	}

	public static TransformElementTask CreateTranslationTask(PaintableImage target, float dx, float dy)
	{
		var result = new TransformElementTask(target);
		result.NewMatrix = SKMatrix.CreateTranslation(dx, dy);
		return result;
	}
	public static TransformElementTask CreateRotationTask(PaintableImage target, float rotate)
	{
		var result = new TransformElementTask(target);
		var selected = target.Selection ?? throw new ArgumentNullException(nameof(target.Selection));
		var bnds = selected.Bounds;
		result.NewMatrix = SKMatrix.CreateRotation(rotate * (MathF.PI / 180f), bnds.MidX, bnds.MidY);
		return result;
	}
	public static TransformElementTask CreateScaleTask(PaintableImage target, float scaleX, float scaleY)
	{
		var result = new TransformElementTask(target);
		var selected = target.Selection ?? throw new ArgumentNullException(nameof(target.Selection));
		var bnds = selected.Bounds;
		result.NewMatrix = SKMatrix.Identity;
		result.NewMatrix = result.NewMatrix.PostConcat(SKMatrix.CreateTranslation(-bnds.MidX, -bnds.MidY));
		result.NewMatrix = result.NewMatrix.PostConcat(SKMatrix.CreateScale(scaleX, scaleY));
		result.NewMatrix = result.NewMatrix.PostConcat(SKMatrix.CreateTranslation(bnds.MidX, bnds.MidY));
		return result;
	}

	public override void RunTask()
    {
        var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
        PreviousSelection.Clear();
        PreviousMatricies.Clear();
        foreach (var element in selected) // todo: Do transforms in selection class and return old values
        {
            PreviousSelection.Add(element);
            PreviousMatricies.Add(element.Matrix);
            element.Matrix = element.Matrix.PostConcat(NewMatrix); 
        }
        Target.InvalidateVisual();
    }

    public override void UnRunTask()
    {
        for (int i = 0; i < PreviousSelection.Count; i++)
        {
            PreviousSelection[i].Matrix = PreviousMatricies[i];
        }
        Target.InvalidateVisual();
    }
    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        if (newTask is TransformElementTask task && task.IsIncremental == IsIncremental)
        {
			if (IsIncremental)
			{
				NewMatrix.PostConcat(task.NewMatrix);
				var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
				foreach (var element in selected)
				{
					element.Matrix = element.Matrix.PostConcat(task.NewMatrix);
				}
			}
			else
			{
				NewMatrix = task.NewMatrix;
				for (int i = 0; i < PreviousSelection.Count; i++)
				{
					PreviousSelection[i].Matrix = PreviousMatricies[i].PostConcat(NewMatrix);
				}
			}
            result = true;
        }
        return result;
    }
}
