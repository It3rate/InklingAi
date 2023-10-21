namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
	using Avalonia.Media;
	using SkiaSharp;

    public class DrawingData
    {
        public static DrawingData Instance { get; } = new DrawingData();
        public BrushMode BrushMode { get; set; } = BrushMode.None;
        public DrawMode DrawMode { get; set; } = DrawMode.Arrow;
        public float BrushSize { get; set; } = 20f;
        public float Feather { get; set; } = 0f;
        public int Transparency { get; set; } = 255;
        public SKPoint ClonePoint { get; set; } = SKPoint.Empty;

        Color _selColor = Colors.Bisque;
		public Color SelColor {
            get => _selColor;
            set => _selColor = value;
        }
        public Color CurrentGray { get; set; } = Colors.Gray;


		private DrawingData() { }
    }

    public enum BrushMode
    {
        None,
        Size,
        Feather,
        Transparency,
        ColorPicker,
        ColorDropper,
        CloneAnchor,
    }

    public enum DrawMode
    {
        None,
        Arrow,
        Paint,
        Erase,
        Line,
        Shape,
        Mask,
        EraseMask,
        Noise,
        FilterBrush,
        Text,

        ZoomPan,
        NodeEdit,
        Shadow,
        Contour,
        Envelope,

    }
}
