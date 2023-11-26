namespace AIEnv.Embeddings
{
    using AIEnv.Utils;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class LastHiddenLayer
    {
        public float[][] Layers { get; private set; }

        public int LayerCount => Layers.Length;
        public int HiddenDimensions => Layers.GetLength(1);

        public LastHiddenLayer(float[][] layers)
        {
            Layers = layers;
        }
        public IEnumerable<float[]> LayerIterator() => Layers.Select(vector => vector);

        public float[] this[int index] => Layers[index];
        public double[] DoublesAt(int index) => Utils.ToDoubles(Layers[index]);

        public float[] PerVectorAverages => Layers.Select(emb => emb.Average()).ToArray();
        public float[] PerVectorSums => Layers.Select(emb => emb.Sum()).ToArray();
        public float[] PerVectorMaxes => Layers.Select(emb => emb.Max()).ToArray();
        public float[] PerVectorMins => Layers.Select(emb => emb.Min()).ToArray();

        public float[] PerColumnAverages => TransposeIterator().Select(col => col.Average()).ToArray();
        public float[] PerColumnSums => TransposeIterator().Select(col => col.Sum()).ToArray();
        public float[] PerColumnMaxes => TransposeIterator().Select(col => col.Max()).ToArray();
        public float[] PerColumnMins => TransposeIterator().Select(col => col.Min()).ToArray();

        public double DotProduct(int index, float[] B) => Utils.DotProduct(Layers[index], B);
        public double CosineSimilarity(int index, float[] B) => Utils.CosineSimilarity(Layers[index], B);

        public IEnumerable<float[]> TransposeIterator()
        {
            for (int dim = 0; dim < HiddenDimensions; dim++)
            {
                var next = new float[LayerCount];
                for (int row = 0; row < LayerCount; row++)
                {
                    next[row] = Layers[row][dim];
                }
                yield return next;
            }
        }
        public float[][] LayersTranspose()
        {
            var result = new float[HiddenDimensions][];
            for (int dim = 0; dim < HiddenDimensions; dim++)
            {
                result[dim] = new float[LayerCount];
                for (int row = 0; row < LayerCount; row++)
                {
                    result[dim][row] = Layers[row][dim];
                }
            }
            return result;
        }

        public void GenAvgStd()
        {
            var avgs = PerColumnAverages;
            File.WriteAllText(IOUtils.TextParsedFolder + "_avg.json", JsonConvert.SerializeObject(avgs, Formatting.Indented));

            var std = Layers.Select(layer => layer.StandardDeviation()).ToArray();
            File.WriteAllText(IOUtils.TextParsedFolder + "_std.json", JsonConvert.SerializeObject(std, Formatting.Indented));
        }
    }
}
