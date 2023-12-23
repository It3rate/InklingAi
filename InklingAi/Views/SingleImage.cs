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
    using AIEnv.Utils;
    using static SkiaSharp.HarfBuzz.SKShaper;
    using Avalonia.Threading;
    using System.Threading.Tasks;
    using InklingAi.Utils;

    public class SingleImage : BaseImage
    {
        //public static readonly StyledProperty<string> SourceProperty =
        //AvaloniaProperty.Register<SingleImage, string>(nameof(Source),
        //    defaultBindingMode: BindingMode.TwoWay);
        //public string Source
        //{
        //    get
        //    {
        //        var result = "";
        //        if (_data != null)
        //        {
        //            result = _data.SourcePath;
        //            GetValue(SourceProperty);
        //        }
        //        return result;
        //    }
        //    set
        //    {
        //        if (_data != null)
        //        {
        //            _data.SourcePath = value;
        //            SetValue(SourceProperty, value);
        //        }
        //    }
        //}

        //public static readonly StyledProperty<ImageData?> DataProperty =
        //    AvaloniaProperty.Register<SingleImage, ImageData?>(nameof(Data));
        //public override ImageData? Data
        //{
        //    get => GetValue(DataProperty);
        //    set
        //    {
        //        if (value != _data)
        //        {
        //            if (_data != null)
        //            {
        //                _data.ResetLinked -= OnRedrawLinked;
        //            }
        //            _data = value;
        //        }
        //        SetValue(DataProperty, value);

        //        if (_data != null)
        //        {
        //            _data.ResetLinked += OnRedrawLinked;
        //        }
        //    }
        //}

        //private async void ProcessInput()
        //{
        //    if (ProcessedImageData != null)
        //    {
        //        if (_data is ProcessedImageData pid && pid.ControlNetConfig != null)
        //        {
        //            _data.Origin = ImageOrigin.Input;
        //            var resultPath = await pid.ControlNetConfig.RunFilter();
        //            if(_data != null)
        //            {
        //                _data.Origin = ImageOrigin.Processed;
        //            }
        //        }
        //    }
        //}
        private async void ProcessInput()
        {
            if (ProcessedImageData != null)
            {
                if (Data.ControlNetConfig != null)
                {
                    //await Dispatcher.UIThread.InvokeAsync(() => Data.Origin = ImageOrigin.Input);
                    ProcessedImageData.ReloadSourceImage();
                    InvalidateVisual();

                    // Run CPU-bound work on a background thread
                    //var resultPath = await Task.Run(() => pid.ControlNetConfig.RunFilter());
                    await Data.ControlNetConfig.RunFilter();
                    // Again, update UI-bound properties on the UI thread

                    await Dispatcher.UIThread.InvokeAsync(() => Data.Origin = ImageOrigin.Processed);
                    ProcessedImageData.ReloadProcessedImage(); 
                    InvalidateVisual();
                    //Data = _data;
                }
            }
        }

        private ProcessedImageData? ProcessedImageData => Data != null && Data is ProcessedImageData pid ? pid : null;
        private bool IsProcessedPath => ProcessedImageData?.Origin == ImageOrigin.Processed;

        public SingleImage()
        {
        }

        //private void OnRedrawLinked()
        //{
        //    Source = _data?.SourcePath ?? "";
        //    InvalidateVisual();
        //}
        protected override void RenderSkia(SKCanvas canvas)
        {
            //if (_data != null && ProcessedImageData != null && _data.Origin == ImageOrigin.Processed)
            //{
            //    DrawImage(canvas, ProcessedImageData.ProcessedImage);
            //}
            //else
            //{
 //               DrawImage(canvas, SourceImage);
            //}
        }
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var prop = change.Property;
            switch (prop.Name)
            {
                case nameof(Source):
                    if (change.NewValue is string source && Data != null)
                    {
                        IOUtils.CopyFile(source, ProcessedImageData.SourcePath);
//                        SourceImage = InklingIOUtils.LoadSKImage(ProcessedImageData.SourcePath);
                        //Data = _data;
                        ProcessInput();
                    }
                    break;
            }
        }

    }
}
