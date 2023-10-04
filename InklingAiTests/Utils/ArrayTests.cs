namespace InklingAiTests.Utils;

using Xunit;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;
using InklingAi.Utils;

public class FloatArrayExtensionsTests
{
    [Fact]
    public void ToDoubles_ConvertsCorrectly()
    {
        float[] data = { 1.0f, 2.0f, 3.0f };
        double[] result = data.ToDoubles();
        Assert.Equal(new double[] { 1.0, 2.0, 3.0 }, result);
    }

    [Fact]
    public void AverageWith_CalculatesCorrectly()
    {
        float[] left = { 1.0f, 2.0f, 3.0f };
        float[] right = { 3.0f, 4.0f, 5.0f };
        float[] result = left.AverageWith(right);
        Assert.Equal(new float[] { 2.0f, 3.0f, 4.0f }, result);
    }

    [Fact]
    public void Add_CalculatesCorrectly()
    {
        float[] left = { 1.0f, 2.0f, 3.0f };
        float[] right = { 3.0f, 4.0f, 5.0f };
        float[] result = left.Add(right);
        Assert.Equal(new float[] { 4.0f, 6.0f, 8.0f }, result);
    }

    [Fact]
    public void Subtract_CalculatesCorrectly()
    {
        float[] left = { 1.0f, 2.0f, 3.0f };
        float[] right = { 3.0f, 4.0f, 5.0f };
        float[] result = left.Subtract(right);
        Assert.Equal(new float[] { -2.0f, -2.0f, -2.0f }, result);
    }

    [Fact]
    public void Multiply_CalculatesCorrectly()
    {
        float[] left = { 1.0f, 2.0f, 3.0f };
        float[] right = { 3.0f, 4.0f, 5.0f };
        float[] result = left.Multiply(right);
        Assert.Equal(new float[] { 3.0f, 8.0f, 15.0f }, result);
    }

    [Fact]
    public void Divide_CalculatesCorrectly()
    {
        float[] left = { 1.0f, 2.0f, 3.0f };
        float[] right = { 3.0f, 4.0f, 5.0f };
        float[] result = left.Divide(right);
        Assert.Equal(new float[] { 1.0f / 3.0f, 0.5f, 3.0f / 5.0f }, result);
    }

    [Fact]
    public void SameSignRatio_CalculatesCorrectly()
    {
        float[] values = { -1.0f, -2.0f, 3.0f, 4.0f };
        float result = values.SameSignRatio();
        Assert.Equal(0.5f, result);
    }

    [Fact]
    public void DisplayString_FormatsCorrectly()
    {
        float[] values = { 1.0f, 2.0f, 3.0f };
        string result = values.DisplayString();
        Assert.Equal("1.00, 2.00, 3.00", result);
    }
    [Fact]
    public void DotProduct_CalculatesCorrectly()
    {
        float[] left = { 1.0f, 2.0f, 3.0f };
        float[] right = { 4.0f, 5.0f, 6.0f };
        double result = left.DotProduct(right);
        Assert.Equal(32.0, result);
    }

    [Fact]
    public void CosineSimilarity_CalculatesCorrectly()
    {
        float[] left = { 1.0f, 2.0f, 3.0f };
        float[] right = { 4.0f, 5.0f, 6.0f };
        double result = left.CosineSimilarity(right);
        Assert.Equal(0.974631846197076, result, 9);
    }

    [Fact]
    public void SelectNonZero_SelectsCorrectly()
    {
        float[] values = { 0.0f, -1.0f, 2.0f, -3.0f };
        Dictionary<int, float> result = values.SelectNonZero();
        Assert.Equal(new Dictionary<int, float> { { 1, -1.0f }, { 3, -3.0f } }, result);
    }

    [Fact]
    public void Variance_CalculatesCorrectly()
    {
        float[] values = { 1.0f, 2.0f, 3.0f, 4.0f };
        double result = values.Variance();
        Assert.Equal(1.666, result, 2);
    }

