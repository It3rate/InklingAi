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

    public abstract class BaseImage : UserControl
    {
        protected List<ImageData> _images = new();
        public float _widthExtra = 2f;
        private Point? _startPoint;
        private int _minDragDistance = 8;

        public static readonly StyledProperty<Stretch> StretchProperty =
            AvaloniaProperty.Register<BaseImage, Stretch>(nameof(Stretch), Stretch.Uniform);
        public Stretch Stretch
        {
            get { return GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public abstract ImageData PrimarySource { get; set; }

        public BaseImage()
        {
            SetValue(DragDrop.AllowDropProperty, true);
            PointerPressed += OnImagePointerPressed;
            PointerMoved += OnImagePointerMoved;
            PointerReleased += OnImagePointerReleased;

            AddHandler(DragDrop.DropEvent, OnDrop);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var prop = change.Property;
            switch (prop.Name)
            {
                case nameof(PrimarySource):
                    _images.Clear();
                    if (change != null && change.NewValue != null && change.NewValue is ImageData imageData)
                    {
                        _images.Add(imageData);
                    }
                    InvalidateVisual();
                    break;
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
            dragData.Set(DataFormats.Files, _images);
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
                            PrimarySource = new ImageData(ImageKind.File, path);
                        }
                    }
                    else if (data is IEnumerable<ImageData> stringList)
                    {
                        var first = stringList.First();
                        if(first != null)
                        {
                            PrimarySource = first;
                        }
                    }
                    else if (data is string filename && filename != PrimarySource.InputPath)
                    {
                        PrimarySource = new ImageData(ImageKind.File, filename);
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
