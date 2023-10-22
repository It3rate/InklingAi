using InkPad.Renderer;
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

    public bool IsPointInside(SKPoint point)
    {
        return false;
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

    public SKPaint Paint
    {
        get
        {
            if (_brushDirty)
            {
                _paint?.Dispose();
                _paint = SKRenderUtils.CreatePen(ColorValue, BrushSize);
                _brushDirty = false;
            }
            return _paint;
        }
    }
    public SKPath PathOutline
    {
        get
        {
            if (_pathDirty || _path == null)
            {
                var path = ToOptimizedCurve(false);
                _path = Paint.GetFillPath(path);
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

	#region private fields
	private SKPath? _path;
    private SKPaint _paint = new();

    private float _brushSize = 20f;
	private uint _colorValue;

	private bool _brushDirty = true;
    private bool _pathDirty = true;
	#endregion

}
