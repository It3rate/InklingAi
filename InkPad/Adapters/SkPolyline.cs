using InkPad.Renderer;
using InkPad.Utils;
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

public class SkPolyline : SkiaShapeAdapter, ISelectableElement
{
    #region private fields
    public int Id { get; }
    private SKPath _path = new SKPath();
    private SKPaint _paint = new();

    private float _brushSize = 20f;

    private bool _brushDirty = true;
    #endregion

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
    public byte Transparency
    {
        get => (byte)((_colorValue & 0xFF000000) >> 24);
        set
        {
            _colorValue = (uint)((_colorValue & 0xFFFFFF) + (value << 24));
            _brushDirty = true;
        }
    }
    public float Feather { get; set; } = 0f;

    private uint _colorValue;
    public uint ColorValue 
    { 
        get => _colorValue; 
        set
        {
            if (_colorValue != value)
            {
				_colorValue = value;
                _brushDirty = true;
            }
        }
    }


    public SkPolyline(int width, int height, float? brushSize = null, uint? currentColor = null, float? feather = null) : base(width, height)
    {
        Id = ISelectableElement.EntityCounter++;

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
    public void SetWithBrushData(BrushData brushData)
    {
        BrushSize = brushData.BrushSize;
        Feather = brushData.Feather;
        Transparency = brushData.Transparency;
        ColorValue = brushData.ColorValue;
    }
    protected override PolyNumberSet CreatePolyset(int width, int height)
    {
        var trait = PositionTrait.Instance;
        var xDomain = new Domain(trait, "xAxis", Focal.UnitFocal, new Focal(0, width));
        var yDomain = new Domain(trait, "yAxis", Focal.UnitFocal, new Focal(0, height));
        var polyNumberX = new PolyNumber(xDomain, new PolyFocal());
        var polyNumberY = new PolyNumber(yDomain, new PolyFocal());
        return new PolyNumberSet(polyNumberX, polyNumberY);
    }
    public SKRect Bounds
    {
        get
        {
            var result = Path.Bounds;
            result.Inflate(BrushSize / 2, BrushSize / 2);
            return result;
        }
    }

    public bool IsPointInside(SKPoint point)
    {
        return PathOutline.Contains(point.X, point.Y);
    }
    public void AddPoint(double x, double y)
    {
        Polyset.AddValue(x, y);
        _pathDirty = true;
    }
    public void RemovePoint()
    {
        Polyset.RemoveLastValue();
    }

    public SKPaint Paint
    {
        get
        {
            if (_brushDirty)
            {
                _paint.Dispose();
                _paint = SKRenderUtils.CreatePen(ColorValue, BrushSize, Feather);
                _brushDirty = false;
            }
            return _paint;
        }
    }
    private SKPath _optimizedPath;
    public SKPath Path
    {
        get
        {
            _ = PathOutline;
            return _optimizedPath;
        }
    }
    public SKPath PathOutline
    {
        get
        {
            if (_pathDirty || _path.PointCount == 0)
            {
                _optimizedPath = ToOptimizedCurve(false);
                _path = Paint.GetFillPath(_optimizedPath);
                _pathDirty = false;
            }
            return _path;
        }
    }
    public override void Reset()
    {
        base.Reset();
        _brushDirty = true;
        _pathDirty = true;
    }


}
