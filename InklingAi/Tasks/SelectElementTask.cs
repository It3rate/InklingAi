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
    public ISelectableEntity NewSelection { get; private set; }
    public ISelectableEntity? PreviousSelection { get; private set; }
    public PaintableImage Target { get; }
    public override bool IsValid => true;
    public SelectElementTask(ISelectableEntity newSelection, PaintableImage target)
    {
        NewSelection = newSelection;
        Target = target;
    }
    public override void RunTask()
    {
        PreviousSelection = Target.SelectedEntity;
        Target.SelectedEntity = NewSelection;
    }

    public override void UnRunTask()
    {
        Target.SelectedEntity = PreviousSelection;
    }
    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        return result;
    }
}
