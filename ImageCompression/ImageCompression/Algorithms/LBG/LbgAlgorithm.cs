using System;
using System.Collections.Generic;
using System.Linq;
using ImageCompression.Structures;

namespace ImageCompression.Algorithms.LBG
{
    public static class LbgAlgorithm
    {
        private const int iterationsCount = 5;

        public static Vector<byte>[] Build(int paletteSize, Vector<byte>[] colors, out Vector<byte>[] palette)
        {
            palette = GetInitialPalette(paletteSize, colors);
            if (paletteSize != palette.Length)
                throw new Exception("WTF");
            var sites = Enumerable.Range(0, paletteSize).Select(x => new List<Vector<byte>>()).ToArray();
            for (var it = 0; it < iterationsCount; ++it)
            {
                foreach (var site in sites)
                {
                    site.Clear();
                }
                foreach (var color in colors)
                {
                    var bestSite = -1;
                    var bestDistance = Int32.MaxValue;
                    for (var j = 0; j < paletteSize; ++j)
                    {
                        var distance = Distance(color, palette[j]);
                        if (distance < bestDistance)
                        {
                            bestSite = j;
                            bestDistance = distance;
                        }
                    }
                    sites[bestSite].Add(color);
                }
                for (var i = 0; i < paletteSize; ++i)
                {
                    palette[i] = sites[i].Any() ? MeanVector(sites[i]) : palette[i];
                }
            }
            return EncodeByPalette(colors, palette);
        }

        private static Vector<byte>[] EncodeByPalette(Vector<byte>[] colors, Vector<byte>[] palette)
        {
            var result = new Vector<byte>[colors.Length];
            for (var i = 0; i < colors.Length; ++i)
            {
                var bestSite = -1;
                var bestDistance = Int32.MaxValue;
                for (var j = 0; j < palette.Length; ++j)
                {
                    var distance = Distance(colors[i], palette[j]);
                    if (distance < bestDistance)
                    {
                        bestSite = j;
                        bestDistance = distance;
                    }
                }
                result[i] = palette[bestSite];
            }
            return result;
        }

        private static Vector<byte>[] GetInitialPalette(int paletteSize, Vector<byte>[] colors)
        {
            byte quantizationRate = 1;
            for (; quantizationRate <= 7; quantizationRate++)
            {
                var palette = CreateQuantizedSet(colors, quantizationRate);
                if (palette.Count >= paletteSize)
                    break;
            }
            var quantizedSet = CreateQuantizedSet(colors, quantizationRate).ToArray();
            return SolveByKruskal(paletteSize, quantizedSet);
        }

        private static Vector<byte>[] SolveByKruskal(int paletteSize, Vector<byte>[] quantizedSet)
        {
            var edges = new List<Edge>();
            for (var i = 0; i < quantizedSet.Length; ++i)
            {
                for (var j = 0; j < i; ++j)
                {
                    edges.Add(new Edge(i, j, Distance(quantizedSet[i], quantizedSet[j])));
                }
            }
            edges.Sort();
            var dsu = new DSU(quantizedSet.Length);
            var count = quantizedSet.Length;
            for (var i = 0; i < edges.Count; ++i)
            {
                if (dsu.Get(edges[i].U) != dsu.Get(edges[i].V))
                {
                    dsu.Link(edges[i].U, edges[i].V);
                    count--;
                }
                if (count == paletteSize)
                    break;
            }
            var result = new Dictionary<int, List<Vector<byte>>>();
            for (var i = 0; i < quantizedSet.Length; ++i)
            {
                if (!result.ContainsKey(dsu.Get(i)))
                    result[dsu.Get(i)] = new List<Vector<byte>>();
                result[dsu.Get(i)].Add(quantizedSet[i]);
            }

            return result.Values.Select(MeanVector).ToArray();
        }

        private static Vector<byte> MeanVector(List<Vector<byte>> cluster)
        {
            var sum = new int[3];
            foreach (var vector in cluster)
            {
                for (var i = 0; i < 3; i++)
                    sum[i] += vector[i];
            }
            return new Vector<byte>(sum.Select(v => (byte)(v / cluster.Count)));
        }


        private static int Distance(Vector<byte> a, Vector<byte> b)
        {
            var result = 0;
            for (var i = 0; i < a.Length; ++i)
            {
                result += (a[i] - b[i])*(a[i] - b[i]);
            }
            return result;
        }

        private static HashSet<Vector<byte>> CreateQuantizedSet(Vector<byte>[] colors, byte i)
        {
            var quantizationVector = QuantizationVectorFunc(i);
            var result = new HashSet<Vector<byte>>();
            foreach (var color in colors)
            {
                result.Add(Quantization.QuantizeVector(color, quantizationVector));
            }
            return result;
        }

        private static readonly Func<byte, Vector<byte>> QuantizationVectorFunc = x => new Vector<byte>(new []{x, x, x});
    }
}
