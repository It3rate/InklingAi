namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    // ColorAllowance = ColorAllowance.Color, MinWidth=1, MaxWidth=100, FeatherMin=0, FeatherMax=50
    public class ImageConstraints
    {
        public ImageKind ImageKind = ImageKind.None;
        public string Name = "";
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
