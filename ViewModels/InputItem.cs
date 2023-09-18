namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class InputItem
    {
        public string InputKind { get; set; }
        public string InputTitle { get; set; }
        public bool IsActive { get; set; } = true;
        public string InputImagePath { get; set; }
        public string ProcessedPath { get; set; }
        public InputItem(string kind, string title, string folder) 
        { 
            InputKind = kind;
            InputTitle = title;
            IsActive = true;
            InputImagePath = folder + kind + "_input.png";
            ProcessedPath = folder + kind + ".png";
        }

        public static Dictionary<string, InputItem> GenerateInputItems(string folder)
        {
            Dictionary<string, InputItem> result = new();
            foreach (var kind in inputKinds.Keys)
            {
                result[kind] = new InputItem(kind, inputKinds[kind], folder);
            }
            return result;
        }
        public static readonly Dictionary<string, string> inputKinds = new ()
        {
            //{ "main", "Main Image" },
            { "input", "Input Image" },
            { "mask", "Mask" },
            //{ "scratch0", "Scratch 1" },
            //{ "scratch1", "Scratch 2" },
            //{ "scratch2", "Scratch 3" },
            { "cn_canny", "Edges" },
            { "cn_depth", "Depth M" },
            { "cn_leres", "Depth LR" },
            { "cn_hed", "Outline" },
            { "cn_openpose", "Pose" },
            { "cn_normal", "Normals" },
            { "cn_mlsd", "Lines" },
            { "cn_pidinet", "Sketch" },
            { "cn_uniformer", "Segment" },
        };
    }

}
