using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkPad.Renderer
{
    public class SKRenderUtils
    {

        public static SKPaint CreatePen(uint colorValue, float brushSize)
        {
            var paint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(colorValue),
                StrokeWidth = brushSize,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeMiter = 3.0f,
                IsAntialias = true,
            };
            return paint;
        }
        public static SKPaint CreateBrush(uint colorValue)
        {
            var paint = new SKPaint()
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(colorValue),
                IsAntialias = true,
            };
            return paint;
        }
    }
}
