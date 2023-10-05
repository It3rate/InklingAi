namespace InkPad.Agents;

using InkPad.Pads;
using Nums.CoreConcepts.Time;
using Nums.CoreConcepts;
using Nums.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAgent
{
    Brain Brain { get; }
    Knowledge Knowledge { get; }
    Workspace Workspace { get; }

    Pad PadFor(PadKind padKind);

    void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime);
    void Clear();

    //RenderStatus RenderStatus { get; }
    //Position this[IPoint point] { get; set; }
    //void UpdatePointRef(IPoint from, IPoint to);
    //bool MouseDown(MouseEventArgs e);
    //bool MouseMove(MouseEventArgs e);
    //bool MouseUp(MouseEventArgs e);
    //bool KeyDown(KeyEventArgs e);
    //bool KeyUp(KeyEventArgs e);
}
