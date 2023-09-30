namespace InklingAi.Onnx
{
    using Microsoft.ML.OnnxRuntime;
    using Microsoft.ML.OnnxRuntime.Tensors;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using InklingAi.Utils;
    using Avalonia.Media;
    using Avalonia;
    using Avalonia.Media.Imaging;
    using Avalonia.Platform;
    using SkiaSharp;

    public class VaeDecoder : IDisposable
    {
        public InferenceSession inferenceSession;
        public int DeviceId = 0;

        //public VaeDecoder(string modelPath = @"F:\tmp\vae\sd_ink\vae_decoder\model.onnx")
        //public VaeDecoder(string modelPath = @"F:\tmp\vae\sd_ink\opt\vae_decoder\model.onnx")
        public VaeDecoder(string modelPath = @"F:\tmp\vae\sd_ink\clean\vae_decoder\model.onnx")
        {
            inferenceSession = OnnxUtils.GetCudaSession(modelPath); // ,@"F:\tmp\vae\sd_ink\clean\vae_decoder\model.onnx");
        }

        unsafe public SKBitmap? DecodePath(string latentPath, IntSize size)
        {
            SKBitmap? result = null;
            var tensorShape = new int[] { 1, 4, (int)Math.Floor(size.Height / 8.0), (int)Math.Floor(size.Width / 8.0) };
            var latentData = IOUtils.GetLatentTensor(latentPath, tensorShape);
            if(latentData != null)
            {
                result = Decode(latentData);
            }
            GC.Collect();
            return result;
        }
        unsafe public SKBitmap? Decode(DenseTensor<Float16> input)
        {
            SKBitmap? result = null;
            List<NamedOnnxValue> namedOnnxValues = new List<NamedOnnxValue>();
            namedOnnxValues.Add(NamedOnnxValue.CreateFromTensor("latent_sample", input));
            var output = inferenceSession.Run(namedOnnxValues);
            if (output != null)
            {
                Tensor<byte> tensor = output.ToList().First().Value as Tensor<byte>;
                result = ConvertToImage(tensor);
            }
            return result;
        }

        public SKBitmap? ConvertToImage(Tensor<byte> tensor)
        {
            var h = tensor.Dimensions[0];
            var w = tensor.Dimensions[1];
            var bytes = tensor.ToArray<byte>();
            bytes = ConvertBGRToRGBA(bytes);
            GCHandle bytesHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                SKBitmap result = new SKBitmap(w, h, SKColorType.Rgba8888, SKAlphaType.Opaque);
                result.SetPixels(bytesHandle.AddrOfPinnedObject());
                return result;
            }
            finally
            {
                bytesHandle.Free();
            }
        }

        private byte[] ConvertBGRToRGBA(byte[] original)
        {
            int length = original.Length;
            if (length % 3 != 0)
            {
                throw new ArgumentException("Invalid 24-bit RGB data");
            }

            byte[] converted = new byte[(length / 3) * 4];
            for (int i = 0, j = 0; i < length; i += 3, j += 4)
            {
                converted[j + 2] = original[i];     // R
                converted[j + 1] = original[i + 1]; // G
                converted[j + 0] = original[i + 2]; // B
                converted[j + 3] = 255; // Ignored byte, can be any value
            }

            return converted;
        }
        public void Dispose()
        {
            inferenceSession?.Dispose();
        }

    }
}
