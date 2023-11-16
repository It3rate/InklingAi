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
    public SKPoint Anchor { get; private set; }
    public List<ISelectableElement> PreviousSelection { get; } = new();
    public List<SKMatrix> PreviousMatricies { get; } = new();
    public PaintableImage Target { get; }
	public bool IsIncremental { get; set; } = true;
    public override bool IsValid => true;
    private TransformElementTask(PaintableImage target, SKPoint? anchor = null)
    {
        NewMatrix = SKMatrix.Identity;
        Target = target;
        if (anchor != null)
        {
            Anchor = anchor.Value;
        }
        else
        {
		    var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
            var bnds = selected.Bounds;
            Anchor = new SKPoint(bnds.MidX, bnds.MidY);
        }
	}
	public TransformElementTask(PaintableImage target, float dx, float dy, float rotate, float scaleX, float scaleY, SKPoint? anchor = null) : this(target, anchor)
	{
		//Target = target;
		//var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
		//NewMatrix = SKMatrix.Identity;
		//var bnds = selected.Bounds;
  //      Anchor = anchor ?? new SKPoint(bnds.MidX, bnds.MidY); 

        if (rotate != 0)
		{
            NewMatrix = NewMatrix.PostConcat(SKMatrix.CreateRotation(rotate * (MathF.PI / 180f), Anchor.X, Anchor.Y));
		}
		if (scaleX != 1 || scaleY != 1)
		{
            NewMatrix = NewMatrix.PostConcat(SKMatrix.CreateTranslation(-Anchor.X, -Anchor.Y));
            NewMatrix = NewMatrix.PostConcat(SKMatrix.CreateScale(scaleX, scaleY));
            NewMatrix = NewMatrix.PostConcat(SKMatrix.CreateTranslation(Anchor.X, Anchor.Y));
		}
		if (dx != 0 || dy != 0)
		{
            NewMatrix = NewMatrix.PostConcat(SKMatrix.CreateTranslation(dx, dy));
		}
	}

	public static TransformElementTask CreateTranslationTask(PaintableImage target, float dx, float dy, SKPoint? anchor = null)
	{
		var result = new TransformElementTask(target, dx, dy, 0, 1f, 1f, anchor);
		return result;
	}
	public static TransformElementTask CreateRotationTask(PaintableImage target, float degrees, SKPoint? anchor = null)
	{
		//var result = new TransformElementTask(target);
		//var selected = target.Selection ?? throw new ArgumentNullException(nameof(target.Selection));
		//var bnds = selected.Bounds;
		//result.NewMatrix = SKMatrix.CreateRotationDegrees(degrees, bnds.MidX, bnds.MidY);
        var result = new TransformElementTask(target, 0, 0, degrees, 1f, 1f, anchor);
        return result;
	}
	public static TransformElementTask CreateScaleTask(PaintableImage target, float scaleX, float scaleY, SKPoint? anchor = null)
    {
        var result = new TransformElementTask(target, 0, 0, 0, scaleX, scaleY, anchor);

  //      var result = new TransformElementTask(target);
		//var selected = target.Selection ?? throw new ArgumentNullException(nameof(target.Selection));
		//var bnds = selected.Bounds;
		//result.NewMatrix = SKMatrix.Identity;
		//result.NewMatrix = result.NewMatrix.PostConcat(SKMatrix.CreateTranslation(-bnds.MidX, -bnds.MidY));
		//result.NewMatrix = result.NewMatrix.PostConcat(SKMatrix.CreateScale(scaleX, scaleY));
		//result.NewMatrix = result.NewMatrix.PostConcat(SKMatrix.CreateTranslation(bnds.MidX, bnds.MidY));
		return result;
	}

    private bool _wasRun = false;
	public override void RunTask()
    {
        var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
        if (IsIncremental && PreviousMatricies.Count > 0)
        {
            for (int i = 0; i < selected.Count; i++)
            { 
                var m = PreviousMatricies[i];
                selected[i].Matrix = m.PostConcat(NewMatrix);
            }
        }
        else
        {
            PreviousSelection.Clear();
            PreviousMatricies.Clear();
            foreach (var element in selected) // todo: Do transforms in selection class and return old values
            {
                    PreviousSelection.Add(element);
                    PreviousMatricies.Add(element.Matrix);
                    element.Matrix = element.Matrix.PostConcat(NewMatrix); 
            }
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

    private int[] PreviousIds() => PreviousSelection.Select(item => item.Id).ToArray();
    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        if (newTask is TransformElementTask task && 
            task.Target == Target && 
            task.IsIncremental == IsIncremental && 
            task.Target.Selection.GetIds().SequenceEqual(PreviousIds()))
        {
			if (IsIncremental)
			{
				NewMatrix.PostConcat(task.NewMatrix);
				var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
				foreach (var element in selected)
				{
					element.Matrix = task.NewMatrix;
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
