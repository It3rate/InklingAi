namespace InklingAiTests.Onnx
{
    using System;
    using System.Linq;
    using Xunit;
    using Microsoft.ML.OnnxRuntime.Tensors;
    using Microsoft.ML.OnnxRuntime;
    using InklingAi.Onnx;

    namespace YourNamespace.Tests
    {
        public class Float16ExtensionsTests
        {
            [Fact]
            public void ToFloat_ConvertsCorrectly()
            {
                Float16 testValue = new Float16(15360); // Example value
                float result = testValue.ToFloat();
                Assert.Equal(1.0f, result, 1); // Assuming 1.0f is the expected value, 1 is precision
            }

            [Fact]
            public void ToFloatArray_ConvertsCorrectly()
            {
                Float16[] testValues = new Float16[]
                {
                new Float16(15360), // Example values
                new Float16(16896)
                };
                float[] result = testValues.ToFloatArray();
                float[] expected = { 1.0f, 3.0f }; // Example expected values
                Assert.Equal(expected, result);
            }

            [Fact]
            public void ToHalf_ConvertsCorrectly()
            {
                Float16 testValue = new Float16(15360); // Example value
                Half result = testValue.ToHalf();
                Half expected = (Half)1.0f;
                Assert.Equal(expected, result);
            }

            [Fact]
            public void ToHalfArray_ConvertsCorrectly()
            {
                Float16[] testValues = new Float16[]
                {
                new Float16(15360), // Example values
                new Float16(16896)
                };
                Half[] result = testValues.ToHalfArray();
                Half[] expected =
                {
                    (Half)1.0f,
                    (Half)3.0f
            };
                Assert.Equal(expected, result);
            }

            [Fact]
            public void ToFloatTensor_ConvertsCorrectly()
            {
                byte[] testData = { 0x00, 0x3C, 0x00, 0x3D, 0x00, 0x3E, 0x00, 0x3F }; 
                int[] dimensions = { 2, 2 }; 
                DenseTensor<Float16> testTensor = OnnxUtils.CreateFloat16TensorFromBytes(testData, dimensions);

                DenseTensor<float> result = testTensor.ToFloatTensor();

                Assert.Equal(dimensions, result.Dimensions.ToArray());
                var f0 = new Float16(0x3c00).ToFloat();
                Assert.Equal(1.0f, f0);
                var f1 = new Float16(0x3d00).ToFloat();
                Assert.Equal(1.25f, f1);
                var f2 = new Float16(0x3e00).ToFloat();
                Assert.Equal(1.5f, f2);
                var f3 = new Float16(0x3f00).ToFloat();
                Assert.Equal(1.75f, f3);
                Assert.Equal(new float[] {f0, f1, f2, f3}, result.ToArray()); 
            }

        }
    }

}
