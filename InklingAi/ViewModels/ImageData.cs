namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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

    public class ImageData : IDisposable
    {
        //private IImage? _latentImage;
        // transform matrix
        // source rect
        // mask/alpha channel (can be vector)
        // latent data
        // process config (eg canny settings, or config that created the image)
        // application settings (skip last steps, etc - can be in config)

        // Composite settings:
        // Images[]
        // Blend stack
        // Blend percents

        // Text:
        // letters, styles, font, size, spans etc
        // path, matrix etc
        // ocr
        // gen vs overlay

        public event Action? ResetLinked;
        protected void DoResetLinked()
        {
            ResetLinked?.Invoke();
        }

        public string Folder { get; set; }
        public InputControl? InputControl { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsApplicable { get; set; } = true;
        public ImageOrigin Origin { get; set; } = ImageOrigin.Input;
        private ImageKind _imageKind = ImageKind.File;
        public ImageKind ImageKind => VectorImage?.BrushSettings.ImageKind ?? _imageKind;
        public bool IsFixedPath => ImageKind.HasFixedPath();



        public ControlNetConfig? ControlNetConfig { get; set; }
        public string InputTitle => VectorImage?.BrushSettings.Name ?? "";
        public int MinWidth => VectorImage?.BrushSettings.MinWidth ?? 1;
        public int MaxWidth => VectorImage?.BrushSettings.MaxWidth ?? 100;
        public int FeatherMin => VectorImage?.BrushSettings.FeatherMin ?? 0;
        public int FeatherMax => VectorImage?.BrushSettings.FeatherMax ?? 50;
        public int AlphaMin => VectorImage?.BrushSettings.AlphaMin ?? 0;
        public int AlphaMax => VectorImage?.BrushSettings.AlphaMax ?? 255;



        protected SKBitmap? _sourceImage;
        public SKBitmap? SourceImage
        {
            get => _sourceImage;
            set
            {
                _sourceImage = value;
                if (Origin == ImageOrigin.Input)
                {
                    CurrentImage = _sourceImage;
                }
            }
        }
        protected string _sourcePath = "";
        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                if (IsFixedPath)
                {
                    if(_sourcePath != value)
                    {
                        IOUtils.CopyFile(value, SourcePath);
                    }
                }
                else
                {
                    _sourcePath = value;
                }
                ReloadSourceImage();
            }
        }

        private string _processedPath = "";
        public string ProcessedPath
        {
            get => _processedPath;
            set
            {
                if (IsFixedPath)
                {
                    if (_processedPath != value)
                    {
                        IOUtils.CopyFile(value, ProcessedPath);
                    }
                }
                else
                {
                    _processedPath = value;
                }
                ReloadProcessedImage();
            }
        }
        private SKBitmap? _processedImage;
        public SKBitmap? ProcessedImage
        {
            get => _processedImage;
            set
            {
                _processedImage = value;
                if (Origin == ImageOrigin.Processed)
                {
                    CurrentImage = _processedImage;
                }
            }
        }

        private SKBitmap? _currentImage;
        public SKBitmap? CurrentImage 
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                DoResetLinked();
            }
        }

        public ImageData(string sourcePath)
        {
            Folder = Path.GetPathRoot(sourcePath) ?? "";
            SourcePath = sourcePath;
        }
        public ImageData(BrushSettings brushSettings, string folder)
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
        public VectorImage? VectorImage { get; protected set; }
        
        public void ReloadSourceImage()
        {
            SourceImage = _sourcePath != null ? InklingIOUtils.LoadSKImage(_sourcePath) : null;
        }
        public void ReloadProcessedImage()
        {
            ProcessedImage = _processedPath != null ? InklingIOUtils.LoadSKImage(_processedPath) : null;
        }

        public void Dispose()
        {
            if (SourceImage != null && SourceImage is IDisposable disposableInput)
            {
                disposableInput.Dispose();
                SourceImage = null;
            }

            if (ProcessedImage != null && ProcessedImage is IDisposable disposableProcessed)
            {
                disposableProcessed.Dispose();
                ProcessedImage = null;
            }
        }
    }
}
