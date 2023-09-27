namespace InklingAi.Utils
{
    using InklingAi.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public static class IntArrayExtensions
    {
        public static string DisplayString(this int[] values) => String.Join(", ", values.Select(f => f.ToString()).ToArray());
    }

    public static class FloatArrayExtensions
    {
        public static double[] ToDoubles(this float[] data) { return Array.ConvertAll(data, x => (double)x); }
        public static double[] ToDoubles(this IEnumerable<float> data) => data.Select(x => (double)x).ToArray();

        public static float[] AverageWith(this float[] left, float[] right) => left.Zip(right, (a, b) => (a + b) / 2.0f).ToArray();

        public static float[] Add(this float[] left, float[] right) => left.Zip(right, (a, b) => a + b).ToArray();
        public static float[] Subtract(this float[] left, float[] right) => left.Zip(right, (a, b) => a - b).ToArray();
        public static float[] Multiply(this float[] left, float[] right) => left.Zip(right, (a, b) => a * b).ToArray();
        public static float[] Divide(this float[] left, float[] right) => left.Zip(right, (a, b) => a / b).ToArray();

        public static float[] Add(this float[] left, float right) => left.Select(x => x + right).ToArray();
        public static float[] Subtract(this float[] left, float right) => left.Select(x => x - right).ToArray();
        public static float[] Multiply(this float[] left, float right) => left.Select(x => x * right).ToArray();
        public static float[] Divide(this float[] left, float right) => left.Select(x => x / right).ToArray();

        public static double DotProduct(this float[] left, float[] right) => FloatUtils.DotProduct(left, right);
        public static double CosineSimilarity(this float[] left, float[] right) => FloatUtils.CosineSimilarity(left, right);
        public static float SameSignRatio(this float[] values)
        {
            var neg = 0;
            foreach (var val in values)
            {
                if (val < 0) neg++;
            }
            var max = Math.Max(neg, values.Length - neg);
            max = max < values.Length - 2 ? 0 : max;
            return max / (float)values.Length;
        }
        public static Dictionary<int, float> SelectNonZero(this float[] values)
        {
            var result = new Dictionary<int, float>();
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < 0)
                {
                    result[i] = values[i];
                }
            }
            return result;
        }

        public static double Variance(this float[] values)
        {
            return Variance(values, values.Average());
        }
        public static float Variance(this float[] values, float mean, bool unbiased)
        {
            float num = 0f;
            for (int i = 0; i < values.Length; i++)
            {
                float num2 = values[i] - mean;
                num += num2 * num2;
            }

            if (unbiased)
            {
                return num / (float)(values.Length - 1);
            }

            return num / (float)values.Length;
        }
        public static float Variance(this float[] values, float mean)
        {
            float num = 0f;
            for (int i = 0; i < values.Length; i++)
            {
                float num2 = values[i] - mean;
                num += num2 * num2;
            }

            return num / (float)(values.Length - 1);
        }

        public static double StandardDeviation(this float[] values)
        {
            return values.StandardDeviation(values.Average());
        }
        public static double StandardDeviation(this float[] values, float mean)
        {
            return System.Math.Sqrt(values.Variance(mean));
        }

        public static string DisplayString(this float[] values) => String.Join(", ", values.Select(f => f.ToString("0.00")).ToArray());
    }



    public static class FloatArrayArrayExtensions
    {
        public static float[] PerVectorAverages(this float[][] array) => array.Select(emb => emb.Average()).ToArray();
        public static float[] PerVectorSums(this float[][] array) => array.Select(emb => emb.Sum()).ToArray();
        public static float[] PerVectorMaxes(this float[][] array) => array.Select(emb => emb.Max()).ToArray();
        public static float[] PerVectorMins(this float[][] array) => array.Select(emb => emb.Min()).ToArray();

        public static float[] PerColumnAverages(float[][] array) => TransposeIterator(array).Select(col => col.Average()).ToArray();
        public static float[] PerColumnSums(this float[][] array) => TransposeIterator(array).Select(col => col.Sum()).ToArray();
        public static float[] PerColumnMaxes(this float[][] array) => TransposeIterator(array).Select(col => col.Max()).ToArray();
        public static float[] PerColumnMins(this float[][] array) => TransposeIterator(array).Select(col => col.Min()).ToArray();

        public static float[] SameSignRatio(this float[][] array) => array.Select(row => row.SameSignRatio()).ToArray();


        public static IEnumerable<float[]> TransposeIterator(this float[][] array)
        {
            var dimension = array[0].Length;
            var count = array.Length;
            for (int dim = 0; dim < dimension; dim++)
            {
                var next = new float[count];
                for (int row = 0; row < count; row++)
                {
                    next[row] = array[row][dim];
                }
                yield return next;
            }
        }
        public static float[][] Transpose(this float[][] array)
        {
            var dimension = array[0].Length;
            var count = array.Length;
            var result = new float[dimension][];
            for (int dim = 0; dim < dimension; dim++)
            {
                result[dim] = new float[count];
                for (int row = 0; row < count; row++)
                {
                    result[dim][row] = array[row][dim];
                }
            }
            return result;
        }
    }

    public static class DoubleArrayExtensions
    {
        public static float[] ToFloats(this double[] data) { return Array.ConvertAll(data, x => (float)x); }
        public static float[] ToFloats(this IEnumerable<double> data) => data.Select(x => (float)x).ToArray();
    }
}
