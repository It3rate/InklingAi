namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Input;
    using SkiaSharp;
    using System.Collections.Generic;
    using InklingAi.ViewModels;
    using Avalonia.Data;
    using Avalonia.Interactivity;
    using Avalonia.Platform.Storage;
    using System.Linq;
    using System.IO;
    using Avalonia.Controls.Shapes;
    using System.ComponentModel;
    using System;
    using System.Reflection;

    public class SingleImage : BaseImage
    {
        #region AvaloniaProperties

        public static readonly StyledProperty<string> SourceProperty =
            AvaloniaProperty.Register<SingleImage, string>(nameof(Source),
                defaultBindingMode: BindingMode.TwoWay);

        public string Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private ImageData? _imageDataItem;
        public static readonly StyledProperty<ImageData?> ImageDataItemProperty =
            AvaloniaProperty.Register<SingleImage, ImageData?>(nameof(ImageDataItem));
        public ImageData? ImageDataItem
        {
            get => GetValue(ImageDataItemProperty);
            set
            {
                if (value != _imageDataItem)
                {
                    if(_imageDataItem != null)
                    {
                        _imageDataItem.ResetSource -= OnRequestRedraw;
                    }

                    _imageDataItem = value;

                    if (_imageDataItem != null)
                    {
                        _imageDataItem.ResetSource += OnRequestRedraw;
                    }
                }
                SetValue(ImageDataItemProperty, value);
            }
        }

        #endregion

        private void OnRequestRedraw()
        {
            Source = _imageDataItem.ProcessedPath;
            InvalidateVisual();
        }
        public SingleImage()
        {
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var prop = change.Property;
            switch (prop.Name)
            {
                case nameof(ImageDataItem):
                    if (change != null) 
                    {
                        if (change.NewValue == null)
                        {
                            ImageDataItem = null;
                        }
                        else if(change.NewValue is ImageData imageData)
                        {
                            ImageDataItem = imageData;
                        }
                    }
                    InvalidateMeasure();
                    InvalidateVisual();
                    break;
                case nameof(Source):
                    if(change.NewValue is string source)
                    {
                        var imgData = ImageDataItem ?? new ImageData(ImageKind.File, source);
                        SetValue(ImageDataItemProperty, imgData);
                    }
                    break;
            }
        }
        protected override void SetImage(ImageData? imageData)
        {
   //         if(_imageData != null)
   //         {
   //             _imageData.ProcessedImage = imageData?.ProcessedImage;
   //             InklingModel.GetRootDataContext(this)?.InfluenceData.ResetActiveItems();
   //             InvalidateMeasure();
   //             InvalidateVisual();
   //         }
   //         else
			//{
                if (imageData?.InputPath != ImageDataItem?.InputPath)
                {
                    ImageDataItem = imageData;
                }
            //}
        }
        protected override void SetDragContents(DataObject dataObject)
        {
            if (ImageDataItem != null)
            {
                dataObject.Set(DataFormats.Files, new List<ImageData>() { ImageDataItem });
            }
        }

        public override void OnDragOver(object? sender, DragEventArgs e)
        {
            if (ImageDataItem != null)
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        public override void OnDrop(object? sender, DragEventArgs e)
        {
            if (ImageDataItem != null)
            {
                var result = e.DragEffects;
                if ((result & DragDropEffects.Copy) != DragDropEffects.None)
                {
                    AcceptImageDrop(e.Data);
                    if (Name == "_editImageControl")
                    {
                        if (_model.InfluenceData.FocusedItem != null)
                        {
                            _model.InfluenceData.FocusedItem.DoResetSource();
                        }
                    }
                }
            }
        }

        protected override void AcceptImageDrop(IDataObject dataObject)
        {
            if (dataObject.Contains(DataFormats.Files))
            {
                var data = dataObject.Get(DataFormats.Files);
                if (data is IEnumerable<IStorageItem> dataList)
                {
                    var firstFile = dataList.FirstOrDefault();
                    var path = firstFile?.Path.AbsolutePath ?? "";
                    if (path != "" && File.Exists(path))
                    {
                        Source = path;
                        //SetImage(new ImageData(ImageKind.File, path));
                    }
                }
                else if (data is IEnumerable<ImageData> stringList)
                {
                    var first = stringList.First();
                    if (first != null)
                    {
                        Source = first.ProcessedPath;
                        ImageDataItem.ProcessedPath = first.ProcessedPath;
                        //SetImage(first);
                    }
                }
                else if (data is string filename)
                {
                    Source = filename;
                    //SetImage(new ImageData(ImageKind.File, filename));
                }
            }
        }
        protected override void RenderSkia(SKCanvas canvas)
        {
                var bmp = _imageDataItem?.ProcessedImage;
                DrawImage(canvas, bmp);
        }

    }
}
