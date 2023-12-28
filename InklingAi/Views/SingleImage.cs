namespace InklingAi.Views
{
    using Avalonia;
    using SkiaSharp;
    using InklingAi.ViewModels;
    using AIEnv.Utils;
    using Avalonia.Threading;
    using System.Diagnostics;

    public class SingleImage : BaseImage
    {
        public SKBitmap? SourceImage { get; set; }
        public SKBitmap? ProcessedImage { get; set; }

        public SingleImage()
        {
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var prop = change.Property;
            switch (prop.Name)
            {
                case nameof(Data):
                    if (change.NewValue is ImageData pid)
                    {
                        pid.ResetLinked += OnResetLinked;
                    }
                    break;
            }
        }

        protected void OnResetLinked()
        {
            CurrentImage = Data.CurrentImage;
        }

    }
}
