namespace InklingAi.ViewModels
{

    using System.Collections.Generic;
    using System.Diagnostics;

    public enum ImageKind
    {
        None,
        File,
        Main,
        Input,
        Mask,
        Erase,
        Scratch0,
        Scratch1,
        Scratch2,
        Canny,
        Depth,
        Leres,
        Hed,
        Openpose,
        Normal,
        Mlsd,
        Pidinet,
        Uniformer,
        Yolo,
    }
    public static class ImageKindExtensions
    {
        public static readonly Dictionary<ImageKind, string> inputKindNames = new()
        {
            {ImageKind.None, "" },
            {ImageKind.File,      "file" },
            {ImageKind.Main,      "main" },
            {ImageKind.Input,     "input" },
            {ImageKind.Mask,      "mask" },
            {ImageKind.Erase,     "erase" },
            {ImageKind.Yolo,      "yolo" },
            {ImageKind.Scratch0,  "scratch0" },
            {ImageKind.Scratch1,  "scratch1" },
            {ImageKind.Scratch2,  "scratch2" },
            {ImageKind.Canny,     "cn_canny" },
            {ImageKind.Depth,     "cn_depth" },
            {ImageKind.Leres,     "cn_leres" },
            {ImageKind.Hed,       "cn_hed" },
            {ImageKind.Openpose,  "cn_openpose" },
            {ImageKind.Normal,    "cn_normal" },
            {ImageKind.Mlsd,      "cn_mlsd" },
            {ImageKind.Pidinet,   "cn_pidinet" },
            {ImageKind.Uniformer, "cn_uniformer" },
        };
        public static string Name(this ImageKind kind) => inputKindNames.ContainsKey(kind) ? inputKindNames[kind] : "";

        public static ImageKind KindFromName(string name)
        {
            var result = ImageKind.None;
            foreach (var kind in inputKindNames)
            {
                if(kind.Value == name)
                {
                    result = kind.Key;
                    break;
                }
            }
            return result;
        }
    }
}