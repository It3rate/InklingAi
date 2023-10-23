namespace InklingAi.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using InklingAi.Tasks;
using InklingAi.Views;
using InkPad.Adapters;
using InkPad.CommandEngine;
using InkPad.Tasks.Core;
using Nums.CoreConcepts.Time;
using Nums.Primitives;

public class CreateSKPolylineCommand : CommandBase
{
    public CreateSKPolylineTask? CreateTask;

    private SkPolyline _polyline;
    private PaintableImage _target;
    public CreateSKPolylineCommand(SkPolyline polyline, PaintableImage target)
    {
        _polyline = polyline;
        _target = target;
    }

    public override void Execute()
    {
        base.Execute();
        if (CreateTask == null)
        {
            CreateTask = new CreateSKPolylineTask(_polyline, _target);
        }
        AddTaskAndRun(CreateTask);
    }

    public override void Unexecute()
    {
        base.Unexecute();
    }

    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
    {
    }

    public override void Completed()
    {
    }
}
