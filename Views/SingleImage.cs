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
    using InklingAi.ViewModels;

    public class SingleImage : BaseImage
    {
        #region AvaloniaProperties

        public static readonly DirectProperty<SingleImage, string> SourceProperty =
        AvaloniaProperty.RegisterDirect<SingleImage, string>(
            nameof(Source),
            o => o.Source,
            (o, v) => o.Source = v);

        public string Source
        {
            get => _images.Count > 0 ? _images[0].ProcessedPath : "";
            set
            {
                var imgData = new ImageData(ImageKind.File, value);
                SetValue(PrimarySourceProperty, imgData);
            }
        }

        //public static readonly StyledProperty<string> SourceProperty =
        //    AvaloniaProperty.Register<SingleImage, string>(nameof(Source), @"C:\Users\Robin\InklingAI\SD\working\edit.png");
        //public string Source
        //{
        //    get { return GetValue(SourceProperty); }
        //    set { SetValue(SourceProperty, value); }
        //}

        public static readonly StyledProperty<ImageData> PrimarySourceProperty =
            AvaloniaProperty.Register<SingleImage, ImageData>(nameof(PrimarySource));
        public override ImageData PrimarySource
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

            if (source != null && source.ProcessedImage != null)
            {
                result = Stretch.CalculateSize(availableSize, source.ProcessedImage.Size, StretchDirection.Both);
            }

            return result;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size result;
            var source = _images.Count > 0 ? _images[0] : null;
            if (source != null && source.ProcessedImage != null)
            {
                var sourceSize = source.ProcessedImage.Size;
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

            var bmp = _images.Count > 0 && _images[0].ProcessedImage != null ? _images[0].ProcessedImage : null;
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
