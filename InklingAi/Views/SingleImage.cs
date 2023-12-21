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

    public class SingleImage : BaseImage
    {
        public SingleImage()
        {
        }

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
                        pid.ControlNetConfig.RunFilter();
                    }
                }
            }
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
                    if (change.NewValue is string source)
                    {
                        var imgData = Data ?? new ImageData(source);
                        SetValue(DataProperty, imgData);
                    }
                    break;
            }
        }

    }
}
