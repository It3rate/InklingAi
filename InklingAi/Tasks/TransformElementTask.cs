namespace InklingAi.Tasks;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using InklingAi.Views;
using InkPad.Adapters;
using InkPad.CommandEngine;
using SkiaSharp;

public class TransformElementTask : TaskBase
{
    public SKMatrix NewMatrix { get; private set; }
    public List<ISelectableEntity> PreviousSelection { get; } = new();
    public List<SKMatrix> PreviousMatricies { get; } = new();
    public PaintableImage Target { get; }
    public override bool IsValid => true;
    public TransformElementTask(SKMatrix newMatrix, PaintableImage target)
    {
        NewMatrix = newMatrix;
        Target = target;
    }
    public override void RunTask()
    {
        var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));
        PreviousSelection.Clear();
        PreviousMatricies.Clear();
        foreach (var element in selected)
        {
            PreviousSelection.Add(element);
            PreviousMatricies.Add(element.Matrix);
            element.Matrix = NewMatrix; // todo: use Selection directly
        }
    }

    public override void UnRunTask()
    {
        var selected = Target.Selection ?? throw new ArgumentNullException(nameof(Target.Selection));

        for (int i = 0; i < PreviousSelection.Count; i++)
        {
            PreviousSelection[i].Matrix = PreviousMatricies[i];
        }
    }
    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        return result;
    }
}
