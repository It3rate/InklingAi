namespace InklingAi.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InklingAi.Views;
using InkPad.Adapters;
using InkPad.CommandEngine;
using Nums.Primitives;

public class CreateSKPolylineTask : TaskBase, ICreateTask
{
    public SkPolyline CreatedPolyline { get; }
    public PaintableImage Target { get; } // will be selected element etc

    public override bool IsValid => true;
    public CreateSKPolylineTask(SkPolyline polyline, PaintableImage target)
    {
        CreatedPolyline = polyline;
        Target = target;
    }
    public override void RunTask()
    {
        Target.AddPath(CreatedPolyline);
    }

    public override void UnRunTask()
    {
        Target.RemoveLastPath();
    }
}
