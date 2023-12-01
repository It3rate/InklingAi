namespace AIEnv.Onnx;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Unet
{
    public InferenceSession inferenceSessionIn;
    public InferenceSession inferenceSessionMid;
    public InferenceSession inferenceSessionOutA;
    public InferenceSession inferenceSessionOutB;
    public InferenceSession inferenceSessionOutC;
    public int DeviceId = 0;
    SchedulerBase _scheduler;
    string _folder;

    public Unet(SchedulerBase scheduler, string folder = @"F:\tmp\vae\sd_ink\")
    //public Unet(string modelPath = @"F:\tmp\vae\sd_ink\opt\unet\model.onnx")
    //public Unet(string modelPath = @"F:\tmp\vae\sd_ink\clean\unet\model.onnx")
    {
        _scheduler = scheduler;
        _folder = folder;
        SetupModels();
    }
    public void SetupModels()
    {
        inferenceSessionIn = OnnxUtils.GetCudaSession(_folder + @"unet_in\model.onnx");
        inferenceSessionMid = OnnxUtils.GetCudaSession(_folder + @"unet_mid\model.onnx");
        inferenceSessionOutA = OnnxUtils.GetCudaSession(_folder + @"unet_out_a\model.onnx");
        inferenceSessionOutB = OnnxUtils.GetCudaSession(_folder + @"unet_out_b\model.onnx");
        inferenceSessionOutC = OnnxUtils.GetCudaSession(_folder + @"unet_out_c\model.onnx");
    }
    // noise_pred = noise_pred_uncond + self.guidance_scale * (noise_pred_text - noise_pred_uncond)
    private Tensor<Float16> PerformGuidance(Tensor<Float16> noisePredUncond, Tensor<Float16> noisePredText, float guidanceScale)
    {
        var dims = noisePredUncond.Dimensions.ToArray();
        var flatUncond = noisePredUncond.ToFloatTensor().ToArray();
        var flatPred = noisePredText.ToFloatTensor().ToArray();
        var combinedFloats = flatUncond.Zip(flatPred, (uncondPred, textPred) => uncondPred + guidanceScale * (textPred - uncondPred));
        Float16[] combined = combinedFloats.ToArray().ToFloat16Array();
        var result = new DenseTensor<Float16>(combined, dims);

        return result;
    }

    public static List<NamedOnnxValue> CreateStartInput(Tensor<Float16> sample, long timeStep, Tensor<Float16> encoderHiddenStates)
    {
        var input = new List<NamedOnnxValue> {
            NamedOnnxValue.CreateFromTensor("encoder_hidden_states", encoderHiddenStates),
            NamedOnnxValue.CreateFromTensor("sample", sample),
            NamedOnnxValue.CreateFromTensor("timestep", new DenseTensor<long>(new long[] { timeStep }, new int[0]))
        };
        return input;
    }
    public static List<NamedOnnxValue> CreateMidInput(
        Tensor<Float16> sample, Tensor<Float16> emb, Tensor<Float16> encoderHiddenStates)
    {
        var input = new List<NamedOnnxValue> {
            NamedOnnxValue.CreateFromTensor("hidden_states", sample),
            NamedOnnxValue.CreateFromTensor("emb", emb),
            NamedOnnxValue.CreateFromTensor("encoder_hidden_states", encoderHiddenStates),
        };
        return input;
    }
    public static List<NamedOnnxValue> CreateEndInput(
        Tensor<Float16> sample, Tensor<Float16> emb, Tensor<Float16> encoderHiddenStates, List<DisposableNamedOnnxValue> hints)
    {
        var input = new List<NamedOnnxValue> {
            NamedOnnxValue.CreateFromTensor("hidden_states", sample),
            NamedOnnxValue.CreateFromTensor("emb", emb),
            NamedOnnxValue.CreateFromTensor("encoder_hidden_states", encoderHiddenStates),
        };
        for(int i = 0; i < hints.Count; i++)
        {
            var nv = NamedOnnxValue.CreateFromTensor("hint" + i, hints[i].AsTensor<Float16>());
            input.Add(nv);

        }
        return input;
    }

    public DenseTensor<Float16> Step(DenseTensor<Float16> latents, DenseTensor<Float16> textEmbeddings, int timestep, float guidanceScale)
    {
        var latentModelInput = latents.Duplicate();
        latentModelInput = _scheduler.ScaleInput(latentModelInput, timestep);

        // Run Inference
        var input = CreateStartInput(latentModelInput, timestep, textEmbeddings);
        var output = inferenceSessionIn.Run(input).ToList();

        var outputTensor = (output[0].Value as DenseTensor<Float16>);
        var emb = (output[1].Value as DenseTensor<Float16>);
        var hints = output.GetRange(2, 12);

        input = CreateMidInput(outputTensor!, emb!, textEmbeddings);
        output = inferenceSessionMid.Run(input).ToList();
        outputTensor = (output[0].Value as DenseTensor<Float16>);

        input = CreateEndInput(outputTensor!, emb!, textEmbeddings, hints.GetRange(6, 6));
        output = inferenceSessionOutA.Run(input).ToList();
        outputTensor = (output[0].Value as DenseTensor<Float16>);

        input = CreateEndInput(outputTensor!, emb!, textEmbeddings, hints.GetRange(2, 4));
        output = inferenceSessionOutB.Run(input).ToList();
        outputTensor = (output[0].Value as DenseTensor<Float16>);

        input = CreateEndInput(outputTensor!, emb!, textEmbeddings, hints.GetRange(0, 2));
        output = inferenceSessionOutC.Run(input).ToList();
        outputTensor = (output[0].Value as DenseTensor<Float16>);

        // guidance
        var splitTensors = outputTensor!.SplitTensorOnZeroDim();
        var noisePred = splitTensors.Item1;
        var noisePredText = splitTensors.Item2;
        noisePred = PerformGuidance(noisePred, noisePredText, guidanceScale);

        // Scheduler Step
        var result = _scheduler.Step(noisePred, timestep, latents);
        return result;
    }



}
