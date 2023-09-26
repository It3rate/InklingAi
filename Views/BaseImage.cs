namespace InklingAi.Views
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Avalonia;
    using Avalonia.Input;
    using Avalonia.Media;
    using Avalonia.Controls;
    using Avalonia.Media.Imaging;
    using Avalonia.Platform.Storage;
    using InklingAi.ViewModels;
    using Avalonia.Data;
    using Avalonia.VisualTree;

    public abstract class BaseImage : UserControl
    {
        protected List<ImageData> _imageDataList = new();
        public float _widthExtra = 2f;
        private Point? _startPoint;
        private int _minDragDistance = 8;

        #region Avalonia Properties

        public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<BaseImage, Stretch>(nameof(Stretch), Stretch.UniformToFill);
        public Stretch Stretch
        {
            get { return GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public static readonly StyledProperty<Size> AspectSizeProperty = AvaloniaProperty.Register<BaseImage, Size>(nameof(AspectSize), new Size(640,512));
        public Size AspectSize
        {
            get { return GetValue(AspectSizeProperty); }
            set { SetValue(AspectSizeProperty, value); }
        }

        #endregion

        public BaseImage()
        {
            SetValue(DragDrop.AllowDropProperty, true);
            PointerPressed += OnImagePointerPressed;
            PointerMoved += OnImagePointerMoved;
            PointerReleased += OnImagePointerReleased;

            AddHandler(DragDrop.DropEvent, OnDrop);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);

            AttachedToVisualTree += OnLoaded;
        }

        protected virtual void OnLoaded(object? sender, VisualTreeAttachmentEventArgs e)
        {
            var mvm = MainViewModel.GetRootDataContext(this);
            if (mvm != null)
            {
                var binding = new Binding("AspectSize")
                {
                    Mode = BindingMode.OneWay,
                    Source = mvm
                };
                this.Bind(AspectSizeProperty, binding);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var prop = change.Property;
            switch (prop.Name)
            {
                case nameof(AspectSize):
                    InvalidateMeasure();
                    InvalidateVisual();
                    break;
            }
        }

        protected override Size MeasureOverride(Size availableSize) => Stretch.CalculateSize(availableSize, AspectSize, StretchDirection.Both);
        protected override Size ArrangeOverride(Size finalSize) => Stretch.CalculateSize(finalSize, AspectSize);

        protected void DrawImage(IImage? img, DrawingContext context, double alpha = 1.0)
        {
            if (img != null && Bounds.Width > 0 && Bounds.Height > 0)
            {
                Rect viewPort = new Rect(Bounds.Size);
                Size sourceSize = img.Size;

                Vector scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection.Both);
                Size scaledSize = sourceSize * scale;

                Rect destRect = viewPort
                    .CenterRect(new Rect(scaledSize))
                    .Intersect(viewPort);
                Rect sourceRect = new Rect(sourceSize)
                    .CenterRect(new Rect(destRect.Size / scale));

                if(alpha != 1.0 && img is IImageBrushSource iibs)
                {
                    var imageBrush = new ImageBrush(iibs)
                    {
                        //SourceRect = new RelativeRect(sourceRect, RelativeUnit.Absolute),
                        //DestinationRect = new RelativeRect(destRect, RelativeUnit.Absolute),
                        Opacity = alpha,
                        Stretch = Stretch.Fill
                    };
                    context.FillRectangle(imageBrush, destRect);

                }
                else
                {
                    context.DrawImage(img, sourceRect, destRect);
                }
            }
        }

        #region Mouse Events
        private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _startPoint = e.GetPosition(this);
            OnImagePointerMoved(sender, e);
        }

        private async Task DoDrag(object? sender, PointerEventArgs e)
        {
            var dragData = new DataObject();
            dragData.Set(DataFormats.Files, _imageDataList);
            var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
            _startPoint = null;
        }
        protected virtual void OnDragOver(object? sender, DragEventArgs e)
        {
        }

        protected abstract void OnDrop(object? sender, DragEventArgs e);

        private void OnImagePointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_startPoint.HasValue)
                return;

            var currentPosition = e.GetPosition(this);
            var diff = currentPosition - _startPoint.Value;

            if (Math.Abs(diff.X) > _minDragDistance || Math.Abs(diff.Y) > _minDragDistance)
            {
                _ = DoDrag(sender, e);
            }
        }

        private void OnImagePointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _startPoint = null;
        }
        #endregion
    }
}
