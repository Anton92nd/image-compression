using System;
using System.Collections.Generic;
using System.Linq;
using ImageCompression.Structures;
using JetBrains.Annotations;

namespace ImageCompression.Algorithms.MedianCutting
{
    public static class MedianCut
    {
        [NotNull]
        public static Vector<byte>[] Build(int paletteSize, [NotNull] IList<Vector<byte>> colors, out Vector<byte>[] palette)
        {
            if ((paletteSize & (paletteSize - 1)) != 0)
                throw new Exception("Pallet size must be power of 2");
            var cubes = new List<Cube> {new Cube(colors)};
            while (cubes.Count < paletteSize)
            {
                var newCubes = new List<Cube>();
                foreach (var cube in cubes)
                {
                    Cube cube1, cube2;
                    cube.Split(out cube1, out cube2);
                    newCubes.Add(cube1);
                    newCubes.Add(cube2);
                }
                cubes = newCubes;
            }
            var dictPalette = new Dictionary<Vector<byte>, Vector<byte>>();
            var listPalette = new List<Vector<byte>>();
            foreach (var cube in cubes)
            {
                var value = cube.GetMedian();
                listPalette.Add(value);
                foreach (var vector in cube.Vectors)
                {
                    dictPalette[vector] = value;
                }
            }
            palette = listPalette.ToArray();
            return colors.Select(c => dictPalette[c]).ToArray();
        }
    }
}
