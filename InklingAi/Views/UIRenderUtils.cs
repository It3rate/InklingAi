using Avalonia.Media;
using InklingAi.ViewModels;
using InkPad.Adapters;
using InkPad.Renderer;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InklingAi.Views
{
    public class UIRenderUtils
    {
        public static SKPaint BlackOutinePen = SKRenderUtils.CreatePen(0xFF000000, .5f);
        public static SKPaint BrushOutinePen = SKRenderUtils.CreatePen(0xFFDDDD00, 1);
        public static SKPaint FramePen = SKRenderUtils.CreatePen(0x44BBDDFF, 1);
        public static SKPaint SelectionPen = SKRenderUtils.CreatePen(0xAA99DDFF, 1);
        public static SKPaint SelectionBrush = SKRenderUtils.CreateBrush(0xAA5588BB);

        public void RenderHoverBrush(SKCanvas canvas, DrawingData data)
		{
			canvas.DrawCircle(data.CurrentPosition.X, data.CurrentPosition.Y, data.BrushSize, BrushOutinePen);
		}
	}
}
