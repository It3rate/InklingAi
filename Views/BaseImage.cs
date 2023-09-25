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

        public abstract ImageData ImageDataItem { get; set; }
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

        private void OnLoaded(object? sender, VisualTreeAttachmentEventArgs e)
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
                case nameof(ImageDataItem):
                    _imageDataList.Clear();
                    if (change != null && change.NewValue != null && change.NewValue is ImageData imageData)
                    {
                        _imageDataList.Add(imageData);
                    }
                    InvalidateVisual();
                    break;
            }
        }

        protected override Size MeasureOverride(Size availableSize) => Stretch.CalculateSize(availableSize, AspectSize, StretchDirection.Both);
        protected override Size ArrangeOverride(Size finalSize) => Stretch.CalculateSize(finalSize, AspectSize);

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
        private void OnDragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void OnDrop(object? sender, DragEventArgs e)
        {
            var result = e.DragEffects;
            if ((result & DragDropEffects.Copy) != DragDropEffects.None)
            {
                if (e.Data.Contains(DataFormats.Files))
                {
                    var data = e.Data.Get(DataFormats.Files);
                    if (data is IEnumerable<IStorageItem> dataList)
                    {
                        var firstFile = dataList.FirstOrDefault();
                        var path = firstFile?.Path.AbsolutePath ?? "";
                        if (path != "" && File.Exists(path))
                        {
                            ImageDataItem = new ImageData(ImageKind.File, path);
                        }
                    }
                    else if (data is IEnumerable<ImageData> stringList)
                    {
                        var first = stringList.First();
                        if(first != null)
                        {
                            ImageDataItem = first;
                        }
                    }
                    else if (data is string filename && filename != ImageDataItem.InputPath)
                    {
                        ImageDataItem = new ImageData(ImageKind.File, filename);
                    }
                }
            }
        }
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
