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
    using static SkiaSharp.HarfBuzz.SKShaper;

    public class ProcessedImageData : ImageData
    {
        public override ImageKind ImageKind => VectorImage?.BrushSettings.ImageKind ?? ImageKind.None;
        //private IImage? _undoImage;
        //private IImage? _resetImage;

        public ControlNetConfig? ControlNetConfig { get; set; }
        public string InputTitle => VectorImage?.BrushSettings.Name ?? "";
        public int MinWidth => VectorImage?.BrushSettings.MinWidth ?? 1;
        public int MaxWidth => VectorImage?.BrushSettings.MaxWidth ?? 100;
        public int FeatherMin => VectorImage?.BrushSettings.FeatherMin ?? 0;
        public int FeatherMax => VectorImage?.BrushSettings.FeatherMax ?? 50;
        public int AlphaMin => VectorImage?.BrushSettings.AlphaMin ?? 0;
        public int AlphaMax => VectorImage?.BrushSettings.AlphaMax ?? 255;

        public override string SourcePath
        {
            get => _sourcePath;
            set
            {
                ReloadSourceImage();
                //_sourcePath = value;
            }
        }

        private string _processedPath = "";

        public string ProcessedPath
        {
            get => _processedPath;
            set
            {
                //_processedPath = value;
                ReloadProcessedImage();
                //ProcessedImage = InklingIOUtils.LoadSKImage(_processedPath);
            }
        }
        private SKBitmap? _processedImage;
        public SKBitmap? ProcessedImage { get => _processedImage; private set => _processedImage = value; }

        public override SKBitmap? CurrentImage
        {
            get
            {
                SKBitmap? result = null;
                switch (Origin)
                {
                    case ImageOrigin.Input:
                    case ImageOrigin.Masked:
                    case ImageOrigin.Composite:
                        result = SourceImage;
                        break;
                    case ImageOrigin.Processed:
                        result = ProcessedImage;
                        break;

                }
                return result;
            }
            set
            {
                switch (Origin)
                {
                    case ImageOrigin.Input:
                    case ImageOrigin.Masked:
                    case ImageOrigin.Composite:
                        _sourceImage = value;
                        break;
                    case ImageOrigin.Processed:
                        _processedImage = value;
                        break;

                }
                DoResetLinked();
                //if (ControlNetConfig != null)
                //{
                //    ControlNetConfig.RunFilter();
                //}
            }
        }

        public ProcessedImageData(string path) : base(path)
        {
            _sourcePath = path;
            ReloadSourceImage();
        }
        public ProcessedImageData(BrushSettings brushSettings, string folder) : base(null)
        {
            VectorImage = new VectorImage(brushSettings);
            VectorImage.Name = InputTitle;

            Folder = folder;
            _sourcePath = Folder + brushSettings.ImageKind.Name() + "_input.png";
            _processedPath = Folder + ImageKind.Name() + ".png";
            ReloadSourceImage();
            ReloadProcessedImage();
            Origin = ImageOrigin.Processed;
        }

        public void ReloadSourceImage()
        {
            SourceImage = _sourcePath != null ? InklingIOUtils.LoadSKImage(_sourcePath) : null;
        }
        public void ReloadProcessedImage()
        {
            ProcessedImage = _processedPath != null ? InklingIOUtils.LoadSKImage(_processedPath) : null;
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
