namespace InklingAi.ViewModels
{
    using AIEnv.Clients.StableDiffusion;
    using AIEnv.Utils;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Media;
    using Avalonia.Media.Imaging;
    using DynamicData;
    using InklingAi.Utils;
    using InklingAi.Views;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum ImageOrigin
    {
        Input,
        Processed,
        Composite,
        Masked,
    }

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

        public event Action ResetLinked;
        public InputControl? InputControl { get; set; }
        public ControlNetConfig? ControlNetConfig { get; set; }
        public string Folder { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsApplicable { get; set; } = true;
        public ImageOrigin Origin { get; set; }

        private string? _inputPath;
        public SKBitmap? InputImage { get; private set; }
        public string? InputPath 
        { 
            get => _inputPath;
            set
            {
                _inputPath = value;
                InputImage = _inputPath != null ? InklingIOUtils.LoadSKImage(_inputPath) : null;
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
        public VectorImage VectorImage { get; private set; }

        private IImage? _undoImage;
        private IImage? _resetImage;
        public ImageKind ImageKind
        {
            get => VectorImage.BrushSettings.ImageKind;
            set => VectorImage.BrushSettings.ImageKind = value;
        }
        public void DoResetSource()
        {
            ResetLinked?.Invoke();
        }
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
            InklingIOUtils.LoadImage(ProcessedPath);
        }
        public ImageData(ImageKind kind, string path)
        {
            var brushSettings = new BrushSettings() { ImageKind = kind };
            VectorImage = new VectorImage(brushSettings);
            Folder = Path.GetPathRoot(path) ?? "";
            InputPath = path;
            ProcessedPath = path;
            InklingIOUtils.LoadImage(ProcessedPath);
        }

        public void Dispose()
        {
            if (InputImage != null && InputImage is IDisposable disposableInput)
            {
                disposableInput.Dispose();
                InputImage = null;
            }
            if (ProcessedImage != null && ProcessedImage is IDisposable disposableProcessed)
            {
                disposableProcessed.Dispose();
                ProcessedImage = null;
            }
            if (_undoImage != null && _undoImage is IDisposable disposableUndo)
            {
                disposableUndo.Dispose();
                _undoImage = null;
            }
            if (_resetImage != null && _resetImage is IDisposable disposableReset)
            {
                disposableReset.Dispose();
                _resetImage = null;
            }
        }

    }
}
