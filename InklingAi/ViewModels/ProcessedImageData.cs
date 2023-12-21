namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AIEnv.Clients.StableDiffusion;
    using AIEnv.Utils;
    using Avalonia.Media;
    using InklingAi.Utils;
    using InklingAi.Views;
    using SkiaSharp;

    public class ProcessedImageData : ImageData
    {
        public override ImageKind ImageKind => VectorImage?.BrushSettings.ImageKind ?? ImageKind.None;

        public ControlNetConfig? ControlNetConfig { get; set; }
        public ImageOrigin Origin { get; set; }

        public override SKBitmap? SourceImage 
        { 
            get => base.SourceImage;
            set
            {
                base.SourceImage = value;
                if (ControlNetConfig != null)
                {
                    ControlNetConfig.RunFilter();
                }
            }
        }

        private string? _processedPath;
        public SKBitmap? ProcessedImage { get; private set; }
        public string? ProcessedPath
        {
            get => _processedPath;
            set
            {
                _processedPath = value;
                ProcessedImage = _processedPath != null ? InklingIOUtils.LoadSKImage(_processedPath) : null;
            }
        }

        //private IImage? _undoImage;
        //private IImage? _resetImage;

        public string InputTitle => VectorImage?.BrushSettings.Name ?? "";
        public int MinWidth => VectorImage?.BrushSettings.MinWidth ?? 1;
        public int MaxWidth => VectorImage?.BrushSettings.MaxWidth ?? 100;
        public int FeatherMin => VectorImage?.BrushSettings.FeatherMin ?? 0;
        public int FeatherMax => VectorImage?.BrushSettings.FeatherMax ?? 50;
        public int AlphaMin => VectorImage?.BrushSettings.AlphaMin ?? 0;
        public int AlphaMax => VectorImage?.BrushSettings.AlphaMax ?? 255;

        public ProcessedImageData(BrushSettings brushSettings, string folder) : base(folder + brushSettings.ImageKind.Name() + "_input.png")
        {
            VectorImage = new VectorImage(brushSettings);
            VectorImage.Name = InputTitle;

            Folder = folder;
            //SourcePath = folder + ImageKind.Name() + "_input.png";
            ProcessedPath = folder + ImageKind.Name() + ".png";
            //InklingIOUtils.LoadImage(ProcessedPath);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (ProcessedImage != null && ProcessedImage is IDisposable disposableProcessed)
            {
                disposableProcessed.Dispose();
                ProcessedImage = null;
            }
            //if (_undoImage != null && _undoImage is IDisposable disposableUndo)
            //{
            //    disposableUndo.Dispose();
            //    _undoImage = null;
            //}
            //if (_resetImage != null && _resetImage is IDisposable disposableReset)
            //{
            //    disposableReset.Dispose();
            //    _resetImage = null;
            //}
        }
    }
}
