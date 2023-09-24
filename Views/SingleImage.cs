namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Media;
    using Avalonia.Controls;
    using Avalonia.Metadata;

    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class SingleImage : BaseImage
    {
        #region StyledProperties
        public static readonly StyledProperty<string> PrimarySourceProperty =
            AvaloniaProperty.Register<GalleryControl, string>(nameof(PrimarySource), @"C:\Users\Robin\InklingAI\SD\working\edit.png");
        public override string PrimarySource
        {
            get { return GetValue(PrimarySourceProperty); }
            set { SetValue(PrimarySourceProperty, value); }
        }
        #endregion

        public SingleImage()
        {
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var source = _images.Count > 0 ? _images[0] : null;
            var result = new Size();

            if (source != null)
            {
                result = Stretch.CalculateSize(availableSize, source.Size, StretchDirection.Both);
            }

            return result;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size result;
            var source = _images.Count > 0 ? _images[0] : null;
            if (source != null)
            {
                var sourceSize = source.Size;
                result = Stretch.CalculateSize(finalSize, sourceSize);
            }
            else
            {
                result = new Size();
            }
            return result;
        }
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var bmp = _images.Count > 0 ? _images[0] : null;
            if (bmp != null && Bounds.Width > 0 && Bounds.Height > 0)
            {
                Rect viewPort = new Rect(Bounds.Size);
                Size sourceSize = bmp.Size;

                Vector scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection.Both);
                Size scaledSize = sourceSize * scale;
                Rect destRect = viewPort
                    .CenterRect(new Rect(scaledSize))
                    .Intersect(viewPort);
                Rect sourceRect = new Rect(sourceSize)
                    .CenterRect(new Rect(destRect.Size / scale));

                context.DrawImage(bmp, sourceRect, destRect);

                //if (IsPointerOver)
                //{
                //    context.DrawRectangle(Brushes.LightBlue, null, destRect);
                //}
            }
        }

    }
}
