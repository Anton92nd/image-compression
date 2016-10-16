using System;
using System.Collections.Generic;
using System.Linq;
using ImageCompression.Structures;

namespace ImageCompression.Algorithms.MedianCutting
{
    public static class MedianCut
    {
        public static Vector<byte>[] Build(int palletSize, IList<Vector<byte>> colors, out Vector<byte>[] pallet)
        {
            if ((palletSize & (palletSize - 1)) != 0)
                throw new Exception("Pallet size must be power of 2");
            Cube initialCube;
            var stats = GetStatistics(colors, out initialCube);
            var cubes = new Cube[] {initialCube};
            while (cubes.Length < palletSize)
            {
                var newCubes = new List<Cube>();
                foreach (var cube in cubes)
                {
                    Cube cube1, cube2;
                    SplitCube(stats, cube, out cube1, out cube2);
                    if (newCubes.Contains(cube1))
                        throw new Exception();
                    newCubes.Add(cube1);
                    if (newCubes.Contains(cube2))
                        throw new Exception();
                    newCubes.Add(cube2);
                }
                cubes = newCubes.ToArray();
            }
            var palletDict = new Dictionary<Cube, Vector<byte>>();
            foreach (var cube in cubes)
            {
                if (palletDict.ContainsKey(cube))
                    throw new Exception();
                palletDict[cube] = GetCubeMedian(stats, cube);
            }
            pallet = palletDict.Values.ToArray();
            return colors.Select(color => palletDict[palletDict.Keys.First(cube => cube.Contains(color))]).ToArray();
        }

        private static Vector<byte> GetCubeMedian(int[][] stats, Cube cube)
        {
            var indexR = BinarySearch(stats[0], cube.Ox);
            var indexG = BinarySearch(stats[1], cube.Oy);
            var indexB = BinarySearch(stats[2], cube.Oy);
            return new Vector<byte>(new []{(byte)indexR, (byte)indexG, (byte)indexB});
        }

        private static int BinarySearch(int[] array, Segment segment)
        {
            int l = segment.Start, r = segment.End;
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

        private static void SplitCube(int[][] stats, Cube cube, out Cube cube1, out Cube cube2)
        {
            if (cube.Ox.Length >= cube.Oy.Length && cube.Ox.Length >= cube.Oz.Length)
            {
                var index = BinarySearch(stats[0], cube.Ox);
                cube1 = new Cube(new Segment(cube.Ox.Start, index), cube.Oy, cube.Oz);
                cube2 = new Cube(new Segment(index + 1, cube.Ox.End), cube.Oy, cube.Oz);
            }
            else if (cube.Oy.Length >= cube.Ox.Length && cube.Oy.Length >= cube.Oz.Length)
            {
                var index = BinarySearch(stats[1], cube.Oy);
                cube1 = new Cube(cube.Ox, new Segment(cube.Oy.Start, index), cube.Oz);
                cube2 = new Cube(cube.Ox, new Segment(index + 1, cube.Oy.End), cube.Oz);
            }
            else
            {
                var index = BinarySearch(stats[2], cube.Oz);
                cube1 = new Cube(cube.Ox, cube.Oy, new Segment(cube.Oz.Start, index));
                cube2 = new Cube(cube.Ox, cube.Oy, new Segment(index + 1, cube.Oz.End));
            }
        }


        private static int[][] GetStatistics(IList<Vector<byte>> colors, out Cube cube)
        {
            var counts = new int[3][];
            counts[0] = new int[256];
            counts[1] = new int[256];
            counts[2] = new int[256];
            var mins = new int[3] {300, 300, 300};
            var maxs = new int[3] {0, 0, 0};
            foreach (var vector in colors)
            {
                for (var i = 0; i < 3; ++i)
                {
                    counts[i][vector[i]]++;
                    maxs[i] = Math.Max(maxs[i], vector[i]);
                    mins[i] = Math.Min(mins[i], vector[i]);
                }
            }
            cube = new Cube(new Segment(mins[0], maxs[0]), new Segment(mins[1], maxs[1]), new Segment(mins[2], maxs[2]));
            for (var i = 0; i < 3; i++)
            {
                for (var j = 1; j < 256; j++)
                {
                    counts[i][j] += counts[i][j - 1];
                }
            }
            return counts;
        }
    }
}
