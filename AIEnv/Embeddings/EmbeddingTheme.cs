namespace AIEnv.Embeddings
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Numerics;
    using AIEnv.Utils;

    [Serializable]
    public class EmbeddingTheme
    {
        [JsonProperty("embeddings")]
        public List<Embedding> Embeddings { get; } = new List<Embedding>();

        [JsonProperty("sentences")]
        public string[] Sentences { get; set; } = new string[0];

        [JsonProperty("count")]
        public int Count => Embeddings.Count;

        [JsonIgnore]
        public string ThemeName { get; set; } = "";

        [JsonIgnore]
        public int HiddenDimensions => Embeddings.Count > 0 ? Embeddings[0].HiddenDimensions : 0;

        [JsonIgnore]
        public int TotalValueCount => Count * HiddenDimensions;

        public Embedding this[int index] => Embeddings[index];
        public float[] VectorAt(int index) => Embeddings[index].Vector;

        public void AddRange(IEnumerable<Embedding> embeddings) => Embeddings.AddRange(embeddings);


        public IEnumerable<Embedding> EmbeddingsIterator() => Embeddings.Select(embedding => embedding);

        public float[] PerVectorAverages() => Embeddings.Select(emb => emb.Average()).ToArray();
        public float[] PerVectorSums() => Embeddings.Select(emb => emb.Sum()).ToArray();
        public float[] PerVectorMaxes() => Embeddings.Select(emb => emb.Max()).ToArray();
        public float[] PerVectorMins() => Embeddings.Select(emb => emb.Min()).ToArray();

        public float[] PerColumnAverages() => TransposeIterator().Select(col => col.Average()).ToArray();
        public float[] PerColumnSums() => TransposeIterator().Select(col => col.Sum()).ToArray();
        public float[] PerColumnMaxes() => TransposeIterator().Select(col => col.Max()).ToArray();
        public float[] PerColumnMins() => TransposeIterator().Select(col => col.Min()).ToArray();

        public float Average() => PerVectorAverages().Average();
        public float Sum() => PerVectorSums().Sum();
        public float Min() => PerVectorMins().Min();
        public float Max() => PerVectorMaxes().Max();

        public double DotProduct(int index, float[] B) => Utils.DotProduct(VectorAt(index), B);
        public double CosineSimilarity(int index, float[] B) => Utils.CosineSimilarity(VectorAt(index), B);

        public Complex[][] ComplexDiff(EmbeddingTheme other) =>
            Embeddings.Zip(other.Embeddings, (a, b) => Utils.ComplexDiff(a.Vector, b.Vector)).ToArray();
        public Complex[][] PerColumnComplexDiff(EmbeddingTheme other) =>
            TransposeIterator().Zip(other.TransposeIterator(), (a, b) => Utils.ComplexDiff(a, b)).ToArray();
        public float[] PerColumnStd() => TransposeIterator().Select(a => a.StandardDeviation()).ToFloats();

        // Returns array per token slot (hidden dimension)
        public IEnumerable<float[]> TransposeIterator()
        {
            for (int dim = 0; dim < HiddenDimensions; dim++)
            {
                var next = new float[Count];
                for (int row = 0; row < Count; row++)
                {
                    next[row] = Embeddings[row][dim];
                }
                yield return next;
            }
        }

        public float[][] EmbeddingsTranspose()
        {
            var result = new float[HiddenDimensions][];
            for (int dim = 0; dim < HiddenDimensions; dim++)
            {
                result[dim] = new float[Count];
                for (int row = 0; row < Count; row++)
                {
                    result[dim][row] = Embeddings[row][dim];
                }
            }
            return result;
        }

        public float[] EvenAverages()
        {
            var evenEmbeddings = Embeddings.Where((c, i) => i % 2 == 0).ToArray();
            var result = new float[HiddenDimensions];
            for (int col = 0; col < result.Length; col++)
            {
                result[col] = evenEmbeddings.Select(a => a[col]).Sum() / evenEmbeddings.Length;
            }
            return result;
        }
        public float[] OddAverages()
        {
            var oddEmbeddings = Embeddings.Where((c, i) => i % 2 != 0).ToArray();
            var result = new float[HiddenDimensions];
            for (int col = 0; col < result.Length; col++)
            {
                result[col] = oddEmbeddings.Select(a => a[col]).Sum() / oddEmbeddings.Length;
            }
            return result;
        }

        public byte[,] EmbeddingsToColorRanges(int[]? ranges = null)
        {
            List<Embedding> eafs;
            if (ranges == null)
            {
                eafs = Embeddings;
            }
            else
            {
                eafs = ranges.Select(index => Embeddings[index]).ToList();
            }

            byte[,] result = new byte[eafs.Count, HiddenDimensions];

            var min = Max();
            var max = Min();
            Trace.WriteLine(min + ":" + max);
            for (int j = 0; j < eafs.Count; j++)
            {
                var emb = eafs[j];
                for (int i = 0; i < emb.HiddenDimensions; i++)
                {
                    var normVal = (emb[i] - min) / (max - min);
                    result[j, i] = (byte)Math.Max(0, Math.Min(255, 255 * normVal)); // clamp
                }
            }
            return result;
        }

        public double[,] PairSimilarityAvg(float[] averages)
        {
            var halfCount = (int)Math.Floor(Count / 2.0);
            var result = new double[halfCount, 2];
            for (int j = 0; j < halfCount; j++)
            {
                result[j, 0] = CosineSimilarity(j * 2, averages);
                result[j, 1] = CosineSimilarity(j * 2 + 1, averages);
                var eachOther = Utils.CosineSimilarity(VectorAt(j * 2), VectorAt(j * 2 + 1));
                Trace.WriteLine("sim " + j + ": " + result[j, 0] + "\t" + result[j, 1] + "\t\t" + eachOther);
            }
            return result;
        }
        public double[] PairSimilarity()
        {
            var halfCount = (int)Math.Floor(Count / 2.0);
            var result = new double[halfCount];
            for (int j = 0; j < halfCount; j++)
            {
                var eachOther = Utils.CosineSimilarity(VectorAt(j * 2), VectorAt(j * 2 + 1));
                result[j] = eachOther;
                //Trace.WriteLine("psim " + j + ": " + eachOther);
            }
            return result;
        }

        private float[]? _avgs = null;
        private float[]? _stds = null;
        public void MakeIsotropic(float[]? avgs = null, float[]? stds = null)
        {
            if (avgs == null)
            {
                GenAvgStd();
                avgs = _avgs;
                stds = _stds;
            }
            Embeddings.ForEach(emb => emb.MakeIsotropic(avgs!, stds!));
        }

        public void GenAvgStd()
        {
            _avgs = PerColumnAverages();
            File.WriteAllText(IOUtils.TextParsedFolder + "_avg.json", JsonConvert.SerializeObject(_avgs, Formatting.Indented));

            //_stds = Embeddings.Select(emb => emb.Vector.StandardDeviation()).ToFloats();
            _stds = TransposeIterator().Select(emb => emb.StandardDeviation()).ToFloats();
            File.WriteAllText(IOUtils.TextParsedFolder + "_std.json", JsonConvert.SerializeObject(_stds, Formatting.Indented));
        }


        //public Image ImageDiff(int indexA, int indexB) => GraphicUtils.ImageDiff(VectorAt(indexA), VectorAt(indexB));
        //public Image ImageSumColumnsDiff(EmbeddingTheme other) => GraphicUtils.ImageDiff(PerColumnSums(), other.PerColumnSums());
        //public Image[] Bitmaps(params int[] indexes) => indexes.Select(i => GraphicUtils.ImageFromFloats(VectorAt(i))).ToArray();

        public static EmbeddingTheme? LoadJsonText(string filename)
        {
            EmbeddingTheme? result = null;
            string fullPath = IOUtils.TextParsedFolder + filename + ".json";
            if (File.Exists(fullPath) && !filename.EndsWith("00test")) // always call api for 'test.txt'
            {
                var txt = File.ReadAllText(fullPath);
                result = JsonConvert.DeserializeObject<EmbeddingTheme>(txt);
            }
            return result;
        }
        public void SaveJson(string filename)
        {
            string fullPath = IOUtils.TextParsedFolder + filename + ".json";
            using (var streamWriter = new StreamWriter(fullPath))
            {
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }
        public static EmbeddingTheme? LoadBinary(string filename)
        {
            EmbeddingTheme? result = null;
            string fullPath = IOUtils.TextParsedFolder + filename + ".bin";
            if (File.Exists(fullPath) && !filename.EndsWith("00test")) // always call api for 'test.txt'
            {
                using (FileStream fileStream = new FileStream(fullPath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
                    result = formatter.Deserialize(fileStream) as EmbeddingTheme;
                }
            }
            return result;
        }
        public void SaveBinary(string filename)
        {
            string fullPath = IOUtils.TextParsedFolder + filename + ".bin";
            using (FileStream fs = new FileStream(fullPath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, this);
            }
        }

    }
}
