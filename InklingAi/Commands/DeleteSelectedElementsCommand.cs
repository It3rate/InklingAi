namespace InklingAi.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InklingAi.Tasks;
using InklingAi.Views;
using InkPad.Adapters;
using InkPad.CommandEngine;
using Nums.CoreConcepts.Time;

public class DeleteSelectedElementsCommand : CommandBase
{ 
    public DeleteElementTask? DeleteTask;
    private VectorImage _target;
    public DeleteSelectedElementsCommand(VectorImage target)
    {
        _target = target;
    }

    public override void Execute()
    {
        base.Execute();
        if (_target.Selection == null) { throw new NullReferenceException("Delete task must contain element."); }

        if (DeleteTask == null)
        {
            DeleteTask = new DeleteElementTask(_target, _target.Selection.ToArray());
        }
        AddTaskAndRun(DeleteTask);
        _target.Selection.Clear();
    }

    public override void Unexecute()
    {
        base.Unexecute();
        if (DeleteTask != null)
        {
            _target.Selection.SetWith(DeleteTask.ElementsToDelete);
        }
    }

    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
    {
    }
    public override bool TryMergeWith(IStackCommand newCommand)
    {
        return false;
    }

    public override void Completed()
    {
    }
}