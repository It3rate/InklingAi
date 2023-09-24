namespace InklingAi.ViewModels
{
    using Avalonia.Controls;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ImageData
    {
        public string Folder { get; set; }
        public ImageKind ImageKind { get; set; }
        public string InputTitle { get; set; }
        public bool IsActive { get; set; } = true;
        public string InputImagePath { get; set; }
        public string ProcessedPath { get; set; }
        // undo image
        // reset image
        // latent data/image
        public ImageData(ImageKind kind, string title, string folder) 
        { 
            IsActive = true;
            ImageKind = kind;
            InputTitle = title;
            Folder = folder;
            InputImagePath = folder + kind.Name() + "_input.png";
            ProcessedPath = folder + kind.Name() + ".png";
        }
        public ImageData(ImageKind kind, string path)
        {
            ImageKind = kind;
            Folder = Path.GetPathRoot(path);
            InputImagePath = path;
            ProcessedPath = path;
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
