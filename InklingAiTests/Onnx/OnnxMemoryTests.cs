namespace InklingAi.Tests.OnnxTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ML.OnnxRuntime.Tensors;
    using Microsoft.ML.OnnxRuntime;
    using Xunit;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;
    using InklingAi.Onnx;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    [Collection("OrtMemoryTests")]
    public class OnnxRuntimeTests
    {
        private const string ModelPath = "path_to_your_model.onnx";

        [Fact(DisplayName = "TestRunOnnxModelWithGpu")]
        public unsafe void TestRunOnnxModelWithGpu()
        {
            var inShape = new long[] { 1, 4, 64, 64 };
            int inLength = (int)inShape.Aggregate(1L, (acc, x) => acc * x);
            var inBufferLen = inLength * sizeof(Float16);
            var inDataPtr = Marshal.AllocHGlobal(inBufferLen);

            var outShape = new long[] { 512, 512, 4 };
            int outLength = (int)outShape.Aggregate(1L, (acc, x) => acc * x);
            var outBufferLen = outLength * sizeof(byte);
            var outDataPtr = Marshal.AllocHGlobal(outBufferLen);

            try
            {
                Span<Float16> data;
                data = new Span<Float16>(inDataPtr.ToPointer(), inLength);
                for (int i = 0; i < inLength; i++)
                {
                    data[i] = (0.5f * i).ToFloat16();
                }

                using (var outTensor = OrtValue.CreateTensorValueWithData(
                    OrtMemoryInfo.DefaultInstance, TensorElementType.UInt8, outShape, outDataPtr, outBufferLen))
                using (var inTensor = OrtValue.CreateTensorValueWithData(
                    OrtMemoryInfo.DefaultInstance, TensorElementType.Float16, inShape, inDataPtr, inBufferLen))
                {
                    VerifyTensorCreateWithData<Float16>(inTensor, TensorElementType.Float16, inShape, data);

                    string modelPath = @"F:\tmp\vae\sd_ink\vae_decoder\model.onnx";
                    var sessionOptions = new SessionOptions();
                    sessionOptions.EnableMemoryPattern = false;
                    sessionOptions.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;
                    sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_DISABLE_ALL;
                    var opt = new OrtCUDAProviderOptions();
                    var dict = new Dictionary<string, string>() { {"cudnn_conv_algo_search", "DEFAULT"} };
                    opt.UpdateOptions(dict);
                    sessionOptions.AppendExecutionProvider_CUDA(opt);
                    var session = new InferenceSession(modelPath, sessionOptions);         
                    //var decoder = new VaeDecoder();

                    var inputNames = new List<string> { "latent_sample" }.AsReadOnly();
                    var inputValues = new List<OrtValue> { inTensor }.AsReadOnly();
                    var outputNames = new List<string> { "sample" }.AsReadOnly();
                    var outputValues = new List<OrtValue> { outTensor }.AsReadOnly();

                    var runOptions = new RunOptions();
                    session.Run(runOptions, inputNames, inputValues, outputNames, outputValues);

                    Assert.True(outputValues.Count == 1);
                    var outData = outputValues[0].Value.GetTensorDataAsSpan<byte>().ToArray();
                    VerifyTensorCreateWithData<byte>(outTensor, TensorElementType.UInt8, outShape, outData);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(inDataPtr);
            }
        }

        static void VerifyTensorCreateWithData<T>(OrtValue tensor, TensorElementType dataType, long[] shape,
            ReadOnlySpan<T> originalData) where T : unmanaged
        {
            // Verify invocation
            var dataTypeInfo = TensorBase.GetTypeInfo(typeof(T));
            Assert.NotNull(dataTypeInfo);
            Assert.Equal(dataType, dataTypeInfo.ElementType);

            var elementsNum = ShapeUtils.GetSizeForShape(shape);

            Assert.True(tensor.IsTensor);
            Assert.False(tensor.IsSparseTensor);
            Assert.Equal(OnnxValueType.ONNX_TYPE_TENSOR, tensor.OnnxType);

            var typeInfo = tensor.GetTypeInfo();
            {
                Assert.Equal(OnnxValueType.ONNX_TYPE_TENSOR, typeInfo.OnnxType);
                var typeShape = typeInfo.TensorTypeAndShapeInfo;
                Assert.Equal(shape.Length, typeShape.DimensionsCount);

                var fetchedShape = typeShape.Shape;
                Assert.Equal(shape.Length, fetchedShape.Length);
                Assert.Equal(shape, fetchedShape);
                Assert.Equal(elementsNum, typeShape.ElementCount);
            }

            using (var memInfo = tensor.GetTensorMemoryInfo())
            {
                Assert.Equal("Cpu", memInfo.Name);
                Assert.Equal(OrtMemType.CpuOutput, memInfo.GetMemoryType());
                Assert.Equal(OrtAllocatorType.DeviceAllocator, memInfo.GetAllocatorType());
            }

            // Verify contained data
            Assert.Equal(originalData.ToArray(), tensor.GetTensorDataAsSpan<T>().ToArray());
        }
    }
}