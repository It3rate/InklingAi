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
    public override bool IsValid => true;
	public TransformElementTask(PaintableImage target, float dx, float dy)
	{
		Target = target;
		NewMatrix = SKMatrix.CreateTranslation(dx, dy);
	}
	public TransformElementTask(PaintableImage target, float rotate)
	{
		Target = target;
		var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
        var bnds = selected.Bounds;
		NewMatrix = SKMatrix.CreateRotation(rotate * (MathF.PI / 180f), bnds.MidX, bnds.MidY);
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
        var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));

        for (int i = 0; i < PreviousSelection.Count; i++)
        {
            PreviousSelection[i].Matrix = PreviousMatricies[i];
        }
        Target.InvalidateVisual();
    }
    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        if (newTask is TransformElementTask task)
        {
            NewMatrix.PostConcat(task.NewMatrix);
            var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
            foreach (var element in selected)
            {
                element.Matrix = element.Matrix.PostConcat(task.NewMatrix);
            }
            result = true;
        }
        return result;
    }
}
