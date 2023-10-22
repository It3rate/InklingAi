namespace InklingAiTests.Onnx
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
    using ExCSS;
    using static System.Runtime.InteropServices.JavaScript.JSType;
    using ManagedCuda;
    using NumSharp.Backends.Unmanaged;
    using InklingAi.Utils;

    [Collection("OrtMemoryTests")]
    public class OnnxRuntimeTests
    {
        private const string ModelPath = "path_to_your_model.onnx";

        //[Fact(DisplayName = "TestRunOnnxModelWithGpu")]
        public unsafe void TestRunOnnxModelWithGpu()
        {
            OrtValue? inTensor = null;
            OrtValue? outTensor = null;
            try
            {
                var inShape = new long[] { 1, 4, 64, 64 };
                CudaDeviceVariable<Float16> deviceInVar;
                (inTensor, deviceInVar) = CreateGPUFloat16Values(inShape);

                var outShape = new long[] { 512, 512, 4 };
                CudaDeviceVariable<byte> deviceVar;
                (outTensor, deviceVar) = CreateGPUByteValues(outShape);


                Assert.NotNull(inTensor);
                var pInResult = new Float16[4 * 64 * 64];
                deviceInVar.CopyToHost(pInResult);
                Assert.Equal(1.5f.ToFloat16(), pInResult[3]);

                Assert.NotNull(outTensor);
                var pOutResult = new byte[512 * 512 * 4];
                deviceVar.CopyToHost(pOutResult);
                Assert.Equal((byte)3, pOutResult[3]);

                // test data
                //var originalData = Enumerable.Range(0, 4*64*64).Select(i => ((i + 0.5f).ToFloat16())).ToArray();
                //Assert.Equal(originalData.ToArray(), inTensor.GetTensorDataAsSpan<Float16>().ToArray());

                string modelPath = IOUtils.SDOnnxFolder + @"vae_decoder\model.onnx";
                byte[] modelData = File.ReadAllBytes(modelPath);
                CudaDeviceVariable<byte> deviceModel = new CudaDeviceVariable<byte>(modelData.Length);
                deviceModel.CopyToDevice(modelData);


                var sessionOptions = new SessionOptions();
                sessionOptions.EnableMemoryPattern = false;
                sessionOptions.ExecutionMode = ExecutionMode.ORT_PARALLEL;
                sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                var opt = new OrtCUDAProviderOptions();
                var dict = new Dictionary<string, string>() { { "cudnn_conv_algo_search", "DEFAULT" } };
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
                var result = new byte[512 * 512 * 4];
                deviceVar.CopyToHost(result);
                //var outData = outputValues[0].Value.GetTensorDataAsSpan<byte>().ToArray();
                Assert.True(result.Length == 512 * 512 * 4);

            }
            finally
            {
                if (inTensor != null) inTensor.Dispose();
                if (outTensor != null) outTensor.Dispose();
            }
        }

        static unsafe (OrtValue?, CudaDeviceVariable<Float16>) CreateGPUFloat16Values(long[] shape)
        {
            int length = (int)shape.Aggregate(1L, (acc, x) => acc * x);
            int size = sizeof(Float16);
            var bufferLen = length * size;

            CudaContext ctx = new CudaContext();
            CudaDeviceVariable<Float16> deviceVar = new CudaDeviceVariable<Float16>(bufferLen);

            Float16[] hostArray = new Float16[length];
            for (int i = 0; i < length; i++)
            {
                hostArray[i] = (i * 0.5f).ToFloat16(); // Example values
            }
            deviceVar.CopyToDevice(hostArray);

            var tensor = OrtValue.CreateTensorValueWithData(
                OrtMemoryInfo.DefaultInstance,
                TensorElementType.Float16,
                shape,
                deviceVar.DevicePointer.Pointer,
                bufferLen);
            return (tensor, deviceVar);
        }

        static unsafe (OrtValue?, CudaDeviceVariable<byte>) CreateGPUByteValues(long[] shape)
        {
            int length = (int)shape.Aggregate(1L, (acc, x) => acc * x);
            int size = sizeof(byte);
            var bufferLen = length * size;

            CudaContext ctx = new CudaContext();
            CudaDeviceVariable<byte> deviceVar = new CudaDeviceVariable<byte>(bufferLen);

            byte[] hostArray = new byte[length];
            for (int i = 0; i < length; i++)
            {
                hostArray[i] = (byte)(i % 255);
            }
            deviceVar.CopyToDevice(hostArray);

            var tensor = OrtValue.CreateTensorValueWithData(
                OrtMemoryInfo.DefaultInstance,
                TensorElementType.Int8,
                shape,
                deviceVar.DevicePointer.Pointer,
                bufferLen);
            return (tensor, deviceVar);
        }

        static unsafe OrtValue? CreateCPUValue(Type elementType, long[] shape, Func<int, object> initializer)
        {
            OrtValue result = null;
            var tType = TensorBase.GetTypeInfo(elementType).ElementType;
            int length = (int)shape.Aggregate(1L, (acc, x) => acc * x);
            int sizeOfT = Marshal.SizeOf(elementType);
            var bufferLen = length * sizeOfT;
            var dataPtr = Marshal.AllocHGlobal(bufferLen);

            try
            {
                for (int i = 0; i < length; i++)
                {
                    object value = initializer(i);
                    IntPtr dest = IntPtr.Add(dataPtr, i * sizeOfT);
                    Marshal.StructureToPtr(value, dest, false);
                }
                var tensor = OrtValue.CreateTensorValueWithData(
                    OrtMemoryInfo.DefaultInstance,
                    tType,
                    shape,
                    dataPtr,
                    bufferLen);

                result = tensor;
            }
            catch
            {
                // Ensure memory is freed in case of an exception
                Marshal.FreeHGlobal(dataPtr);
                throw;
            }
            finally
            {
                VerifyTensorCreateWithData(result, tType, shape, elementType);
            }
            return result;
        }

        static void VerifyTensorCreateWithData(OrtValue? tensor, TensorElementType dataType, long[] shape, Type t)
        {
            Assert.NotNull(tensor);
            // Verify invocation
            var dataTypeInfo = TensorBase.GetTypeInfo(t);
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

        }
    }
}