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

    public static SKPoint Center(this SKRect rect) => new SKPoint(rect.MidX, rect.MidY);
    public static SKPoint TopLeft(this SKRect rect) => new SKPoint(rect.Left, rect.Top);
    public static SKPoint TopRight(this SKRect rect) => new SKPoint(rect.Right, rect.Top);
    public static SKPoint BottomRight(this SKRect rect) => new SKPoint(rect.Right, rect.Bottom);
    public static SKPoint BottomLeft(this SKRect rect) => new SKPoint(rect.Left, rect.Bottom);
}
