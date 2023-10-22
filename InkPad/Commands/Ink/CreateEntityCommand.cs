namespace InkPad.Commands.Ink;

using InkPad.Elements;
using InkPad.Pads;
using InkPad.Tasks.Ink;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CreateEntityCommand : EditCommand
{
    public CreateEntityTask? EntityTask { get; private set; }
    public Entity? Entity => EntityTask?.Entity;
    public int? EntityKey => EntityTask?.EntityKey;

    public CreateEntityCommand(Pad pad) : base(pad) { }

    // maybe tasks need to be start/update/complete, where eg merge endpoints is on complete, and MoveElementTask is available for updates.
    // this makes tasks continuous like animation or transform commands.

    public override void Execute()
    {
        base.Execute();
        EntityTask ??= new CreateEntityTask(Pad.PadKind);
        AddTaskAndRun(EntityTask);
    }

    public override void Unexecute()
    {
        base.Unexecute();
    }

    public override void Update(SKPoint point)
    {
    }

    public override void Completed()
    {
    }
}
