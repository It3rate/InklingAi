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
        public void DoResetSource()
        {
            ResetLinked?.Invoke();
        }

        public InputControl? InputControl { get; set; }
        public string Folder { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsApplicable { get; set; } = true;
        public virtual ImageKind ImageKind => ImageKind.File;
        public VectorImage? VectorImage { get; protected set; }


        private string? _sourcePath;
        public virtual SKBitmap? SourceImage { get; set; }
        public string? SourcePath 
        { 
            get => _sourcePath;
            set
            {
                _sourcePath = value;
                SourceImage = _sourcePath != null ? InklingIOUtils.LoadSKImage(_sourcePath) : null;
            }
        }
        public ImageData(string path)
        {
            Folder = Path.GetPathRoot(path) ?? "";
            SourcePath = path;

            //var brushSettings = new BrushSettings() { ImageKind = kind };
            //VectorImage = new VectorImage(brushSettings);
            //ProcessedPath = path;
            //InklingIOUtils.LoadImage(ProcessedPath);
        }
        public virtual void Dispose()
        {
            if (SourceImage != null && SourceImage is IDisposable disposableInput)
            {
                disposableInput.Dispose();
                SourceImage = null;
            }
        }
    }
}
