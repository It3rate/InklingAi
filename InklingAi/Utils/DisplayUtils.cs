namespace InklingAi.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AIEnv.Utils;
    using Avalonia;
    using Avalonia.Media;
    using InklingAi.ViewModels;
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
            var brightness = (byte)(v * 2.55f);
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
    public static class SKSizeExtensions
    {
        public static SKSize ToSKSize(this Size s)
        {
            return new SKSize((float)s.Width, (float)s.Height);
        }
    }
    public static class SKRectExtensions
    {
        public static SKMatrix CreateTransformMatrix(this SKRect boundingBox, float dx, float dy, float rotationDegrees)
        {
            var matrix = SKMatrix.CreateIdentity();
            matrix = matrix.PreConcat(SKMatrix.CreateTranslation(-boundingBox.MidX, -boundingBox.MidY));
            matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees(rotationDegrees));
            matrix = matrix.PreConcat(SKMatrix.CreateTranslation(boundingBox.MidX + dx, boundingBox.MidY + dy));
            return matrix;
        }
        public static SKRect ZoomRect(this SKRect frame, float zoomLevel, SKPoint panAmount)
        {
            var result = SKRect.Create(frame.Location, frame.Size);
            if (zoomLevel != 1f || panAmount.X != 0 || panAmount.Y != 0)
            {
                float zoomFactor = zoomLevel <= .2f ? .2f : zoomLevel > 4f ? 4f : zoomLevel;
                zoomFactor = 1f / zoomFactor;
                var w = (int)(frame.Width * zoomFactor);
                var h = (int)(frame.Height * zoomFactor);
                var x = frame.Left + (int)(panAmount.X - (result.Width - frame.Width) / 2.0f);
                var y = frame.Top + (int)(panAmount.Y - (result.Height - frame.Height) / 2.0f);
                result = SKRect.Create(x, y, w, h);
            }
            return result;
        }
    }
    public static class SKBitmapExtensions
    {
        public static SKSize Size(this SKBitmap bmp)
        {
            return new SKSize(bmp.Width, bmp.Height);
        }
        public static SKRect Rect(this SKBitmap bmp)
        {
            return new SKRect(0, 0, bmp.Width, bmp.Height);
        }
        public static Avalonia.Size ToAvaloniaSize(this SKBitmap bmp)
        {
            return new Avalonia.Size(bmp.Width, bmp.Height);
        }
        public static SKBitmap ResizeZoom(this SKBitmap bmp, float zoomLevel, SKPoint panAmount, int targetWidth = -1, int targetHeight = -1)
        {
            var result = bmp;
            var curSize = InklingModel.Instance.SDConfig.AspectSize;
            var orgFrame = SKRect.Create(0, 0, bmp.Width, bmp.Height);
            var frame = orgFrame.ZoomRect(zoomLevel, panAmount);
            targetWidth = (targetWidth == -1) ? curSize.Width : targetWidth;
            targetHeight = (targetHeight == -1) ? curSize.Height : targetHeight;
            if (frame.Width != targetWidth || frame.Height != targetHeight || frame.Left != 0 || frame.Top != 0)
            {
                float ratioWidth = (float)targetWidth / frame.Width;
                float ratioHeight = (float)targetHeight / frame.Height;
                float ratio = Math.Max(ratioWidth, ratioHeight);

                int newWidth = (int)(frame.Width * ratio);
                int newHeight = (int)(frame.Height * ratio);

                result = new SKBitmap(targetWidth, targetHeight);

                using (var canvas = new SKCanvas(result))
                {
                    using (SKPaint paint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High })
                    {
                        canvas.Clear(SKColors.Transparent);
                        int x = (targetWidth - newWidth) / 2;
                        int y = (targetHeight - newHeight) / 2;
                        var src = SKRect.Create(x, y, newWidth, newHeight);

                        canvas.DrawBitmap(bmp, src, frame, paint);
                    }
                }
            }

            return result;
        }
    }
}
