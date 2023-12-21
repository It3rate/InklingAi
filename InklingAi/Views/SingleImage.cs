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

    public class SingleImage : BaseImage
    {
        public static readonly StyledProperty<ImageData?> DataProperty =
            AvaloniaProperty.Register<SingleImage, ImageData?>(nameof(Data));
        public override ImageData? Data
        {
            get => GetValue(DataProperty);
            set
            {
                if (value != _data)
                {
                    if (_data != null)
                    {
                        _data.ResetLinked -= OnRedrawLinked;
                    }
                    _data = value;
                }
                SetValue(DataProperty, value);

                if (_data != null)
                {
                    _data.ResetLinked += OnRedrawLinked;
                    if (_data is ProcessedImageData pid && pid.ControlNetConfig != null)
                    {
                        //pid.ControlNetConfig.RunFilter();
                    }
                }
            }
        }
        public override string CurrentPath
        {
            get => ProcessedImageData?.ProcessedPath ?? Source;
            set
            {
                if (Data.Origin == ImageOrigin.Processed && ProcessedImageData != null)
                {
                    ProcessedImageData.ProcessedPath = value;
                }
                else
                {
                    Source = value;
                }
            }
        }

        private ProcessedImageData? ProcessedImageData => Data != null && Data is ProcessedImageData pid ? pid : null;
        private bool IsProcessedPath => ProcessedImageData?.Origin == ImageOrigin.Processed;

        public SingleImage()
        {
        }

        private void OnRedrawLinked()
        {
            Source = _data?.SourcePath ?? "";
            InvalidateVisual();
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
                        IOUtils.CopyFile(source, CurrentPath);
                        CurrentPath = source;
                        Data = _data;
                    }
                    break;
            }
        }

    }
}