    [Fact]
    public void VarianceWithMeanAndUnbiasedFlag_CalculatesCorrectly()
    {
        float[] values = { 1.0f, 2.0f, 3.0f, 4.0f };
        float result = values.Variance(2.5f, true);
        Assert.Equal(1.666f, result, 2);
    }

    [Fact]
    public void VarianceWithMean_CalculatesCorrectly()
    {
        float[] values = { 1.0f, 2.0f, 3.0f, 4.0f };
        float result = values.Variance(2.5f);
        Assert.Equal(1.666f, result, 2);
    }

    [Fact]
    public void StandardDeviation_CalculatesCorrectly()
    {
        float[] values = { 1.0f, 2.0f, 3.0f, 4.0f };
        double result = values.StandardDeviation();
        Assert.Equal(1.29, result, 2);
    }

    [Fact]
    public void StandardDeviationWithMean_CalculatesCorrectly()
    {
        float[] values = { 1.0f, 2.0f, 3.0f, 4.0f };
        double result = values.StandardDeviation(2.5f);
        Assert.Equal(1.29, result, 2);
    }


    // floats per element:

    private readonly float[][] _testArray = new float[][]
    {
            new float[] { 1f, 2f, 3f },
            new float[] { 4f, 5f, 6f },
            new float[] { 7f, 8f, 9f }
    };

    [Fact]
    public void PerVectorAverages_CalculatesCorrectly()
    {
        var result = _testArray.PerVectorAverages();
        Assert.Equal(new float[] { 2f, 5f, 8f }, result);
    }

    [Fact]
    public void PerVectorSums_CalculatesCorrectly()
    {
        var result = _testArray.PerVectorSums();
        Assert.Equal(new float[] { 6f, 15f, 24f }, result);
    }

    [Fact]
    public void PerVectorMaxes_CalculatesCorrectly()
    {
        var result = _testArray.PerVectorMaxes();
        Assert.Equal(new float[] { 3f, 6f, 9f }, result);
    }

    [Fact]
    public void PerVectorMins_CalculatesCorrectly()
    {
        var result = _testArray.PerVectorMins();
        Assert.Equal(new float[] { 1f, 4f, 7f }, result);
    }

    [Fact]
    public void PerColumnAverages_CalculatesCorrectly()
    {
        var result = _testArray.PerColumnAverages();
        Assert.Equal(new float[] { 4f, 5f, 6f }, result);
    }

    [Fact]
    public void PerColumnSums_CalculatesCorrectly()
    {
        var result = _testArray.PerColumnSums();
        Assert.Equal(new float[] { 12f, 15f, 18f }, result);
    }

    [Fact]
    public void PerColumnMaxes_CalculatesCorrectly()
    {
        var result = _testArray.PerColumnMaxes();
        Assert.Equal(new float[] { 7f, 8f, 9f }, result);
    }

    [Fact]
    public void PerColumnMins_CalculatesCorrectly()
    {
        var result = _testArray.PerColumnMins();
        Assert.Equal(new float[] { 1f, 2f, 3f }, result);
    }

    [Fact]
    public void SameSignRatioPer_CalculatesCorrectly()
    {
        var result = _testArray.SameSignRatio();
        Assert.All(result, r => Assert.Equal(1f, r));
    }

    [Fact]
    public void TransposeIterator_TransposesCorrectly()
    {
        var result = _testArray.TransposeIterator().ToArray();
        Assert.Equal(3, result.Length);
        Assert.Equal(new float[] { 1f, 4f, 7f }, result[0]);
        Assert.Equal(new float[] { 2f, 5f, 8f }, result[1]);
        Assert.Equal(new float[] { 3f, 6f, 9f }, result[2]);
    }

    [Fact]
    public void Transpose_TransposesCorrectly()
    {
        var result = _testArray.Transpose();
        Assert.Equal(3, result.Length);
        Assert.Equal(new float[] { 1f, 4f, 7f }, result[0]);
        Assert.Equal(new float[] { 2f, 5f, 8f }, result[1]);
        Assert.Equal(new float[] { 3f, 6f, 9f }, result[2]);
    }
}
