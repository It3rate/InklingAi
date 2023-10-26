namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Media;
    using Avalonia.Skia;
    using Avalonia.Input;
    using Avalonia.Platform;
    using Avalonia.Platform.Storage;
    using Avalonia.Media.Imaging;
    using Avalonia.Rendering.SceneGraph;
    using Avalonia.Controls;
    using Avalonia.Metadata;
    using SkiaSharp;

    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using InklingAi.ViewModels;
    using System.IO;
    using Avalonia.Interactivity;
    using Avalonia.Data;
    using MathNet.Numerics;

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

        public static readonly StyledProperty<ImageData?> ImageDataItemProperty =
            AvaloniaProperty.Register<SingleImage, ImageData?>(nameof(ImageDataItem));
        public ImageData? ImageDataItem
        {
            get => GetValue(ImageDataItemProperty);
            set => SetValue(ImageDataItemProperty, value);
        }
        #endregion

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
                    _imageDataList.Clear();
                    if (change != null && change.NewValue != null && change.NewValue is ImageData imageData)
                    {
                        _imageDataList.Add(imageData);
                    }
                    InvalidateMeasure();
                    InvalidateVisual();
                    break;
                case nameof(Source):
                    if(change.NewValue is string source)
                    {
                        var imgData = new ImageData(ImageKind.File, source);
                        SetValue(ImageDataItemProperty, imgData);
                    }
                    break;
            }
        }
        protected override void SetImage(ImageData? imageData)
        {
            if(_imageDataList.Count > 0)
            {
                _imageDataList[0].ProcessedImage = imageData?.ProcessedImage;
                InklingModel.GetRootDataContext(this)?.InfluenceData.ResetActiveItems();
                InvalidateMeasure();
                InvalidateVisual();
            }
            else
			{
                if (imageData?.InputPath != ImageDataItem?.InputPath)
                {
                    ImageDataItem = imageData;
                }
            }
        }

        protected override void OnDragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;
        }
        protected override void OnDrop(object? sender, DragEventArgs e)
        {
            var result = e.DragEffects;
            if ((result & DragDropEffects.Copy) != DragDropEffects.None)
            {
                AcceptImageDrop(e.Data);
            }
        }

        protected override void RenderSkia(SKCanvas canvas)
        {
            var bmp = _imageDataList.Count > 0 ? _imageDataList[0].ProcessedImage : null;
            DrawImage(canvas, bmp);
        }

    }
}
