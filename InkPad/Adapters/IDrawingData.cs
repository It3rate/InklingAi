namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InkPad.Adapters;

    public interface IDrawingData
    {
        float BrushSize { get; set; }
        float Feather { get; set; }
        byte Transparency { get; set; }
        uint ColorValue { get; set; }
    }
}
