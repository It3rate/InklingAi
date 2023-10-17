using Nums.Composites;
using Nums.CoreConcepts.CoreTraits;
using Nums.Primitives;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkPad.Adapters;

public class SkPolylineAdapter : SkiaAdapter
{
	public SkPolylineAdapter(int width, int height)
	{
		var trait = PositionTrait.Instance;
		var xDomain = new Domain(trait, "xAxis", Focal.UnitFocal, new Focal(0, width));
		var yDomain = new Domain(trait, "yAxis", Focal.UnitFocal, new Focal(0, height));
		var polyNumberX = new PolyNumber(xDomain, new PolyFocal());
		var polyNumberY = new PolyNumber(yDomain, new PolyFocal());
		_polyset = new PolyNumberSet(polyNumberX, polyNumberY);
	}

	public void AddPoint(double x, double y)
	{
		_polyset.AddValues(x, y);
	}

    private int _lastRenderPointCount = 0;
    private SKPaint? _lastPaint;
    public SKPath? _lastPath;
    public SKPath PathOutline(SKPaint paint)
    {
        if(_lastPath == null || Count != _lastRenderPointCount || paint.StrokeWidth != _lastPaint.StrokeWidth)
        {
            _lastPaint = paint;
            _lastRenderPointCount = Count;
            var path = ToOptimizedCurve(false);
            _lastPath = paint.GetFillPath(path);
        }
        return _lastPath;
    }
}
