namespace AIEnv.Embeddings
{
    using AIEnv.Utils;
    using Newtonsoft.Json;
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class EmbeddingThemes
    {
        public List<EmbeddingTheme> Themes = new List<EmbeddingTheme>();

        public EmbeddingTheme this[int index]
        {
            get => Themes[index];
            set => Themes[index] = value;
        }
        public int Count => Themes.Count;
        public int TotalEmbeddings => Themes.Aggregate(0, (total, next) => total + next.Count);

        public void Add(EmbeddingTheme theme) => Themes.Add(theme);
        public void AddRange(IEnumerable<EmbeddingTheme> themes) => Themes.AddRange(themes);
        public void Remove(EmbeddingTheme theme) => Themes.Remove(theme);
        public void RemoveAt(int index) => Themes.RemoveAt(index);
        public void RemoveRange(int index, int count) => Themes.RemoveRange(index, count);

        public float Average() => Sum() / Count;
        public float Sum() => Themes.Aggregate(0f, (total, theme) => total + theme.Sum());
        public float Min() => Themes.Select(theme => theme.Min()).ToArray().Min();
        public float Max() => Themes.Select(theme => theme.Max()).ToArray().Max();

        public int HiddenDimensions => Themes.Count > 0 ? Themes[0].HiddenDimensions : 0;

        // Returns array per token slot (hidden dimension) of all themes combined
        public IEnumerable<float[]> TransposeAllIterator()
        {
            var totalEmbeddings = TotalEmbeddings;
            var perSlotArray = new float[totalEmbeddings];
            for (int i = 0; i < HiddenDimensions; i++)
            {
                int index = 0;
                foreach (var theme in Themes)
                {
                    foreach (var femb in theme.Embeddings)
                    {
                        perSlotArray[index++] = femb[i];
                    }
                }
                yield return perSlotArray;
            }
        }

        public float[] CompareOverlap(int indexA, int indexB)
        {
            var themeA = Themes[indexA];
            var themeB = Themes[indexB];

            var result = new float[themeB.HiddenDimensions];
            var meansA = themeA.PerColumnAverages();
            var meansB = themeB.PerColumnAverages();
            var stdsA = themeA.PerColumnStd();
            var stdsB = themeB.PerColumnStd();
            var stdAvgA = stdsA.Average();
            var stdAvgB = stdsB.Average();
            var scale = Math.Abs(meansA.Average() + meansB.Average()) / (stdAvgA + stdAvgB);
            for (int i = 0; i < result.Length; i++)
            {
                var isNarrow = stdsA[i] < stdAvgA * 1f && stdsB[i] < stdAvgB * 1f;
                var cutoff = (stdsA[i] + stdsB[i]) * scale;
                var hasOverlap = Math.Abs(meansA[i] - meansB[i]) <= cutoff;
                result[i] = !hasOverlap && isNarrow ? meansA[i] - meansB[i] - cutoff : 0;
            }
            return result;
        }

        public float[][] GetDiffs()
        {
            var result = new List<float[]>();
            if (Themes.Count > 1)
            {
                var averages = new float[Themes.Count][];
                for (int i = 0; i < Themes.Count; i++)
                {
                    averages[i] = Themes[i].PerColumnAverages();
                }

                for (int i = 1; i < Themes.Count; i++)
                {
                    result.Add(averages[i].Subtract(averages[i - 1]));
                }
            }
            return result.ToArray();
        }

        public Dictionary<int, float[]> FindGradients()
        {
            var result = new Dictionary<int, float[]>();
            if (Themes.Count > 2)
            {
                var averages = new float[Themes.Count][];
                for (int i = 0; i < Themes.Count; i++)
                {
                    averages[i] = Themes[i].PerColumnAverages();
                }

                var directions = averages[1].Zip(averages[0], (a, b) => a - b).ToArray();
                for (int i = 2; i < Themes.Count; i++)
                {
                    var newDifs = averages[i].Subtract(averages[i - 1]);
                    for (int j = 0; j < newDifs.Length; j++)
                    {
                        var a = directions[j];
                        if (a != 0)
                        {
                            var b = newDifs[j];
                            directions[j] = Math.Sign(a) == Math.Sign(b) ? b : 0;
                        }
                    }
                }

                for (int i = 0; i < directions.Length; i++)
                {
                    if (directions[i] != 0)
                    {
                        var sequence = new float[averages.Length];
                        for (int j = 0; j < sequence.Length; j++)
                        {
                            sequence[j] = averages[j][i];
                        }
                        result[i] = sequence;
                    }
                }
            }
            return result;
        }


        private float[]? _avgs = null;
        private float[]? _stds = null;
        public void MakeIsotropic()
        {
            if (_avgs == null)
            {
                GenAvgStd();
            }
            Themes.ForEach(emb => emb.MakeIsotropic(_avgs, _stds));
        }

        public void GenAvgStd()
        {
            _avgs = TransposeAllIterator().Select(col => col.Average()).ToArray();
            File.WriteAllText(IOUtils.TextParsedFolder + "_avg.json", JsonConvert.SerializeObject(_avgs, Formatting.Indented));

            _stds = TransposeAllIterator().Select(col => col.StandardDeviation()).ToFloats();
            File.WriteAllText(IOUtils.TextParsedFolder + "_std.json", JsonConvert.SerializeObject(_stds, Formatting.Indented));
        }

    }
}
