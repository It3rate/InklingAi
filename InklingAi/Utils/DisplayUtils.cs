namespace InklingAi.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Avalonia.Media;
    using SkiaSharp;

    public static class SKColorExtensions
    {
        public static SKColor ToBlackAndWhite(this SKColor color)
        {
            int avg = (color.Red + color.Green + color.Blue) / 3;
            return avg > 127 ? SKColors.White : SKColors.Black;
        }

        public static SKColor ToGrayscale(this SKColor color)
        {
            color.ToHsv(out float _, out _, out float v);
            var brightness = (byte)(v * 255);
            return new SKColor(brightness, brightness, brightness, color.Alpha);
        }

        public static SKColor ToFullySaturated(this SKColor color)
        {
            color.ToHsl(out float h, out float s, out float l);
            return SKColor.FromHsl(h, 100f, 50f);
        }
        public static Color ToColor(this SKColor c)
        {
            return new Color(c.Alpha, c.Red, c.Green, c.Blue);
        }
    }
}
