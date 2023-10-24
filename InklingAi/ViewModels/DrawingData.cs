namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
	using Avalonia.Media;
	using SkiaSharp;

    public class DrawingData : ViewModelBase
	{
        public static DrawingData Instance { get; } = new DrawingData();

        public SKPoint CurrentPosition { get; set; }

        public BrushMode BrushMode { get; set; } = BrushMode.None;
        public DrawMode DrawMode { get; set; } = DrawMode.Arrow;
        public float BrushSize { get; set; } = 20f;
        public float Feather { get; set; } = 0f;
        public SKPoint ClonePoint { get; set; } = SKPoint.Empty;

		private byte _transparency = 255;
		public byte Transparency 
        { 
            get => _transparency;
            set
			{
				SetProperty(ref _transparency, value);
				SelColor = new Color(_transparency, _selColor.R, _selColor.G, _selColor.B);
            }
        }

		Color _selColor = Color.FromUInt32(0xFF0072B2);
		public Color SelColor {
            get => _selColor;
            set
			{
				SetProperty(ref _selColor, new Color(_transparency, value.R, value.G, value.B));
                //if (_transparency != _selColor.A)
                //{
                //    Transparency = _selColor.A;
                //}
            }
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
        Pen,
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
