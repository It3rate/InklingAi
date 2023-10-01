namespace InklingAi.Onnx;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class OnnxUtils
{
    public static Random random = new Random();

    public static InferenceSession GetCudaSession(string modelPath, string? optPath = null)
    {
        var deviceId = 0;
        var sessionOptions = new SessionOptions();
        sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        sessionOptions.AppendExecutionProvider_CUDA(deviceId);
        sessionOptions.AppendExecutionProvider_CPU();
        //sessionOptions.EnableProfiling = true;
        sessionOptions.EnableMemoryPattern = true;
        sessionOptions.EnableCpuMemArena = true;
        sessionOptions.ExecutionMode = ExecutionMode.ORT_PARALLEL;
        sessionOptions.RegisterOrtExtensions();
        if(optPath != null)
        {
            sessionOptions.OptimizedModelFilePath = optPath;
        }
        var result = new InferenceSession(modelPath, sessionOptions);
        return result;
    }

    public static DenseTensor<T> CreateTensor<T>(T[] data, int[] dimensions)
    {
        return new DenseTensor<T>(data, dimensions);
    }
    public static DenseTensor<Float16> CreateFloat16TensorFromBytes(byte[] data, int[] dimensions)
    {
        Float16[] f16Array = data.ToFloat16Array();
        return new DenseTensor<Float16>(f16Array, dimensions);
    }


    public static void MultipleTensorByFloat(float[] data, float value)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] *= value;
        }
    }
    public static void DivideTensorByFloat(float[] data, float value)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] /= value;
        }
    }
    public static void AddTensors(float[] sample, float[] toAdd)
    {
        for (var i = 0; i < sample.Length; i++)
        {
            sample[i] += toAdd[i];
        }
    }
    public static void SubtractTensors(float[] sample, float[] toSubtract)
    {
        for (var i = 0; i < sample.Length; i++)
        {
            sample[i] -= toSubtract[i];
        }
    }

    public static DenseTensor<float> MultipleTensorByFloat(Tensor<float> data, float value)
    {
        var floats = data.ToArray();
        MultipleTensorByFloat(floats, value);
        return CreateTensor(floats, data.Dimensions.ToArray());
    }
    public static DenseTensor<float> DivideTensorByFloat(Tensor<float> data, float value)
    {
        var floats = data.ToArray();
        DivideTensorByFloat(floats, value);
        return CreateTensor(floats, data.Dimensions.ToArray());
    }
    public static DenseTensor<float> AddTensors(Tensor<float> sample, Tensor<float> toAdd)
    {
        var sampleFloats = sample.ToArray();
        var toAddFloats = toAdd.ToArray();
        AddTensors(sampleFloats, toAddFloats);
        return CreateTensor(sampleFloats, sample.Dimensions.ToArray());
    }
    public static DenseTensor<float> SubtractTensors(Tensor<float> sample, Tensor<float> toSubtract)
    {
        var sampleFloats = sample.ToArray();
        var toSubtractFloats = toSubtract.ToArray();
        SubtractTensors(sampleFloats, toSubtractFloats);
        return CreateTensor(sampleFloats, sample.Dimensions.ToArray());
    }

    public static DenseTensor<Float16> MultipleTensorByFloat(Tensor<Float16> data, float value)
    {
        var floats = data.ToFloatTensor().ToArray();
        MultipleTensorByFloat(floats, value);
        return CreateTensor(floats, data.Dimensions.ToArray()).ToFloat16Tensor();
    }
    public static DenseTensor<Float16> DivideTensorByFloat(Tensor<Float16> data, float value)
    {
        var floats = data.ToFloatTensor().ToArray();
        DivideTensorByFloat(floats, value);
        return CreateTensor(floats, data.Dimensions.ToArray()).ToFloat16Tensor();
    }
    public static DenseTensor<Float16> AddTensors(Tensor<Float16> data, Tensor<Float16> toAdd)
    {
        var floats = data.ToFloatTensor().ToArray();
        var toAddFloats = data.ToFloatTensor().ToArray();
        AddTensors(floats, toAddFloats);
        return CreateTensor(floats, data.Dimensions.ToArray()).ToFloat16Tensor();
    }
    public static DenseTensor<Float16> SubtractTensors(Tensor<Float16> data, Tensor<Float16> toSubtract)
    {
        var floats = data.ToFloatTensor().ToArray();
        var toSubtractFloats = data.ToFloatTensor().ToArray();
        SubtractTensors(floats, toSubtractFloats);
        return CreateTensor(floats, data.Dimensions.ToArray()).ToFloat16Tensor();
    }


    public static DenseTensor<float> Duplicate(float[] data, int[] dimensions)
    {
        data = data.Concat(data).ToArray();
        return CreateTensor(data, dimensions);
    }

    public static DenseTensor<float> SumTensors(Tensor<float>[] tensorArray, int[] dimensions)
    {
        var sumTensor = new DenseTensor<float>(dimensions);
        var sumArray = new float[sumTensor.Length];

        for (int m = 0; m < tensorArray.Count(); m++)
        {
            var tensorToSum = tensorArray[m].ToArray();
            for (var i = 0; i < tensorToSum.Length; i++)
            {
                sumArray[i] += (float)tensorToSum[i];
            }
        }

        return CreateTensor(sumArray, dimensions);
    }

    public static Tensor<float> GetRandomTensor(ReadOnlySpan<int> dimensions)
    {
        var latents = new DenseTensor<float>(dimensions);
        var latentsArray = latents.ToArray();

        for (int i = 0; i < latentsArray.Length; i++)
        {
            // Generate a random number from a normal distribution with mean 0 and variance 1
            var u1 = random.NextDouble(); // Uniform(0,1) random number
            var u2 = random.NextDouble(); // Uniform(0,1) random number
            var radius = Math.Sqrt(-2.0 * Math.Log(u1)); // Radius of polar coordinates
            var theta = 2.0 * Math.PI * u2; // Angle of polar coordinates
            var standardNormalRand = radius * Math.Cos(theta); // Standard normal random number
            latentsArray[i] = (float)standardNormalRand;
        }

        latents = OnnxUtils.CreateTensor(latentsArray, latents.Dimensions.ToArray());

        return latents;

    }
}
