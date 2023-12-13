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

        public static SKPaint CreatePen(uint colorValue, float brushSize, float feather)
        {
            var col = new SKColor(colorValue);
            SKMaskFilter? blur = null;
            if (feather > 0)
            {
                blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, feather / 2);
            }
            //var gradientStops = new SKColor[] 
            //{
            //    col, // Opaque at the start of the stroke
            //    SKColors.Transparent // Transparent at the end of the stroke
            //};
            var paint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = col,
                StrokeWidth = brushSize,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeMiter = 3.0f,
                IsAntialias = true,
                //Shader = SKShader.CreateLinearGradient(
                //    new SKPoint(0, 0), // Start point of the gradient
                //    new SKPoint(0, brushSize), // End point of the gradient
                //    gradientStops,
                //    new float[] { 0, 1 }, // Positions of the gradient stops
                //    SKShaderTileMode.Clamp), 
                MaskFilter = blur,

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
