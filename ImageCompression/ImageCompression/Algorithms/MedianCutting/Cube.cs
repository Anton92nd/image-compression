using System;
using System.Collections.Generic;
using System.Linq;
using ImageCompression.Structures;
using JetBrains.Annotations;

namespace ImageCompression.Algorithms.MedianCutting
{
    public class Cube
    {
        public Vector<byte>[] Vectors { get; private set; }

        public Cube(IEnumerable<Vector<byte>> vectors)
        {
            Vectors = vectors.ToArray();
        }

        public void Split(out Cube cube1, out Cube cube2)
        {
            byte[] min, max;
            var stats = GetStats(out min, out max);
            for (var i = 0; i < 3; i++)
            {
                var j = (i + 1)%3;
                var k = (i + 2)%3;
                if (max[i] - min[i] >= max[j] - min[j] && max[i] - min[i] >= max[k] - min[k])
                {
                    var index = BinarySearch(stats[i], min[i], max[i]);
                    SplitByIndex(i, index, Vectors, out cube1, out cube2);
                    return;
                }
            }
            throw new Exception("Impossible exception");
        }

        private int[][] GetStats(out byte[] min, out byte[] max)
        {
            var stats = new int[3][];
            stats[0] = new int[256];
            stats[1] = new int[256];
            stats[2] = new int[256];
            min = new byte[] {255, 255, 255};
            max = new byte[] {0, 0, 0};
            foreach (var vector in Vectors)
            {
                for (var i = 0; i < 3; i++)
                {
                    min[i] = Math.Min(min[i], vector[i]);
                    max[i] = Math.Max(max[i], vector[i]);
                    stats[i][vector[i]]++;
                }
            }
            for (var i = 0; i < 3; ++i)
            {
                for (var j = 1; j < 256; ++j)
                    stats[i][j] += stats[i][j - 1];
            }
            return stats;
        }

        public Vector<byte> GetMedian()
        {
            byte[] min, max;
            var stats = GetStats(out min, out max);
            var v = new byte[]{0, 0, 0};
            for (var i = 0; i < 3; i++)
            {
                v[i] = (byte) BinarySearch(stats[i], min[i], max[i]);
            }
            return new Vector<byte>(v);
        }

        private static void SplitByIndex(int dim, int splitBy, Vector<byte>[] vectors, out Cube cube1, out Cube cube2)
        {
            var list1 = new List<Vector<byte>>();
            var list2 = new List<Vector<byte>>();
            foreach (var vector in vectors)
            {
                if (vector[dim] <= splitBy)
                {
                    list1.Add(vector);
                }
                else
                {
                    list2.Add(vector);
                }
            }
            cube1 = new Cube(list1);
            cube2 = new Cube(list2);
        }

        private static int BinarySearch([NotNull] int[] array, int l, int r)
        {
            var toFind = (array[l] + array[r])/2;
            while (l + 1 < r)
            {
                var m = (l + r)/2;
                if (array[m] < toFind)
                    l = m;
                else
                    r = m;
            }
            return l;
        }
    }
}