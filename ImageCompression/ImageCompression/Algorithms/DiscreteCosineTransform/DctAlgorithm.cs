using System;
using System.Windows.Media.Imaging;
using ImageCompression.Structures;

namespace ImageCompression.Algorithms.DiscreteCosineTransform
{
    public static class DctAlgorithm
    {
        public static Vector<byte>[] Build(BitmapSource bitmap, DctParameters dctParameters)
        {
            var map = bitmap.GetMap();
            var mapInYCbCr = ToYCbCr(map);
            var decimatedMap = Decimate(mapInYCbCr, dctParameters.DecimationType);
            return null;
        }

        private static Vector<byte>[,] ToYCbCr(Vector<byte>[,] map)
        {
            var result = new Vector<byte>[map.GetLength(0), map.GetLength(1)];
            for (var i = 0; i < map.GetLength(0); ++i)
            {
                for (var j = 0; j < map.GetLength(1); ++j)
                {
                    result[i, j] = Transformation.VectorRGBToYCbCr(map[i, j]);
                }
            }
            return result;
        }

        private static Vector<byte>[,] Decimate(Vector<byte>[,] map, DecimationType decimationType)
        {
            if (decimationType == DecimationType.NoDecimation)
                return map;
            if (decimationType == DecimationType.Horizontal)
            {
                var result = new Vector<byte>[map.GetLength(0), map.GetLength(1) / 2];
                for (var i = 0; i < map.GetLength(0); ++i)
                {
                    for (var j = 0; j < map.GetLength(1); j += 2)
                    {
                        result[i, j/2] = map[i, j];
                    }
                }
                return result;
            }
            else if (decimationType == DecimationType.Vertical)
            {
                var result = new Vector<byte>[map.GetLength(0)/2, map.GetLength(1)];
                for (var i = 0; i < map.GetLength(0); i += 2)
                {
                    for (var j = 0; j < map.GetLength(1); ++j)
                    {
                        result[i/2, j] = map[i, j];
                    }
                }
                return result;
            }
            else
            {
                var result = new Vector<byte>[map.GetLength(0)/2, map.GetLength(1)/2];
                for (var i = 0; i < map.GetLength(0); i += 2)
                {
                    for (var j = 0; j < map.GetLength(1); j += 2)
                    {
                        result[i/2, j/2] = map[i, j];
                    }
                }
                return result;
            }
        }
    }
}
