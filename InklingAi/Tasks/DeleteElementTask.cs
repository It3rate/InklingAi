namespace InklingAi.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarfBuzzSharp;
using InklingAi.Views;
using InkPad.Adapters;
using InkPad.CommandEngine;

public class DeleteElementTask : TaskBase
{
    public ISelectableEntity ElementToDelete { get; private set; }
    public int DeletionIndex { get; private set; } = -1;
    public PaintableImage Target { get; }
    public override bool IsValid => true;
    public DeleteElementTask(PaintableImage target)
    {
        ElementToDelete = target.SelectedEntity ?? throw new ArgumentNullException(nameof(target.SelectedEntity), "ElementToDelete cannot be null.");
        Target = target;
    }
    public override void RunTask()
    {
        Target.RemoveLastElement();
    }

    public override void UnRunTask()
    {
        if (DeletionIndex > -1)
        {
            Target.AddElementAt(DeletionIndex, ElementToDelete);
        }
    }
    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        return result;
    }
}
