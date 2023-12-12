namespace InklingAi.ViewModels
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Media;
    using Avalonia.Media.Imaging;
    using DynamicData;
    using InklingAi.Views;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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


        public InputControl? InputControl { get; set; }

        public string Folder { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsApplicable { get; set; } = true;

        private IImage? _undoImage;
        private IImage? _resetImage;
        public string InputPath { get; set; }
        private SKBitmap? _inputImage;
        public SKBitmap? InputImage {
            get
            {
                if (_inputImage == null)
                {
                    _inputImage = LoadSKImage(InputPath);
                }
                return _inputImage;
            }
            set { _inputImage = value; }
        }

        public string ProcessedPath { get; set; }
        private SKBitmap? _processedImage;
        public SKBitmap? ProcessedImage
        {
            get
            {
                if (_processedImage == null)
                {
                    _processedImage = LoadSKImage(ProcessedPath);
                }
                return _processedImage;
            }
            set { _processedImage = value; }
        }

        public VectorImage VectorImage { get; private set; }

        public ImageKind ImageKind => VectorImage.BrushSettings.ImageKind;
        public string InputTitle => VectorImage.BrushSettings.Name;
        public int MinWidth => VectorImage.BrushSettings.MinWidth;
        public int MaxWidth => VectorImage.BrushSettings.MaxWidth;
        public int FeatherMin => VectorImage.BrushSettings.FeatherMin;
        public int FeatherMax => VectorImage.BrushSettings.FeatherMax;
        public int AlphaMin => VectorImage.BrushSettings.AlphaMin;
        public int AlphaMax => VectorImage.BrushSettings.AlphaMax;

        public ImageData(BrushSettings brushSettings, string folder) 
        {
            VectorImage = new VectorImage(brushSettings);
            VectorImage.Name = InputTitle;

            Folder = folder;
            InputPath = folder + ImageKind.Name() + "_input.png";
            ProcessedPath = folder + ImageKind.Name() + ".png";
            LoadImage(ProcessedPath);
        }
        public ImageData(ImageKind kind, string path)
        {
            var brushSettings = new BrushSettings() { ImageKind = kind };
            VectorImage = new VectorImage(brushSettings);
            Folder = Path.GetPathRoot(path) ?? "";
            InputPath = path;
            ProcessedPath = path;
            LoadImage(ProcessedPath);
        }

        private SKBitmap? LoadSKImage(string? path)
        {
            SKBitmap? result = null;
            if (path != null &&  path != "" && File.Exists(path))
            {
                using (SKFileStream stream = new SKFileStream(path))
                {
                    result = SKBitmap.Decode(stream);
                }
            }
            return result;
        }
        private IImage? LoadImage(string? path)
        {
            IImage? result = null;
            if (path != null &&  path != "" && File.Exists(path))
            {
                result = new Bitmap(path);
            }
            return result;
        }

        public void Dispose()
        {
            if (_inputImage is IDisposable disposableInput)
            {
                disposableInput.Dispose();
                _inputImage = null;
            }
            if (_processedImage is IDisposable disposableProcessed)
            {
                disposableProcessed.Dispose();
                _processedImage = null;
            }
            if (_undoImage is IDisposable disposableUndo)
            {
                disposableUndo.Dispose();
                _undoImage = null;
            }
            if (_resetImage is IDisposable disposableReset)
            {
                disposableReset.Dispose();
                _resetImage = null;
            }
        }

    }
}
