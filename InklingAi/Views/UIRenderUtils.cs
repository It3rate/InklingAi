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
        private SKPaint _brushOutine = SKRenderUtils.CreatePen(0xFFDDDD00, 1);
        private SKPaint _selOutine = SKRenderUtils.CreatePen(0xAA0066DD, 1);

        public void RenderHoverBrush(SKCanvas canvas, DrawingData data)
		{
			canvas.DrawCircle(data.CurrentPosition.X, data.CurrentPosition.Y, data.BrushSize, _brushOutine);
		}
		public void RenderSelectionFrame(SKCanvas canvas, ISelectableEntity entity)
		{
            var bnds = entity.Bounds;
            if(bnds != null)
            {
                canvas.DrawRect(bnds.Value, _selOutine);
            }
		}
	}
}
