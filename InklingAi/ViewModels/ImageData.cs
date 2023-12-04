namespace InklingAi.ViewModels
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Media;
    using Avalonia.Media.Imaging;
    using DynamicData;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ImageData : IDisposable
    {
        public string Folder { get; set; }
        public ImageKind ImageKind { get; set; }
        public string InputTitle { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsApplicable { get; set; } = true;

        private IImage? _undoImage;
        private IImage? _resetImage;
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


        public ImageData(ImageKind kind, string title, string folder) 
        { 
            ImageKind = kind;
            InputTitle = title;
            Folder = folder;
            InputPath = folder + kind.Name() + "_input.png";
            ProcessedPath = folder + kind.Name() + ".png";
            LoadImage(ProcessedPath);
        }
        public ImageData(ImageKind kind, string path)
        {
            ImageKind = kind;
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

        public static Dictionary<string, ImageData> GenerateInputItems(string folder)
        {
            Dictionary<string, ImageData> result = new();
            foreach (var kind in inputKinds.Keys)
            {
                result[kind.Name()] = new ImageData(kind, inputKinds[kind], folder);
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

        public static readonly Dictionary<ImageKind, string> inputKinds = new ()
        {
            //{ "main", "Main Image" },
            { ImageKind.Input, "Input Image" },
            { ImageKind.Mask, "Mask" },
            //{ "scratch0", "Scratch 1" },
            //{ "scratch1", "Scratch 2" },
            //{ "scratch2", "Scratch 3" },
            { ImageKind.Canny, "Edges" },
            { ImageKind.Depth, "Depth M" },
            { ImageKind.Leres, "Depth LR" },
            { ImageKind.Hed, "Outline" },
            { ImageKind.Openpose, "Pose" },
            { ImageKind.Normal, "Normals" },
            { ImageKind.Mlsd, "Lines" },
            { ImageKind.Pidinet, "Sketch" },
            { ImageKind.Uniformer, "Segment" },
        };
    }

}
