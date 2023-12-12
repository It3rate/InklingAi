namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Avalonia.Media;
    using InkPad.Adapters;
    using SkiaSharp;

    public class BrushState : ViewModelBase, IBrushState
    {

        private BrushData _brushData { get; } = new BrushData();

        public event EventHandler BrushSizeChanged;
        public event EventHandler FeatherChanged;
        public event EventHandler TransparencyChanged;
        public event EventHandler ColorValueChanged;
        private bool _pauseEvents = false;

        public SKPoint CurrentPosition { get; set; }

        public BrushState() { }
        public BrushData GetBrushDataClone() => _brushData.Clone();
        public void SetWithBrushData(BrushData brushData)
        {
            BrushSize = brushData.BrushSize;
            Feather = brushData.Feather;
            Transparency = brushData.Transparency;
            ColorValue = brushData.ColorValue;
        }

        public float BrushSize
        {
            get => _brushData.BrushSize;
            set
            {
                SetProperty(ref _brushData.BrushSize, value);
                if (!_pauseEvents)
                {
                    BrushSizeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public float Feather
        {
            get => _brushData.Feather;
            set
            {
                SetProperty(ref _brushData.Feather, value);
                if (!_pauseEvents)
                {
                    FeatherChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public byte Transparency
        {
            get => _brushData.Transparency;
            set
            {
                SetProperty(ref _brushData.Transparency, value);
                SelColor = new Color(_brushData.Transparency, _selColor.R, _selColor.G, _selColor.B); // set both so UI reflects changes
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

        public Color _selColor = Color.FromUInt32(0xFF0072B2);
        public Color SelColor
        {
            get => _selColor;
            set
            {
                SetProperty(ref _selColor, new Color(_brushData.Transparency, value.R, value.G, value.B));
                _brushData.ColorValue = _selColor.ToUInt32();
                if (!_pauseEvents)
                {
                    ColorValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public void SetValuesNoEvents(IBrushState data)
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
    }

    public enum BrushPanelMode
    {
        None,
        Size,
        Feather,
        Transparency,
        ColorPicker,
        ColorDropper,
        CloneAnchor,
    }

    public enum DrawPanelMode
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
