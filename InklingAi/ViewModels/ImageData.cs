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
        public ImageKind ImageKind { get; set; }
        public string InputTitle { get; set; }
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

        public PaintableImage PaintableImage { get; set; } = new PaintableImage();

        public ImageData(ImageKind kind, string title, string folder) 
        {
            PaintableImage.Name = title;

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
            InputTitle = "";
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
                result[kind.Name()] = new ImageData(kind, inputKinds[kind].Name, folder);
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

        public static readonly Dictionary<ImageKind, ImageConstraints> inputKinds = new ()
        {
            //{ "main", "Main Image" },
            { ImageKind.Input, new (){Name="Input Image" } },
            { ImageKind.Mask, new (){Name="Mask", ColorAllowance = ColorAllowance.Grayscale } },
            //{ "scratch0", "Scratch 1" },
            //{ "scratch1", "Scratch 2" },
            //{ "scratch2", "Scratch 3" },
            { ImageKind.Canny, new (){Name="Edges", ColorAllowance = ColorAllowance.BlackAndWhite, MaxWidth=1, FeatherMax=0, AlphaMin=255 }},
            { ImageKind.Depth, new (){Name="Depth M", ColorAllowance = ColorAllowance.Grayscale }},
            { ImageKind.Leres, new (){Name="Depth LR", ColorAllowance = ColorAllowance.Grayscale }},
            { ImageKind.Hed, new (){Name="Outline", ColorAllowance = ColorAllowance.Grayscale }},
            { ImageKind.Openpose, new (){Name="Pose", ColorAllowance = ColorAllowance.None, MaxWidth=0, FeatherMax=0,  AlphaMin=255 }},
            { ImageKind.Normal, new (){Name="Normals", ColorAllowance = ColorAllowance.Normal, AlphaMin=255 }},
            { ImageKind.Mlsd, new (){Name="Lines", ColorAllowance = ColorAllowance.Lines, MaxWidth=1, FeatherMax=0, AlphaMin=255 }},
            { ImageKind.Pidinet, new (){Name="Sketch", ColorAllowance = ColorAllowance.Grayscale }},
            { ImageKind.Uniformer, new (){Name="Segment", ColorAllowance = ColorAllowance.ColorSeg, FeatherMax=0,  AlphaMin=255 }},
        };
    }
    // ColorAllowance = ColorAllowance.Color, MinWidth=1, MaxWidth=100, FeatherMin=0, FeatherMax=50
    public class ImageConstraints
    {
        public string Name;
        public ColorAllowance ColorAllowance = ColorAllowance.Color;
        public int MinWidth = 1;
        public int MaxWidth = 100;
        public int FeatherMin = 0;
        public int FeatherMax = 100;
        public int AlphaMin = 0;
        public int AlphaMax = 255;
    }

    public enum ColorAllowance
    {
        None, // pose etc
        BlackAndWhite,
        Grayscale,
        Color,
        Normal,
        Lines,
        ColorSeg,
    }
}
