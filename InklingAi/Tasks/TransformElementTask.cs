namespace InklingAi.Tasks;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InklingAi.Views;
using InkPad.CommandEngine;
using SkiaSharp;

public class TransformElementTask : TaskBase
{
    public SKMatrix NewMatrix { get; private set; }
    public SKMatrix OldMatrix { get; private set; }
    public PaintableImage Target { get; }
    public override bool IsValid => true;
    public TransformElementTask(SKMatrix newMatrix, PaintableImage target)
    {
        NewMatrix = newMatrix;
        Target = target;
    }
    public override void RunTask()
    {
        var selected = Target.SelectedEntity ?? throw new ArgumentNullException(nameof(Target.SelectedEntity));
        OldMatrix = selected.Matrix;
        selected.Matrix = NewMatrix;
    }

    public override void UnRunTask()
    {
        var selected = Target.SelectedEntity ?? throw new ArgumentNullException(nameof(Target.SelectedEntity));
        selected.Matrix = OldMatrix;
    }
    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        return result;
    }
}
