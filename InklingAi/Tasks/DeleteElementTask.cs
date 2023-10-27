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
    public List<ISelectableElement> ElementsToDelete { get; } = new();
    public List<int> DeletionIndexes { get; } = new ();
    public PaintableImage Target { get; }
    public override bool IsValid => true;
    public DeleteElementTask(PaintableImage target, params ISelectableElement[] elementsToDelete)
    {
        Target = target;
        ElementsToDelete.AddRange(elementsToDelete);
    }
    public override void RunTask()
    {
        foreach (var element in ElementsToDelete)
        {
            DeletionIndexes.Add(Target.RemoveElement(element));
        }
    }

    public override void UnRunTask()
    {
        var revElements = ElementsToDelete.ToArray();
        revElements.Reverse();
        var revIndexes = DeletionIndexes.ToArray();
        revIndexes.Reverse();
        for (var i = 0; i < revElements.Length; i++)
        {
            Target.AddElementAt(revIndexes[i], revElements[i]);
        }
    }
    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        return result;
    }
}
