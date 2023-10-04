namespace InklingAi.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public class FloatUtils
    {
        public static double[] ToDoubles(float[] data) { return Array.ConvertAll(data, x => (double)x); }
        public static float[] ToFloats(double[] data) { return Array.ConvertAll(data, x => (float)x); }

        public static float[] Add(float[] from, float[] by) => from.Zip(by, (a, b) => a + b).ToArray();
        public static float[] Subtract(float[] from, float[] by) => from.Zip(by, (a, b) => a - b).ToArray();
        public static float[] Multiply(float[] from, float[] by) => from.Zip(by, (a, b) => a * b).ToArray();
        public static float[] Divide(float[] from, float[] by) => from.Zip(by, (a, b) => b != 0 ? a / b : float.MaxValue).ToArray();

        public static float[] Add(float[] from, float by) => from.Select(a => a + by).ToArray();
        public static float[] Subtract(float[] from, float by) => from.Select(a => a - by).ToArray();
        public static float[] Multiply(float[] from, float by) => from.Select(a => a * by).ToArray();
        public static float[] Divide(float[] from, float by) => from.Select(a => by != 0 ? a / by : float.MaxValue).ToArray();

        public static void AddTo(float[] result, float[] toAdd)
        {
            var min = Math.Min(result.Length, toAdd.Length);
            for (int i = 0; i < min; i++)
            {
                result[i] += toAdd[i];
            }
        }
        public static void SubtractFrom(float[] result, float[] toSubtract)
        {
            var min = Math.Min(result.Length, toSubtract.Length);
            for (int i = 0; i < min; i++)
            {
                result[i] -= toSubtract[i];
            }
        }
        public static void MultiplyBy(float[] result, float[] toMult)
        {
            var min = Math.Min(result.Length, toMult.Length);
            for (int i = 0; i < min; i++)
            {
                result[i] *= toMult[i];
            }
        }
        public static void DivideBy(float[] result, float[] divisor)
        {
            var min = Math.Min(result.Length, divisor.Length);
            for (int i = 0; i < min; i++)
            {
                result[i] = divisor[i] != 0 ? result[i] / divisor[i] : float.MaxValue;
            }
        }
        public static void AddTo(float[] result, float toAdd)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] += toAdd;
            }
        }
        public static void SubtractFrom(float[] result, float toSubtract)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] -= toSubtract;
            }
        }
        public static void MultiplyBy(float[] result, float toMult)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] *= toMult;
            }
        }
        public static void DivideBy(float[] result, float divisor)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = divisor != 0 ? result[i] / divisor : float.MaxValue;
            }
        }

        public static float[][] BytesToJaggedFloats(byte[] data, int sentenceCount, int hiddenLayers)
        {
            var tensor = new float[sentenceCount][];
            var bytesPerLayer = sizeof(float) * hiddenLayers;
            try
            {
                for (int sc = 0; sc < sentenceCount; sc++)
                {
                    var start = sc * bytesPerLayer;
                    var row = new float[hiddenLayers];
                    var len = (int)Math.Min(data.Length - start, bytesPerLayer);
                    GCHandle handle = GCHandle.Alloc(row, GCHandleType.Pinned);
                    try { Marshal.Copy(data, start, handle.AddrOfPinnedObject(), len); } finally { handle.Free(); }
                    tensor[sc] = row;
                }
            }
            catch (Exception ex) { Trace.WriteLine(ex); }

            return tensor;
        }

        public static float[][][] BytesToJaggedFloats(byte[] data, int sentenceCount, int tokenCount, int hiddenLayers)
        {
            var tensor = new float[sentenceCount][][];
            for (int sc = 0; sc < sentenceCount; sc++)
            {
                tensor[sc] = new float[tokenCount][];
                for (int tc = 0; tc < tokenCount; tc++)
                {
                    var start = sc * sentenceCount + tc * tokenCount;
                    var row = new float[hiddenLayers];
                    GCHandle handle = GCHandle.Alloc(row, GCHandleType.Pinned);
                    try { Marshal.Copy(data, start, handle.AddrOfPinnedObject(), hiddenLayers); } finally { handle.Free(); }
                    tensor[sc][tc] = row;
                }
            }
            return tensor;
        }
        public static float[,,] BytesToNestedFloats(byte[] data, int sentenceCount, int tokenCount, int vectorLength)
        {
            var tensor = new float[sentenceCount, tokenCount, vectorLength];
            GCHandle handle = GCHandle.Alloc(tensor, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(data, 0, handle.AddrOfPinnedObject(), data.Length);
            }
            finally
            {
                handle.Free();
            }
            return tensor;
        }

        public static double DotProduct(float[] A, float[] B)
        {
            double dotProduct = 0.0;
            for (int i = 0; i < A.Length; ++i)
            {
                dotProduct += A[i] * B[i];
            }
            return dotProduct;
        }
        public static double CosineSimilarity(float[] A, float[] B)
        {
            double dotProduct = 0.0, denominatorA = 0.0, denominatorB = 0.0;
            for (int i = 0; i < A.Length; ++i)
            {
                dotProduct += A[i] * B[i];
                denominatorA += A[i] * A[i];
                denominatorB += B[i] * B[i];
            }
            return dotProduct / (Math.Sqrt(denominatorA) * Math.Sqrt(denominatorB));
        }

        public static Complex[] ComplexDiff(float[] a, float[] b)
        {
            var min = Math.Min(a.Length, b.Length);
            var result = new Complex[min];
            for (int i = 0; i < min; i++)
            {
                result[i] = new Complex(-a[i], b[i]);
            }
            return result;
        }

        public static int GetLargestDivisor(int n, int min = 2)
        {
            int result = (int)Math.Sqrt(n);
            while (result > min && (n % result) != 0) result--;
            return result;
        }
    }
}
