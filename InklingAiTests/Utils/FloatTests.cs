namespace InklingAiTests.Utils;

using InklingAi.Utils;
using System.Numerics;
using Xunit;

    public class FloatTests
    {
        [Fact]
        public void AddTo_Array_AddsCorrectly()
        {
            float[] result = { 1f, 2f, 3f };
            float[] toAdd = { 4f, 5f, 6f };
            FloatUtils.AddTo(result, toAdd);
            Assert.Equal(new float[] { 5f, 7f, 9f }, result);
        }

        [Fact]
        public void SubtractFrom_Array_SubtractsCorrectly()
        {
            float[] result = { 5f, 7f, 9f };
            float[] toSubtract = { 4f, 5f, 6f };
            FloatUtils.SubtractFrom(result, toSubtract);
            Assert.Equal(new float[] { 1f, 2f, 3f }, result);
        }

        [Fact]
        public void MultiplyBy_Array_MultipliesCorrectly()
        {
            float[] result = { 1f, 2f, 3f };
            float[] toMult = { 4f, 5f, 6f };
            FloatUtils.MultiplyBy(result, toMult);
            Assert.Equal(new float[] { 4f, 10f, 18f }, result);
        }

        [Fact]
        public void DivideBy_Array_DividesCorrectly()
        {
            float[] result = { 4f, 10f, 18f };
            float[] divisor = { 4f, 5f, 6f };
            FloatUtils.DivideBy(result, divisor);
            Assert.Equal(new float[] { 1f, 2f, 3f }, result);
        }

        [Fact]
        public void AddTo_Scalar_AddsCorrectly()
        {
            float[] result = { 1f, 2f, 3f };
            FloatUtils.AddTo(result, 4f);
            Assert.Equal(new float[] { 5f, 6f, 7f }, result);
        }

        [Fact]
        public void SubtractFrom_Scalar_SubtractsCorrectly()
        {
            float[] result = { 5f, 6f, 7f };
            FloatUtils.SubtractFrom(result, 4f);
            Assert.Equal(new float[] { 1f, 2f, 3f }, result);
        }

        [Fact]
        public void MultiplyBy_Scalar_MultipliesCorrectly()
        {
            float[] result = { 1f, 2f, 3f };
            FloatUtils.MultiplyBy(result, 4f);
            Assert.Equal(new float[] { 4f, 8f, 12f }, result);
        }

        [Fact]
        public void DivideBy_Scalar_DividesCorrectly()
        {
            float[] result = { 4f, 8f, 12f };
            FloatUtils.DivideBy(result, 4f);
            Assert.Equal(new float[] { 1f, 2f, 3f }, result);
        }


    [Fact]
    public void BytesToJaggedFloats_1D_ConvertsCorrectly()
    {
        byte[] data = { 0, 0, 128, 63, 0, 0, 0, 64 }; // 1.0f and 2.0f in byte format
        var result = FloatUtils.BytesToJaggedFloats(data, 2, 1);
        Assert.Equal(1.0f, result[0][0]);
        Assert.Equal(2.0f, result[1][0]);
    }

    [Fact]
    public void BytesToNestedFloats_ConvertsCorrectly()
    {
        byte[] data = { 0, 0, 128, 63, 0, 0, 0, 64 }; // 1.0f and 2.0f in byte format
        var result = FloatUtils.BytesToNestedFloats(data, 1, 1, 2);
        Assert.Equal(1.0f, result[0, 0, 0]);
        Assert.Equal(2.0f, result[0, 0, 1]);
    }

    [Fact]
    public void DotProduct_CalculatesCorrectly()
    {
        float[] A = { 1.0f, 2.0f };
        float[] B = { 3.0f, 4.0f };
        var result = FloatUtils.DotProduct(A, B);
        Assert.Equal(11.0, result);
    }

    [Fact]
    public void CosineSimilarity_CalculatesCorrectly()
    {
        float[] A = { 1.0f, 2.0f };
        float[] B = { 3.0f, 4.0f };
        var result = FloatUtils.CosineSimilarity(A, B);
        Assert.Equal(0.9838699100999074, result);
    }

    [Fact]
    public void ComplexDiff_CalculatesCorrectly()
    {
        float[] a = { 1.0f, 2.0f };
        float[] b = { 3.0f, 4.0f };
        var result = FloatUtils.ComplexDiff(a, b);
        Assert.Equal(new Complex(-1.0, 3.0), result[0]);
        Assert.Equal(new Complex(-2.0, 4.0), result[1]);
    }

    [Fact]
    public void GetLargestDivisor_FindsCorrectly()
    {
        var result = FloatUtils.GetLargestDivisor(100);
        Assert.Equal(10, result);
    }
}

