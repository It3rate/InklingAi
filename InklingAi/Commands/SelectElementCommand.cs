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

public class SelectElementCommand : CommandBase
{
    public List<ISelectableElement> Elements { get; private set; } = new();
    public SelectElementTask? SelectTask;
    private PaintableImage _target;
    public SelectElementCommand(PaintableImage target, params ISelectableElement?[]? elements)
    {
        if (elements != null)
        {
            ISelectableElement[] filtered = (ISelectableElement[])(elements.Where(item => item != null).ToArray());
            Elements.AddRange(filtered);
        }
        _target = target;
    }
    public override bool CanUndo => false;

    public override void Execute()
    {
        base.Execute();
        if (SelectTask == null)
        {
            SelectTask = new SelectElementTask(_target, Elements.ToArray());
        }
        AddTaskAndRun(SelectTask);
    }

    public override void Unexecute()
    {
        base.Unexecute();
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