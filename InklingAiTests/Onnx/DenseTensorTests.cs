using InklingAi.Onnx;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Microsoft.ML.OnnxRuntime.Tensors;
    using InklingAi.Onnx;
    using Microsoft.ML.OnnxRuntime;

public class TensorOperationsTests
{
    [Fact]
    public void AddTensors_AddsCorrectly()
    {
        var sample = new DenseTensor<float>(new float[] { 1, 2, 3, 4 }, new int[] { 2, 2 });
        var toAdd = new DenseTensor<float>(new float[] { 4, 3, 2, 1 }, new int[] { 2, 2 });
        var result = OnnxUtils.AddTensors(sample, toAdd);
        Assert.Equal(new float[] { 5, 5, 5, 5 }, result.ToArray());
    }

    [Fact]
    public void SubtractTensors_SubtractsCorrectly()
    {
        var sample = new DenseTensor<float>(new float[] { 5, 5, 5, 5 }, new int[] { 2, 2 });
        var toSubtract = new DenseTensor<float>(new float[] { 4, 3, 2, 1 }, new int[] { 2, 2 });
        var result = OnnxUtils.SubtractTensors(sample, toSubtract);
        Assert.Equal(new float[] { 1, 2, 3, 4 }, result.ToArray());
    }
    [Fact]
    public void MultipleTensorByFloat_MultipliesCorrectly()
    {
        var data = new DenseTensor<float>(new float[] { 1, 2, 3, 4 }, new int[] { 2, 2 });
        float value = 2f;
        var result = OnnxUtils.MultipleTensorByFloat(data, value);
        Assert.Equal(new float[] { 2, 4, 6, 8 }, result.ToArray());
    }

    [Fact]
    public void DivideTensorByFloat_DividesCorrectly()
    {
        var data = new DenseTensor<float>(new float[] { 2, 4, 6, 8 }, new int[] { 2, 2 });
        float value = 2f;
        var result = OnnxUtils.DivideTensorByFloat(data, value);
        Assert.Equal(new float[] { 1, 2, 3, 4 }, result.ToArray());
    }

    [Fact]
    public void AddF16Tensors_AddsCorrectly()
    {
        var data = new DenseTensor<Float16>(OnnxUtils.Float16Array(1f, 2f, 3f, 4f), new int[] { 2, 2 });
        var toAdd = new DenseTensor<Float16>(OnnxUtils.Float16Array(4f, 3f, 2f, 1f), new int[] { 2, 2 });
        var result = OnnxUtils.AddTensors(data, toAdd);
        Assert.Equal(OnnxUtils.Float16Array(5f, 5f, 5f, 5f), result.ToArray());
    }

    [Fact]
    public void SubtractF16Tensors_SubtractsCorrectly()
    {
        var data = new DenseTensor<Float16>(OnnxUtils.Float16Array(5f, 5f, 5f, 5f), new int[] { 2, 2 });
        var toSubtract = new DenseTensor<Float16>(OnnxUtils.Float16Array(4f, 3f, 2f, 1f), new int[] { 2, 2 });
        var result = OnnxUtils.SubtractTensors(data, toSubtract);
        Assert.Equal(OnnxUtils.Float16Array(1f, 2f, 3f, 4f), result.ToArray());
    }
    [Fact]
    public void MultipleF16TensorByFloat_MultipliesCorrectly()
    {
        var data = new DenseTensor<Float16>(OnnxUtils.Float16Array(1f, 2f, 3f, 4f), new int[] { 2, 2 });
        float value = 2f;
        var result = OnnxUtils.MultipleTensorByFloat(data, value);
        Assert.Equal(OnnxUtils.Float16Array(2f, 4f, 6f, 8f), result.ToArray());
    }

    [Fact]
    public void DivideF16TensorByFloat_DividesCorrectly()
    {
        var data = new DenseTensor<Float16>(OnnxUtils.Float16Array(2f, 4f, 6f, 8f), new int[] { 2, 2 });
        float value = 2f;
        var result = OnnxUtils.DivideTensorByFloat(data, value);
        Assert.Equal(OnnxUtils.Float16Array(1f, 2f, 3f, 4f), result.ToArray());
    }

    [Fact]
    public void SumTensors_SumsCorrectly()
    {
        var tensorArray = new Tensor<float>[]
        {
                new DenseTensor<float>(new float[] { 1, 2, 3, 4 }, new int[] { 2, 2 }),
                new DenseTensor<float>(new float[] { 5, 6, 7, 8 }, new int[] { 2, 2 }),
                new DenseTensor<float>(new float[] { 9, 10, 11, 12 }, new int[] { 2, 2 })
        };
        int[] dimensions = { 2, 2 };
        var result = OnnxUtils.SumTensors(tensorArray, dimensions);
        Assert.Equal(new float[] { 15, 18, 21, 24 }, result.ToArray());
    }

    [Fact]
    public void GetRandomTensor_GeneratesTensorWithCorrectDimensions()
    {
        int[] dimensions = { 2, 3 };
        var result = OnnxUtils.GetRandomTensor(dimensions);
        Assert.Equal(dimensions.ToArray(), result.Dimensions.ToArray());
        Assert.NotEqual(result[0], result[1]);
    }
}