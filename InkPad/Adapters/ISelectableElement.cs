namespace InkPad.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InklingAi.ViewModels;
    using SkiaSharp;

    public interface ISelectableElement : IDrawingData
    {
        public static int EntityCounter = 1;
        int Id { get; }
        bool IsPointInside(SKPoint point);
        SKRect Bounds { get; }
        SKMatrix Matrix { get; set; }
        SKPoint TransformAnchor { get; set; }
        public SKPaint Paint { get; }
        public SKPath? Path { get; }
        public SKPath PathOutline { get; }
    }
}
