namespace InklingAi.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InklingAi.Views;
using InkPad.Adapters;
using InkPad.CommandEngine;

public class SelectElementTask : TaskBase
{
    public List<ISelectableEntity> NewSelection { get; private set; } = new();
    public List<ISelectableEntity> PreviousSelection { get; private set; } = new();
    public PaintableImage Target { get; }
    public override bool IsValid => true;
    public SelectElementTask(PaintableImage target, params ISelectableEntity[] newSelection)
    {
        NewSelection = newSelection.ToList();
        Target = target;
    }
    public override void RunTask()
    {
        PreviousSelection = Target.Selection.ToList();
        Target.Selection.SetWith(NewSelection);
    }

    public override void UnRunTask()
    {
        Target.Selection.SetWith(PreviousSelection);
    }
    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        return result;
    }
}
