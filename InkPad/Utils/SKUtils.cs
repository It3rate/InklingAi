namespace InkPad.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

public class SKUtils
{
}

public static class SKRectExtensions
{
    //public static Rect ToRect(this SKRect r) => new Rect(r.Left, r.Top, r.Width, r.Height);
}
public static class RectExtensions
{
    //public static SKRect ToSKRect(this Rect r) => new SKRect((float)r.X, (float)r.Y, (float)(r.X + r.Width), (float)(r.Y + r.Height));

    public static SKPoint TopLeft(this SKRect rect) => new SKPoint(rect.Left, rect.Top);
    public static SKPoint TopRight(this SKRect rect) => new SKPoint(rect.Right, rect.Top);
    public static SKPoint BottomRight(this SKRect rect) => new SKPoint(rect.Right, rect.Bottom);
	public static SKPoint BottomLeft(this SKRect rect) => new SKPoint(rect.Left, rect.Bottom);
    public static SKPoint Center(this SKRect rect) => new SKPoint(rect.MidX, rect.MidY);
	public static SKPoint TopCenter(this SKRect rect) => new SKPoint(rect.MidX, rect.Top);
	public static SKPoint RightCenter(this SKRect rect) => new SKPoint(rect.Right, rect.MidY);
	public static SKPoint BottomCenter(this SKRect rect) => new SKPoint(rect.MidX, rect.Bottom);
	public static SKPoint LeftCenter(this SKRect rect) => new SKPoint(rect.Left, rect.MidY);

    public static SKPoint PointFromPosition(this SKRect rect, RectPosition pos) 
    {
        var result = SKPoint.Empty;
        switch (pos)
        {
			case RectPosition.TopLeft:
				result = rect.TopLeft();
				break;

			case RectPosition.TopRight:
				result = rect.TopRight();
				break;

			case RectPosition.BottomLeft:
				result = rect.BottomLeft();
				break;

			case RectPosition.BottomRight:
				result = rect.BottomRight();
				break;

			case RectPosition.Center:
				result = rect.Center();
				break;

			case RectPosition.TopCenter:
				result = rect.TopCenter();
				break;

			case RectPosition.RightCenter:
				result = rect.RightCenter();
				break;

			case RectPosition.BottomCenter:
				result = rect.BottomCenter();
				break;

			case RectPosition.LeftCenter:
				result = rect.LeftCenter();
				break;
		}
        return result;
    }

	public static SKPoint[] CornerPoints(this SKRect rect)
    {
        return new SKPoint[]
        {
            new SKPoint(rect.Left, rect.Top),
            new SKPoint(rect.Right, rect.Top),
            new SKPoint(rect.Right, rect.Bottom),
            new SKPoint(rect.Left, rect.Bottom)
        };
    }
    public static SKRect FromCornerPoints(this SKPoint[] rectCorners) => SKRect.Create(rectCorners[0], 
        new SKSize(rectCorners[2].X - rectCorners[0].X, rectCorners[2].Y - rectCorners[0].Y));
    public static SKRect Transform(this SKRect rect, SKMatrix matrix)
    {
        var pts = matrix.MapPoints(rect.CornerPoints());
        return FromCornerPoints(pts);
    }
    public static SKPoint Center(this SKPoint[] points)
    {
        var result = SKPoint.Empty;
        if (points.Length > 0)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            foreach (var p in points)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            result = new SKPoint(minX + (maxX - minX) / 2f, minY + (maxY - minY) / 2f);
        }
        return result;
    }
}
public enum RectPosition
{
	None,
	TopLeft,
	TopRight,
    BottomLeft,
    BottomRight,
    Center,
	TopCenter,
	RightCenter,
	BottomCenter,
	LeftCenter,
}
public static class RectPosExtensions
{
	public static RectPosition OppositePosition(this RectPosition pos)
	{
		var result = RectPosition.Center;
		switch (pos)
		{
			case RectPosition.TopLeft:
				result = RectPosition.BottomRight;
				break;
			case RectPosition.TopRight:
				result = RectPosition.BottomLeft;
				break;
			case RectPosition.BottomRight:
				result = RectPosition.TopLeft;
				break;
			case RectPosition.BottomLeft:
				result = RectPosition.TopRight;
				break;
			case RectPosition.TopCenter:
				result = RectPosition.BottomCenter;
				break;
			case RectPosition.RightCenter:
				result = RectPosition.LeftCenter;
				break;
			case RectPosition.BottomCenter:
				result = RectPosition.TopCenter;
				break;
			case RectPosition.LeftCenter:
				result = RectPosition.RightCenter;
				break;
		}
		return result;
	}
	public static bool IsEdge(this RectPosition pos)
	{
		var result = false;
		switch (pos)
		{
			case RectPosition.TopCenter:
			case RectPosition.RightCenter:
			case RectPosition.BottomCenter:
			case RectPosition.LeftCenter:
				result = true;
				break;
		}
		return result;
	}
	public static bool IsCorner(this RectPosition pos)
	{
		var result = false;
		switch (pos)
		{
			case RectPosition.TopLeft:
			case RectPosition.TopRight:
			case RectPosition.BottomRight:
			case RectPosition.BottomLeft:
				result = true;
				break;
		}
		return result;
	}
}

public static class ColorExtensions
{
    public static byte Alpha(this uint color) => (byte)((color & 0xFF000000) >> 24);
    public static byte Red(this uint color) => (byte)((color & 0x00FF0000) >> 16);
    public static byte Green(this uint color) => (byte)((color & 0x0000FF00) >> 8);
    public static byte Blue(this uint color) => (byte)((color & 0x000000FF));

}