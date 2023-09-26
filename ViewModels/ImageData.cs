namespace InklingAi.ViewModels
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Media;
    using Avalonia.Media.Imaging;
    using DynamicData;
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

        public string InputPath { get; set; }
        private IImage? _inputImage;
        public IImage? InputImage {
            get
            {
                if (_inputImage == null)
                {
                    _inputImage = LoadImage(InputPath);
                }
                return _inputImage;
            }
            set { _inputImage = value; }
        }

        public string ProcessedPath { get; set; }
        private IImage? _processedImage;
        public IImage? ProcessedImage
        {
            get
            {
                if (_processedImage == null)
                {
                    _processedImage = LoadImage(ProcessedPath);
                }
                return _processedImage;
            }
            set { _processedImage = value; }
        }

        private IImage? _undoImage;
        private IImage? _resetImage;
        //private IImage? _latentImage;

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
