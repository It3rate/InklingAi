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
            get => _imageDataList.Count > 0 ? _imageDataList[0].ProcessedPath : "";
            set
            {
                var imgData = new ImageData(ImageKind.File, value);
                SetValue(ImageDataItemProperty, imgData);
            }
        }

        public static readonly StyledProperty<ImageData?> ImageDataItemProperty =
            AvaloniaProperty.Register<SingleImage, ImageData?>(nameof(ImageDataItem));
        public override ImageData? ImageDataItem
        {
            get => GetValue(ImageDataItemProperty);
            set => SetValue(ImageDataItemProperty, value);
        }
        #endregion

        public SingleImage()
        {
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var bmp = _imageDataList.Count > 0 ? _imageDataList[0].ProcessedImage : null;
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
            }
        }

    }
}
