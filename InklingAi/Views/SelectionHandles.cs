namespace InklingAi.Views
{
    using System;
    using System.Collections.Generic;
	using Avalonia.Platform;
	using InklingAi.ViewModels;
    using InkPad.Utils;
    using SkiaSharp;
	using Svg.Skia;
	using static SkiaSharp.HarfBuzz.SKShaper;

	public class SelectionHandles
    {
        public PaintableImage Paintable { get; }
        Selection Selection => Paintable.Selection;
        public SKPoint TransformAnchor { get; set; } = SKPoint.Empty;

		private SKSvg _tl = new SKSvg();
		private SKSvg _tr = new SKSvg();
		private SKSvg _br = new SKSvg();
		private SKSvg _bl = new SKSvg();
		private SKSvg _c = new SKSvg();
		private SKPaint _svgPaint = new SKPaint{IsAntialias = true, FilterQuality = SKFilterQuality.High };

		private Dictionary<RectPosition, SKSvg> _assetMap = new Dictionary<RectPosition, SKSvg>();
        private Dictionary<RectPosition, SKRect> _dragHandleHitAreas = new Dictionary<RectPosition, SKRect>();

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

			_dragHandleHitAreas.Add(RectPosition.TopLeft, SKRect.Empty);
			_dragHandleHitAreas.Add(RectPosition.TopRight, SKRect.Empty);
			_dragHandleHitAreas.Add(RectPosition.BottomRight, SKRect.Empty);
			_dragHandleHitAreas.Add(RectPosition.BottomLeft, SKRect.Empty);
			_dragHandleHitAreas.Add(RectPosition.Center, SKRect.Empty);
			_dragHandleHitAreas.Add(RectPosition.TopCenter, SKRect.Empty);
			_dragHandleHitAreas.Add(RectPosition.RightCenter, SKRect.Empty);
			_dragHandleHitAreas.Add(RectPosition.BottomCenter, SKRect.Empty);
			_dragHandleHitAreas.Add(RectPosition.LeftCenter, SKRect.Empty);
		}
        public RectPosition HitTest(SKPoint point)
        {
            var result = RectPosition.None;
            if (Selection.HasSelection)
            {
                foreach (var rp in _dragHandleHitAreas) 
                {
                    if (rp.Value.Contains(point))
                    {
                        result = rp.Key;
                        break;
                    }
                }
            }
			return result;
        }

        public bool IsHandlePressed => CurrentDownHandle != RectPosition.None;
		public RectPosition CurrentDownHandle { get; set; } = RectPosition.None;
		private SKRect StartTransformBounds { get; set; } = SKRect.Empty;
		private SKRect _bounds { get; set; } = SKRect.Empty;
		public void StartDragHandle(RectPosition hnd)
        {
            CurrentDownHandle = hnd;
            StartTransformBounds = _bounds;

			if (hnd == RectPosition.Center)
            {
				TransformAnchor = _bounds.PointFromPosition(hnd);
			}
            else if(hnd == RectPosition.None)
            {
                TransformAnchor = SKPoint.Empty;
			}
            else
			{
				TransformAnchor = _bounds.PointFromPosition(hnd.OppositePosition());
			}
		}
		public void EndDragHandle()
		{
			StartTransformBounds = SKRect.Empty;
			CurrentDownHandle = RectPosition.None;
			TransformAnchor = SKPoint.Empty;
		}

		public void RenderSkia(SKCanvas canvas)
        {
            if (Selection.HasSelection)
            {
                SKRect? bnds = Selection?.Bounds;
                if (bnds != null)
                {
					_bounds = bnds.Value;
                    DrawFrame(canvas, _bounds);
                    DrawHandle(canvas, _bounds, RectPosition.TopLeft);
                    DrawHandle(canvas, _bounds, RectPosition.TopRight);
                    DrawHandle(canvas, _bounds, RectPosition.BottomRight);
                    DrawHandle(canvas, _bounds, RectPosition.BottomLeft);
                    DrawCenter(canvas, _bounds.Center());
                }
            }
        }

        private static int _handleHalfWidth = 4;
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
		private SKRect DrawHandle(SKCanvas canvas, SKRect bnds, RectPosition pos)
		{
            var result = SKRect.Empty;
            var pt = bnds.PointFromPosition(pos);
			switch (Selection.TransformKind)
			{
				case TransformKind.Scale:
					result = new SKRect(pt.X - _handleHalfWidth, pt.Y - _handleHalfWidth, pt.X + _handleHalfWidth, pt.Y + _handleHalfWidth);
                    canvas.DrawRoundRect(result, 2, 2, UIRenderUtils.SelectionBrush);
                    canvas.DrawRoundRect(result, 2, 2, UIRenderUtils.BlackOutinePen);
                    break;

                case TransformKind.Rotate:
                    var pic = _assetMap[pos].Picture;
                    if (pic != null)
                    {
                        var w = pic.CullRect.Width;
                        var tl = new SKPoint(pt.X - w / 2, pt.Y - w / 2);
                        canvas.DrawPicture(_assetMap[pos].Picture, tl, _svgPaint);
                        result = new SKRect(tl.X, tl.Y, tl.X + w, tl.Y + w);
                    }
					break;
			}
            
            result.Inflate(6, 6);
            _dragHandleHitAreas[pos] = result;
            return result;
		}
        
		private SKRect DrawCenter(SKCanvas canvas, SKPoint center)
        {
            SKRect result = SKRect.Empty;
            switch (Selection.TransformKind)
			{
				case TransformKind.Scale:
                    break;
				case TransformKind.Rotate:
					result = new SKRect(
                        center.X - _handleHalfWidth, 
                        center.Y - _handleHalfWidth, 
                        center.X + _handleHalfWidth,
						center.Y + _handleHalfWidth);
					canvas.DrawCircle(center, _handleHalfWidth, UIRenderUtils.SelectionPen);
                    canvas.DrawCircle(center, 1, UIRenderUtils.SelectionPen);
                    break;
            }

			result.Inflate(6, 6);
			_dragHandleHitAreas[RectPosition.Center] = result;
			return result;
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
