namespace InklingAi.ViewModels
{
    using Avalonia.Controls;
    using Avalonia.Svg.Skia;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DesignTimeInputItem : ProcessedImageData
    {
        public DesignTimeInputItem() : base(
            new BrushSettings() { Name = "Edges", ColorAllowance = ColorAllowance.BlackAndWhite, MaxWidth = 1, FeatherMax = 0, AlphaMin = 255 },
            @"C:\Users\Robin\InklingAI\SD\working\")
        {
        }
    }
}
