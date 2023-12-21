namespace InklingAi.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Avalonia.Data;
    using Avalonia;
    using SkiaSharp;
    using System.IO;
    using InklingAi.Utils;
    using AIEnv.Utils;
    using InklingAi.ViewModels;
    using Avalonia.Threading;

    public class FileImage : BaseImage
    {
        public override SKBitmap? SourceImage
        {
            get => _data?.SourceImage;
            protected set { if (_data != null) _data.SourceImage = value; }
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
        protected override void RenderSkia(SKCanvas canvas)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Source != null && File.Exists(Source))
                {
                    var img = InklingIOUtils.LoadSKImage(Source);
                    DrawImage(canvas, img);
                }
            });
        }
    }
}
