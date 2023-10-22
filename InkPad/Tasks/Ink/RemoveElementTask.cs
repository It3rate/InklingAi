namespace InkPad.Tasks.Ink;

using InkPad.CommandEngine;
using InkPad.Elements;
using InkPad.Pads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RemoveElementTask : EditTask, IChangeTask
{
    public List<int> ElementKeys { get; }
    private IElement? _removedElement;
    private IEnumerable<IConstraint> _removedConstraints = new List<IConstraint>();

    public RemoveElementTask(PadKind padKind, IElement element) : base(padKind)
    {
        ElementKeys = element.AllKeys;
    }

    public override void RunTask()
    {
        base.RunTask();
        foreach (var key in ElementKeys)
        {
            _removedElement = Pad.ElementAt(key);
            Pad.RemoveElement(key);
            _removedConstraints = Pad.GetRelatedConstraints(_removedElement);
            Pad.RemoveConstraints(_removedConstraints);
        }
    }

    public override void UnRunTask()
    {
        if (_removedElement != null)
        {
            Pad.AddElement(_removedElement);
        }
        Pad.AddConstraints(_removedConstraints);
    }
}
