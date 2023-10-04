namespace InklingAi.Onnx;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Float16Extensions
{

    public static float ToFloat(this Float16 value)
    {
        return value.ToHalf().ToFloat();
    }
    public static float[] ToFloatArray(this Float16[] values)
    {
        float[] result = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i].ToFloat();
        }
        return result;
    }
    public static Half ToHalf(this Float16 float16)
    {
        return BitConverter.ToHalf(BitConverter.GetBytes(float16.value), 0);
    }
    public static Half[] ToHalfArray(this Float16[] values)
    {
        Half[] result = new Half[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i].ToHalf();
        }
        return result;
    }
    public static DenseTensor<float> ToFloatTensor(this Tensor<Float16> tensor)
    {
        var floats = tensor.ToArray().ToFloatArray();
        return OnnxUtils.CreateTensor(floats, tensor.Dimensions.ToArray());
    }
}

public static class FloatExtensions
{
    public static Float16 ToFloat16(this float value)
    {
        return value.ToHalf().ToFloat16();
    }
    public static Float16[] ToFloat16Array(this float[] values)
    {
        Float16[] result = new Float16[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i].ToFloat16();
        }
        return result;
    }
    public static DenseTensor<Float16> ToFloat16Tensor(this Tensor<float> tensor)
    {
        var float16s = tensor.ToArray().ToFloat16Array();
        return OnnxUtils.CreateTensor(float16s, tensor.Dimensions.ToArray());
    }
    public static Half ToHalf(this float value)
    {
        return (Half)value;
    }
    public static Half[] ToHalfArray(this float[] values)
    {
        Half[] result = new Half[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i].ToHalf();
        }
        return result;
    }
}

public static class HalfExtensions
{
    public static float ToFloat(this Half value)
    {
        return (float)value;
    }
    public static float[] ToFloatArray(this Half[] values)
    {
        float[] result = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i].ToFloat();
        }
        return result;
    }
    public static Float16 ToFloat16(this Half value)
    {
        var bytes = BitConverter.ToUInt16(BitConverter.GetBytes(value), 0);
        return new Float16(bytes);
    }
    public static Float16[] ToFloat16Array(this Half[] values)
    {
        Float16[] result = new Float16[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i].ToFloat16();
        }
        return result;
    }
}

public static class ByteConversionExtensions
{
    public static Float16[] ToFloat16Array(this byte[] value)
    {
        var len = (value.Length / 2);
        var result = new Float16[len];
        for (int i = 0; i < len; i++)
        {
            ushort bval = BitConverter.ToUInt16(value, i * 2);
            result[i] = new Float16(bval);
        }
        return result;
    }
}


public static class TensorExtensions
{
    // tensor math
    public static DenseTensor<Float16> Duplicate(this DenseTensor<Float16> tensor)
    {
        var dimensions = tensor.Dimensions.ToArray();
        var data = tensor.ToArray<Float16>();
        data = data.Concat(data).ToArray();
        dimensions[0] *= 2;
        return OnnxUtils.CreateTensor(data, dimensions);
    }
    public static DenseTensor<Float16> DivideTensorByFloat(float[] data, float value, int[] dimensions)
    {
        Float16[] result = new Float16[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (data[i] / value).ToFloat16();
        }
        return OnnxUtils.CreateTensor(result, dimensions);
    }
    public static Tuple<Tensor<Float16>, Tensor<Float16>> SplitTensorOnZeroDim(this Tensor<Float16> inputTensor)
    {
        if (inputTensor.Dimensions[0] % 2 != 0)
        {
            throw new ArgumentException("Expected even first dimension.");
        }
        var dims = inputTensor.Dimensions.ToArray();
        dims[0] /= 2;

        var flattened = inputTensor.ToArray<Float16>();
        var halfLength = flattened.Length / 2;

        var firstHalf = new Float16[halfLength];
        var secondHalf = new Float16[halfLength];
        Array.Copy(flattened, 0, firstHalf, 0, halfLength);
        Array.Copy(flattened, halfLength, secondHalf, 0, halfLength);
        var tensor1 = new DenseTensor<Float16>(firstHalf, dims);
        var tensor2 = new DenseTensor<Float16>(secondHalf, dims);

        return new Tuple<Tensor<Float16>, Tensor<Float16>>(tensor1, tensor2);

    }
}
