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

        public VectorImage VectorImage { get; set; } = new VectorImage();

        private BrushSettings _brushSettings;
        public ImageKind ImageKind => _brushSettings.ImageKind;
        public string InputTitle => _brushSettings.Name;
        public int MinWidth => _brushSettings.MinWidth;
        public int MaxWidth => _brushSettings.MaxWidth;
        public int FeatherMin => _brushSettings.FeatherMin;
        public int FeatherMax => _brushSettings.FeatherMax;
        public int AlphaMin => _brushSettings.AlphaMin;
        public int AlphaMax => _brushSettings.AlphaMax;

        public ImageData(BrushSettings imageContraints, string folder) 
        {
            _brushSettings = imageContraints;
            VectorImage.Name = InputTitle;

            Folder = folder;
            InputPath = folder + ImageKind.Name() + "_input.png";
            ProcessedPath = folder + ImageKind.Name() + ".png";
            LoadImage(ProcessedPath);
        }
        public ImageData(ImageKind kind, string path)
        {
            _brushSettings = new BrushSettings() { ImageKind = kind };
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
