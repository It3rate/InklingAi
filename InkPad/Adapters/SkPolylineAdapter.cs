using Nums.Composites;
using Nums.CoreConcepts.CoreTraits;
using Nums.Primitives;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkPad.Adapters;

public class SkPolylineAdapter : SkiaAdapter
{
    private bool _brushDirty = true;
    private bool _pathDirty = true;
    private int _lastRenderPointCount = 0;
    public SKPath? _lastPath;

    private float _brushSize = 20f;
    public float BrushSize
    {
        get => _brushSize; 
        set
        {
            if (_brushSize != value)
            {
                _brushSize = value; 
                _brushDirty = true; 
                _pathDirty = true;
            }
        }
    }


    public uint _colorValue { get; set; }
    public uint ColorValue { get => _colorValue; 
        set
        {
            if (_colorValue != value)
            {
				_colorValue = value;
                _brushDirty = true;
            }
        }
    }
    public float Feather { get; set; } = 0f;

    SKPaint _paint;
    public SKPaint Paint
    {
        get
        {
            if (_brushDirty)
            {
                _paint?.Dispose();
                _paint = new SKPaint()
                {
                    Style = SKPaintStyle.Stroke,
                    Color = new SKColor(ColorValue),
                    StrokeWidth = BrushSize,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round,
                    StrokeMiter = 3.0f,
                    IsAntialias = true,
                };
                _brushDirty = false;
            }
            return _paint;
        }
    }
    public SkPolylineAdapter(int width, int height, float? brushSize = null, uint? currentColor = null, float? feather = null)
    {
        var trait = PositionTrait.Instance;
        var xDomain = new Domain(trait, "xAxis", Focal.UnitFocal, new Focal(0, width));
        var yDomain = new Domain(trait, "yAxis", Focal.UnitFocal, new Focal(0, height));
        var polyNumberX = new PolyNumber(xDomain, new PolyFocal());
        var polyNumberY = new PolyNumber(yDomain, new PolyFocal());
        _polyset = new PolyNumberSet(polyNumberX, polyNumberY);

        if (brushSize != null)
        {
            BrushSize = brushSize.Value;
        }
        if (currentColor != null)
        {
            ColorValue = currentColor.Value;
        }
        if (feather != null)
        {
            Feather = feather.Value;
        }
    }


    public void AddPoint(double x, double y)
    {
        _polyset.AddValue(x, y);
        _pathDirty = true;
    }
    public void RemovePoint()
    {
        _polyset.RemoveLastValue();
    }

    public override void Reset()
    {
        base.Reset();
        _brushDirty = true;
        _pathDirty = true;
    }

    public SKPath PathOutline
    {
        get
        {
            if (_pathDirty || _lastPath == null || Count != _lastRenderPointCount)
            {
                _lastRenderPointCount = Count;
                var path = ToOptimizedCurve(false);
                _lastPath = Paint.GetFillPath(path);
                _pathDirty = false;
            }
            return _lastPath;
        }
    }
}
