namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Avalonia.Media;
    using Avalonia.Skia;
    using InklingAi.Utils;
    using SkiaSharp;

    // ColorAllowance = ColorAllowance.Color, MinWidth=1, MaxWidth=100, FeatherMin=0, FeatherMax=50
    public class BrushSettings
    {
        public ImageKind ImageKind = ImageKind.None;
        public string Name = "";
        public ColorAllowance ColorAllowance = ColorAllowance.Color;
        public uint DefaultColor = 0xFF0072B2;
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
    public static class ColorAllowanceExtensions
    {
        public static Color Clamp(this ColorAllowance colorAllowance, Color inputColor)
        {
            var result = colorAllowance.Clamp(inputColor.ToSKColor());
            return result.ToColor();
        }
        public static SKColor Clamp(this ColorAllowance colorAllowance, SKColor inputColor)
        {
            SKColor result;
            switch (colorAllowance)
            {
                case ColorAllowance.BlackAndWhite:
                case ColorAllowance.Lines:
                    result = inputColor.ToBlackAndWhite();
                    break;
                case ColorAllowance.Grayscale:
                    result = inputColor.ToGrayscale();
                    break;
                case ColorAllowance.Normal:
                    result = inputColor.ToFullySaturated();
                    break;
                case ColorAllowance.ColorSeg:
                    result = inputColor.ToFullySaturated(); // todo: convert to color tables of yolo
                    break;
                default:
                    result = inputColor;
                    break;

            }
            return result;
        }
    }
}
