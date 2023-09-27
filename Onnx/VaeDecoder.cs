namespace InklingAi.Onnx
{
    using Microsoft.ML.OnnxRuntime;
    using Microsoft.ML.OnnxRuntime.Tensors;
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using InklingAi.Utils;

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

        unsafe public Image? DecodePath(string latentPath, Size size)
        {
            Image? result = null;
            var tensorShape = new int[] { 1, 4, (int)Math.Floor(size.Height / 8.0), (int)Math.Floor(size.Width / 8.0) };
            var latentData = IOUtils.GetLatentTensor(latentPath, tensorShape);
            if(latentData != null)
            {
                result = Decode(latentData);
            }
            GC.Collect();
            return result;
        }
        unsafe public Image? Decode(DenseTensor<Float16> input)
        {
            Image? result = null;
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

        public Image? ConvertToImage(Tensor<byte> tensor)
        {
            var h = tensor.Dimensions[0];
            var w = tensor.Dimensions[1];
            var bytes = tensor.ToArray<byte>();
            GCHandle bytesHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var result = new Bitmap(w, h, w * 3, PixelFormat.Format24bppRgb, bytesHandle.AddrOfPinnedObject());
            //bytesHandle.Free();
            return result;

            //IOUtils.SaveLocalImage(result, path);

            //var imageName = name == "" ? $"sd_image_{DateTime.Now.ToString("yyyyMMddHHmm")}.png" : name;
            //var imagePath = Path.Combine(Directory.GetCurrentDirectory(), path, imageName);
            //result.Save(imagePath);
            //Console.WriteLine($"Image saved to: {imagePath}");
        }

        public void Dispose()
        {
            inferenceSession?.Dispose();
        }

    }
}
