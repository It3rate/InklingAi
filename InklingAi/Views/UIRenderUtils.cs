using InklingAi.ViewModels;
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
		private SKPaint _brushOutine = SKRenderUtils.CreatePen(0xFFDDDD00, 1);

		public void RenderHoverBrush(SKCanvas canvas, DrawingData data)
		{
			canvas.DrawCircle(data.CurrentPosition.X, data.CurrentPosition.Y, data.BrushSize, _brushOutine);
		}
		public void RenderSelectionFrame(SKCanvas canvas)
		{
		}
	}
}
