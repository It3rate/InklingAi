namespace InklingAi.Agents
{
    using Avalonia.Input;
    using InklingAi.Pads;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IAgent
    {
        //RenderStatus RenderStatus { get; }
        //Position this[IPoint point] { get; set; }

        Pad PadFor(PadKind padKind);

        //void UpdatePointRef(IPoint from, IPoint to);
        void Clear();

        //bool MouseDown(MouseEventArgs e);
        //bool MouseMove(MouseEventArgs e);
        //bool MouseUp(MouseEventArgs e);
        //bool KeyDown(KeyEventArgs e);
        //bool KeyUp(KeyEventArgs e);
    }
}
