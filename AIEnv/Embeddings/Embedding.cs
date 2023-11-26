namespace AIEnv.Embeddings
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using AIEnv.Utils;

    [Serializable]
    public class Embedding
    {
        public string Sentence { get; private set; } = "";
        public float[] Vector { get; private set; }

        public int HiddenDimensions => Vector.Length;

        public Embedding(float[] vector)
        {
            Vector = vector;
        }
        public static Embedding[] CreateEmbeddings(float[][] values)
        {
            return values.Select(vals => new Embedding(vals)).ToArray();
        }
        public float this[int index] => Vector[index];
        public double[] AdjustedDoubles(float[] vals) => Vector.ToDoubles().Zip(vals, (a, b) => a - b).ToArray();

        public float Average() => Vector.Average();
        public float Sum() => Vector.Sum();
        public float Max() => Vector.Max();
        public float Min() => Vector.Min();

        //var sum = TransposeIterator().Sum(vectors => vectors.Sum());

        public void Add(float[] vals) => Vector = Vector.Zip(vals, (a, b) => a + b).ToArray();
        public void Subtract(float[] vals) => Vector = Vector.Zip(vals, (a, b) => a - b).ToArray();
        public void Multiply(float[] vals) => Vector = Vector.Zip(vals, (a, b) => a * b).ToArray();
        public void Divide(float[] vals) => Vector = Vector.Zip(vals, (a, b) => a / b).ToArray();

        public double StandardDeviation() => Vector.StandardDeviation();
        public double Variance() => Vector.Variance();
        public double DotProduct(float[] B) => Utils.DotProduct(Vector, B);
        public double CosineSimilarity(float[] B) => Utils.CosineSimilarity(Vector, B);


        public Complex[] ComplexDiff(Embedding other) => Vector.Zip(other.Vector, (a, b) => new Complex(-a, b)).ToArray();

        //public Image Bitmap() => GraphicUtils.ImageFromFloats(Vector);

        public float[] DiffBytes(float[] data0, float[] data1)
        {
            float[] result = new float[HiddenDimensions];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = data0[i] - data1[i];
            }
            return result;
        }


        public void MakeIsotropic(float[] averages, float[] stds)
        {
            Subtract(averages);
            Divide(stds);
        }



    }
}
