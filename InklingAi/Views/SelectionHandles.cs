namespace InklingAi.Views
{
    using System;
    using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
	using Avalonia;
	using Avalonia.Platform;
	using Avalonia.Svg.Skia;
	using ExCSS;
	using InklingAi.ViewModels;
    using InkPad.Adapters;
    using InkPad.Utils;
    using SkiaSharp;
	using Svg.Skia;
	using Svg.Transforms;

	public class SelectionHandles
    {
        public PaintableImage Paintable { get; }
        Selection Selection => Paintable.Selection;

		private SKSvg _tl = new SKSvg();
		private SKSvg _tr = new SKSvg();
		private SKSvg _br = new SKSvg();
		private SKSvg _bl = new SKSvg();
		private SKSvg _c = new SKSvg();
		private SKPaint _svgPaint = new SKPaint{IsAntialias = true, FilterQuality = SKFilterQuality.High };

		private new Dictionary<RectPosition, SKSvg> _assetMap = new Dictionary<RectPosition, SKSvg>();

		public SelectionHandles(PaintableImage paintable)
        {
            Paintable = paintable;
            LoadAssets();
        }
        private void LoadAssets()
		{
			_tl.Load(AssetLoader.Open(new Uri("avares://InklingAI/Assets/rotate_TL.svg")));
			_tr.Load(AssetLoader.Open(new Uri("avares://InklingAI/Assets/rotate_TR.svg")));
			_bl.Load(AssetLoader.Open(new Uri("avares://InklingAI/Assets/rotate_BL.svg")));
			_br.Load(AssetLoader.Open(new Uri("avares://InklingAI/Assets/rotate_BR.svg")));
			_c.Load(AssetLoader.Open(new Uri("avares://InklingAI/Assets/rotate_C.svg")));
			_assetMap.Add(RectPosition.TopLeft, _tl);
			_assetMap.Add(RectPosition.TopRight, _tr);
			_assetMap.Add(RectPosition.BottomRight, _br);
			_assetMap.Add(RectPosition.BottomLeft, _bl);
			_assetMap.Add(RectPosition.Center, _c);
		}

        public void RenderSkia(SKCanvas canvas)
        {
            if (Selection.Count > 0)
            {
                SKRect? bnds = Selection?.Bounds;
                if (bnds != null)
                {
                    var b = bnds.Value;
                    DrawFrame(canvas, b);
                    DrawHandle(canvas, b, RectPosition.TopLeft);
                    DrawHandle(canvas, b, RectPosition.TopRight);
                    DrawHandle(canvas, b, RectPosition.BottomRight);
                    DrawHandle(canvas, b, RectPosition.BottomLeft);
                    DrawCenter(canvas, b.Center());
                }
            }
        }

        private int _handleHalfWidth = 4;
        private void DrawFrame(SKCanvas canvas, SKRect frame)
        { 
            switch(Selection.TransformKind)
			{
				case TransformKind.Scale:
					canvas.DrawRect(frame, UIRenderUtils.FramePen);
					break;
				case TransformKind.Rotate:
					canvas.DrawRect(frame, UIRenderUtils.FramePen);
					break;
			}
		}
		private void DrawHandle(SKCanvas canvas, SKRect bnds, RectPosition pos)
		{
            var pt = bnds.PointFromPosition(pos);
			switch (Selection.TransformKind)
			{
				case TransformKind.Scale:
                    var rect = new SKRect(pt.X - _handleHalfWidth, pt.Y - _handleHalfWidth, pt.X + _handleHalfWidth, pt.Y + _handleHalfWidth);
                    canvas.DrawRoundRect(rect, 2, 2, UIRenderUtils.SelectionBrush);
                    canvas.DrawRoundRect(rect, 2, 2, UIRenderUtils.BlackOutinePen);
                    break;

                    case TransformKind.Rotate:
                    var pic = _assetMap[pos].Picture;
                    if (pic != null)
                    {
                        var hw = pic.CullRect.Width / 2;
                        var tl = new SKPoint(pt.X - hw, pt.Y - hw);
                        canvas.DrawPicture(_assetMap[pos].Picture, tl, _svgPaint);
                    }
					break;
			}
		}
        
		private void DrawCenter(SKCanvas canvas, SKPoint center)
        {
            switch (Selection.TransformKind)
            {
                case TransformKind.Scale:
                    canvas.DrawCircle(center, _handleHalfWidth, UIRenderUtils.SelectionPen);
                    canvas.DrawCircle(center, 1, UIRenderUtils.SelectionPen);
                    break;
            }
        }
    }

    public enum TransformKind
    {
        None,
        Scale,
        Rotate,
        Envelope
    }
}
