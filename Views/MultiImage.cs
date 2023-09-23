namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Automation.Peers;
    using Avalonia.Controls;
    using Avalonia.Controls.Automation.Peers;
    using Avalonia.Input;
    using Avalonia.Media;
    using Avalonia.Media.Imaging;
    using Avalonia.Metadata;
    using Avalonia.Platform.Storage;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class MultiImage : UserControl
    {
        private List<IImage> _images = new();

        #region StyledProperties
        public static readonly StyledProperty<Stretch> StretchProperty =
            AvaloniaProperty.Register<Image, Stretch>(nameof(Stretch), Stretch.Uniform);
        public Stretch Stretch
        {
            get { return GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public static readonly StyledProperty<IImage?> SourceProperty =
            AvaloniaProperty.Register<Image, IImage?>(nameof(Source));
        [Content]
        public IImage? Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        #endregion

        public MultiImage()
        {
            AffectsRender<Image>(SourceProperty, StretchProperty);
            AffectsMeasure<Image>(SourceProperty, StretchProperty);
        }
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var prop = change.Property;
            switch (prop.Name)
            {
                case nameof(Source):
                    _images.Clear();
                    if (change != null && change.NewValue != null && change.NewValue is IImage img)
                    {
                        _images.Add(img);
                    }
                    InvalidateVisual();
                    break;
            }
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
