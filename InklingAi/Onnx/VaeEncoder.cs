namespace InklingAi.Onnx;

using Avalonia.Media;
using InklingAi.Utils;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class VaeEncoder
{
    public InferenceSession inferenceSession;
    public int DeviceId = 0;

    //public VaeEncoder(string modelPath = @"F:\tmp\vae\sd_ink\vae_encoder\model.onnx")
    //public VaeEncoder(string modelPath = @"F:\tmp\vae\sd_ink\opt\vae_encoder\model.onnx")
    public VaeEncoder(string modelPath = @"F:\tmp\vae\sd_ink\clean\vae_encoder\model.onnx")
    {
        inferenceSession = OnnxUtils.GetCudaSession(modelPath); // ,@"F:\tmp\vae\sd_ink\clean\vae_encoder\model.onnx");
    }

    unsafe public DenseTensor<Float16> EncodeImage(SKBitmap sourceBmp)
    {
        // might need to resize
        var tensor = InklingIOUtils.GetTensorFromImage(sourceBmp);
        var result = Encode(tensor);
        return result;
    }
    unsafe public DenseTensor<Float16> Encode(DenseTensor<byte> input)
    {
        List<NamedOnnxValue> namedOnnxValues = new List<NamedOnnxValue>();
        namedOnnxValues.Add(NamedOnnxValue.CreateFromTensor("sample", input));
        var output = inferenceSession.Run(namedOnnxValues);
        var result = output.ToList().First().Value as DenseTensor<Float16>;
        return result!;
    }


}
