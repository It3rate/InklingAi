namespace InklingAi.Views
{
    using Avalonia;
    using SkiaSharp;
    using InklingAi.ViewModels;
    using AIEnv.Utils;
    using Avalonia.Threading;

    public class SingleImage : BaseImage
    {
        public SKBitmap? ProcessedImage { get; set; }
        private async void ProcessInput()
        {
            if (Data != null)
            {
                if (Data.ControlNetConfig != null)
                {
                    Data.ReloadSourceImage();
                    InvalidateVisual();

                    await Data.ControlNetConfig.RunFilter();

                    await Dispatcher.UIThread.InvokeAsync(() => Data.Origin = ImageOrigin.Processed);
                    Data.ReloadProcessedImage(); 
                    InvalidateVisual();
                }
            }
        }

        private bool IsProcessedPath => Data?.Origin == ImageOrigin.Processed;

        public SingleImage()
        {
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
                        IOUtils.CopyFile(source, Data.SourcePath);
                        //SourceImage = InklingIOUtils.LoadSKImage(ProcessedImageData.SourcePath);
                        //Data = _data;
                        ProcessInput();
                    }
                    break;
                case nameof(SourceImage):
                    if (change.NewValue is SKBitmap sourceImage)
                    {
                        if(Data != null)
                        {
                            IOUtils.SaveLocalImage(sourceImage, Data.SourcePath);
                            Data.Origin = ImageOrigin.Input;
                        }
                        CurrentImage = sourceImage;
                        ProcessInput();
                    }
                    break;
                case nameof(ProcessedImage):
                    if (change.NewValue is SKBitmap processedImage && Data != null)
                    {
                        IOUtils.SaveLocalImage(processedImage, Data.ProcessedPath);
                        Data.Origin = ImageOrigin.Processed;
                        CurrentImage = processedImage;
                    }
                    break;
                case nameof(CurrentImage):
                    if (change.NewValue is SKBitmap currentImage)
                    {
                        _currentImage = currentImage;
                    }
                    break;
            }
        }


    }
}
