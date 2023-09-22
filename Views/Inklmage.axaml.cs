namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Input;
    using Avalonia.Platform.Storage;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using static SkiaSharp.HarfBuzz.SKShaper;

    public partial class InkImage : UserControl
    {
        public float _widthExtra = 2f;
        private Avalonia.Point? _startPoint;
        private int _minDragDistance = 5;

        public static readonly StyledProperty<string> SourceProperty =
            AvaloniaProperty.Register<GalleryControl, string>(nameof(Source), @"C:\Users\Robin\InklingAI\SD\working\input.png");
        public string Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public InkImage()
        {
            InitializeComponent();

            AddHandler(DragDrop.DropEvent, OnDrop);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);
        }

        private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _startPoint = e.GetPosition(this);
            OnImagePointerMoved(sender, e);
        }

        private async Task DoDrag(object? sender, PointerEventArgs e)
        {
            var dragData = new DataObject();
            dragData.Set(DataFormats.Files, Source); 
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
            if((result & DragDropEffects.Copy) != DragDropEffects.None)
            {
                if (e.Data.Contains(DataFormats.Files))
                {
                    var data = e.Data.Get(DataFormats.Files);
                    if(data is IEnumerable<IStorageItem> dataList)
                    {
                        var firstFile = dataList.FirstOrDefault();
                        var path = firstFile?.Path.AbsolutePath ?? "";
                        if (path != "" && File.Exists(path))
                        {
                            Source = path;
                        }
                    }
                    else if (data is string filename)
                    {
                        Source = filename;
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

        private void OnImagePointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _startPoint = null;
        }


    }
}
