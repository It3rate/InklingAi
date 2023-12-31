namespace AIEnv.Clients.StableDiffusion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading.Tasks;
    using AIEnv.Utils;
    using SkiaSharp;

    public class MaskModuleConfig : SideModuleConfig
    {
        public float Levels { get; set; } = 1f; // 0 is black and white, 1 is full grayscale
        public float MinGray { get; set; } = 0f;
        public float MaxGray { get; set; } = 1f;
        public float ConversionThreshold => (MaxGray - MinGray) / 2f + MinGray;

        public string SourcePath{ get; private set; }
        public string DestinationPath { get; private set; }
        public MaskModuleConfig(string sourcePath, string destinationPath) : base("mask")
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }

        public override async Task<string> RunFilter()
        {
            return await Task.Run(() => RunFilterInternal(SourcePath, DestinationPath));
        }

        public override async Task<string> RunFilter(string source, string destination)
        {
            return await Task.Run(() => RunFilterInternal(source, destination));
        }

        // B&W
        private static SKPaint _bwPainter = GetBWPainter();
        private static SKPaint GetBWPainter(int threshold = 128)
        {
            byte[] alphaRemapTable = new byte[256];
            byte[] redRemapTable = new byte[256];
            byte[] greenRemapTable = new byte[256];
            byte[] blueRemapTable = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                alphaRemapTable[i] = (byte)i;
                redRemapTable[i] =  i < threshold ? (byte)0 : (byte)255;
                greenRemapTable[i] = i < threshold ? (byte)0 : (byte)255;
                blueRemapTable[i] = i < threshold ? (byte)0 : (byte)255;
            }
            var colorFilter = SKColorFilter.CreateTable(alphaRemapTable, redRemapTable, greenRemapTable, blueRemapTable);
            var paint = new SKPaint { ColorFilter = colorFilter };
            return paint;
        }


    // Gray

    private static SKColorFilter _grayFilter = SKColorFilter.CreateColorMatrix(new float[]
                                                {
                                                    0.21f, 0.72f, 0.07f, 0, 0,
                                                    0.21f, 0.72f, 0.07f, 0, 0,
                                                    0.21f, 0.72f, 0.07f, 0, 0,
                                                    0,     0,     0,     1, 0
                                                });
        private static SKPaint _grayPainter = new SKPaint() { ColorFilter = _grayFilter };
        public string RunFilterInternal(string source, string destination)
        {
            var result = source;
            var sourceImage = IOUtils.GetLocalImage(source);
            if (sourceImage != null)
            {
                var destImage = new SKBitmap(sourceImage.Width, sourceImage.Height);

                using (var canvas = new SKCanvas(destImage))
                {
                    canvas.Clear(SKColors.Transparent);
                    canvas.DrawBitmap(sourceImage, new SKPoint(0, 0), _grayPainter);
                    canvas.DrawBitmap(destImage, new SKPoint(0, 0), _bwPainter);
                    IOUtils.SaveLocalImage(destImage, destination);
                    result = destination;
                }
            }
            return result;
        }
    }
}
