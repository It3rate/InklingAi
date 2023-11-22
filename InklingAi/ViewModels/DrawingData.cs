namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
	using Avalonia.Media;
	using SkiaSharp;

    public class DrawingData : ViewModelBase, IDrawingData
    {
        public event EventHandler BrushSizeChanged;
        public event EventHandler FeatherChanged;
        public event EventHandler TransparencyChanged;
        public event EventHandler ColorValueChanged;
        private bool _pauseEvents = false;

        public SKPoint CurrentPosition { get; set; }

        public float _brushSize = 20f;
        public float BrushSize
        {
            get => _brushSize;
            set
            {
                SetProperty(ref _brushSize, value);
                if (!_pauseEvents)
                {
                    BrushSizeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public float _feather = 0f;
        public float Feather
        {
            get => _feather;
            set
            {
                SetProperty(ref _feather, value);
                if (!_pauseEvents)
                {
                    FeatherChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private byte _transparency = 255;
		public byte Transparency 
        { 
            get => _transparency;
            set
			{
				SetProperty(ref _transparency, value);
				SelColor = new Color(_transparency, _selColor.R, _selColor.G, _selColor.B);
                if (!_pauseEvents)
                {
                    TransparencyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public uint ColorValue
        {
            get => SelColor.ToUInt32();
            set 
            { 
                SelColor = Color.FromUInt32(value);
                if (!_pauseEvents)
                {
                    ColorValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        Color _selColor = Color.FromUInt32(0xFF0072B2);
		public Color SelColor
        {
            get => _selColor;
            set
            {
                SetProperty(ref _selColor, new Color(_transparency, value.R, value.G, value.B));
                if (!_pauseEvents)
                {
                    ColorValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public void SetValuesNoEvents(IDrawingData data)
        {
            _pauseEvents = true;
            Transparency = data.Transparency;
            SelColor = Color.FromUInt32(data.ColorValue);
            BrushSize = data.BrushSize;
            Feather = data.Feather;
            _pauseEvents = false;
        }
        public SKPoint ClonePoint { get; set; } = SKPoint.Empty;

        public Color CurrentGray { get; set; } = Colors.Gray;

		public DrawingData() { }
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
