namespace InklingAi.Commands.EditCommands;

using InklingAi.Commands.Tasks;
using InklingAi.Elements;
using InklingAi.Pads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RemoveElementCommand : EditCommand
{
    public RemoveElementTask RemoveElementTask { get; set; }
    public IElement Element;

    public RemoveElementCommand(Pad pad, IElement element) : base(pad)
    {
        Element = element;
    }

    public override void Execute()
    {
        base.Execute();
        if (RemoveElementTask == null)
        {
            RemoveElementTask = new RemoveElementTask(Pad.PadKind, Element);
        }
        AddTaskAndRun(RemoveElementTask);
    }
}
