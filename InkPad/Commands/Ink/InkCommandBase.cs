namespace InkPad.Commands.Ink
{
    using InkPad.CommandEngine;
    using InkPad.Elements;
    using InkPad.Pads;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class InkCommandBase : BrainCommandBase
    {
        public int CommandKey { get; }
        public Pad Pad { get; }
        public PadKind PadKind => Pad.PadKind;

        public IElement ElementAt(int key) => Pad.ElementAt(key);

        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public TimeSpan Duration { get; }

        //public event EventHandler OnExecute;
        //public event EventHandler OnUpdate;
        //public event EventHandler OnUnexecute;
        //public event EventHandler OnCompleted;

        public InkCommandBase(Pad pad)
        {
            Pad = pad;
        }

        public virtual void Update(SKPoint point)
        {
        }

    }

}
