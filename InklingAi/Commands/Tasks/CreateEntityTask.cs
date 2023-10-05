namespace InklingAi.Commands.Tasks;

using InklingAi.Elements;
using InklingAi.Pads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CreateEntityTask : EditTask, ICreateTask
{
    public Entity Entity { get; private set; } = Entity.Empty;
    public int EntityKey => Entity.Key;
    public CreateEntityTask(PadKind padKind) : base(padKind)
    {
        // create entity, assign key
    }

    public override void RunTask()
    {
        base.RunTask();
        if (Entity.IsEmpty)
        {
            Entity = new Entity(PadKind);
        }
        else
        {
            Pad.AddElement(Entity);
        }
    }
    public override void UnRunTask()
    {
        base.UnRunTask();
        Pad.RemoveElement(EntityKey);
    }
}
