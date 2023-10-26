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
    private PaintableImage _target;
    public DeleteSelectedElementsCommand(PaintableImage target)
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
    }

    public override void Unexecute()
    {
        base.Unexecute();
    }

    public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
    {
    }
    public override bool TryMergeWith(ICommand newCommand)
    {
        return false;
    }

    public override void Completed()
    {
    }
}