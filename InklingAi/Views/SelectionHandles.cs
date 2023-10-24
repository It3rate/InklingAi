namespace InklingAi.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InkPad.Adapters;
    using InkPad.Utils;
    using SkiaSharp;

    public class SelectionHandles
    {
        public PaintableImage Paintable { get; }
        ISelectableEntity? Selection => Paintable.SelectedEntity;
        TransformKind TransformKind { get; set; } = TransformKind.Scale;

        public SelectionHandles(PaintableImage paintable)
        {
            Paintable = paintable;
        }
        public void RenderSkia(SKCanvas canvas)
        {
            SKRect? bnds = Selection?.Bounds;
            if (bnds != null)
            {
                DrawFrame(canvas, bnds.Value);
                DrawHandle(canvas, bnds.Value.TopLeft());
                DrawHandle(canvas, bnds.Value.TopRight());
                DrawHandle(canvas, bnds.Value.BottomRight());
                DrawHandle(canvas, bnds.Value.BottomLeft());
                DrawCenter(canvas, bnds.Value.Center());
            }
        }

        private int _handleHalfWidth = 4;
        private void DrawFrame(SKCanvas canvas, SKRect frame)
        { 
            switch(TransformKind)
            {
                case TransformKind.Scale:
                    canvas.DrawRect(frame, UIRenderUtils.FramePen);
                    break;
            }
        }
        private void DrawHandle(SKCanvas canvas, SKPoint pt)
        {
            var rect = new SKRect(pt.X - _handleHalfWidth, pt.Y - _handleHalfWidth, pt.X + _handleHalfWidth, pt.Y + _handleHalfWidth);
            switch(TransformKind)
            {
                case TransformKind.Scale:
                    canvas.DrawRoundRect(rect, 2, 2, UIRenderUtils.SelectionBrush);
                    canvas.DrawRoundRect(rect, 2, 2, UIRenderUtils.BlackOutinePen);
                    break;
            }
        }
        private void DrawCenter(SKCanvas canvas, SKPoint center)
        {
            switch (TransformKind)
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
