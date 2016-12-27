using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Algorithms.JPG;
using ImageCompression.Structures;
using Matrix = ImageCompression.Structures.Matrix;

namespace ImageCompression.Algorithms.DiscreteCosineTransform
{
    public static class DctAlgorithm
    {
        public static Vector<int>[,] ApplyDct(BitmapSource bitmap, DctParameters dctParameters)
        {
            var map = bitmap.GetMap();
            var mapInYCbCr = ToYCbCr(map);
            var decimatedMap = Decimate(mapInYCbCr, dctParameters.DecimationType);
            var blocks = SplitToBlocks(decimatedMap, 8);
            var dct = GetDctMatrix();
            var transposedDct = dct.Transpose();
            var appliedDct = ApplyDct(blocks, dct, transposedDct);
            var quantizedDct = QuantizeMatrices(appliedDct, dctParameters);
            return Flatten(quantizedDct);
        }

        public static BitmapSource DecodeDct(DctParameters dctParameters)
        {
            var encoded = Jpeg.Load(dctParameters);
            var encodedMatrices = SplitToBlocksInt(encoded, 8);
            var dequantizedMatrices = DequantizeMatrices(encodedMatrices, dctParameters);
            var dct = GetDctMatrix();
            var transposedDct = dct.Transpose();
            var inversedDct = ApplyDct(dequantizedMatrices, transposedDct, dct);
            var flattened = FlattenByte(inversedDct);
            var undecimated = Undecimate(flattened, dctParameters.DecimationType);
            var inRgb = ToRgbBytes(undecimated);
            return BitmapSource.Create(undecimated.GetLength(0), undecimated.GetLength(1), 96.0, 96.0, PixelFormats.Bgr32, 
                null, inRgb, inRgb.Length / undecimated.GetLength(0));
        }

        private static Vector<Matrix>[,] DequantizeMatrices(Vector<Matrix>[,] encodedMatrices, DctParameters dctParameters)
        {
            switch (dctParameters.QuantizationType)
            {
                case QuantizationType.LargestN:
                    return encodedMatrices;
                case QuantizationType.DefaultJpegMatrix:
                    return DequantizeByMatrices(encodedMatrices, DefaultMatrixY, DefaultMatrixC);
                case QuantizationType.QuantizationMatrix:
                    var matrixY = GetQuantizationMatrix(dctParameters.GeneratorsY);
                    var matrixC = GetQuantizationMatrix(dctParameters.GeneratorsC);
                    return DequantizeByMatrices(encodedMatrices, matrixY, matrixC);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Vector<int>[,] Flatten(Vector<Matrix>[,] matrices)
        {
            var n = matrices.GetLength(0);
            var m = matrices.GetLength(1);
            var result = new Vector<int>[n * 8, m * 8];
            for (var i = 0; i < n; ++i)
                for (var j = 0; j < m; ++j)
                    PlaceMatrix(result, i*8, j*8, matrices[i, j]);
            return result;
        }

        private static Vector<byte>[,] FlattenByte(Vector<Matrix>[,] matrices)
        {
            var n = matrices.GetLength(0);
            var m = matrices.GetLength(1);
            var result = new Vector<byte>[n * 8, m * 8];
            for (var i = 0; i < n; ++i)
                for (var j = 0; j < m; ++j)
                    PlaceMatrixByte(result, i * 8, j * 8, matrices[i, j]);
            return result;
        }

        private static void PlaceMatrix(Vector<int>[,] result, int x, int y, Vector<Matrix> vector)
        {
            for (var i = 0; i < 8; ++i)
                for (var j = 0; j < 8; ++j)
                {
                    var i1 = i;
                    var j1 = j;
                    result[x + i, y + j] = vector.Convert(v => Convert.ToInt32(v[i1, j1]));
                }
        }

        private static void PlaceMatrixByte(Vector<byte>[,] result, int x, int y, Vector<Matrix> vector)
        {
            for (var i = 0; i < 8; ++i)
                for (var j = 0; j < 8; ++j)
                {
                    var i1 = i;
                    var j1 = j;
                    result[x + i, y + j] = vector.Convert(v => ConvertToByte(v[i1, j1]));
                }
        }

        private static byte ConvertToByte(double value)
        {
            var intValue = Convert.ToInt32(Math.Round(value));
            return (byte)Math.Max(0, Math.Min(255, intValue));
        }

        private static Vector<Matrix>[,] QuantizeMatrices(Vector<Matrix>[,] appliedDct, DctParameters dctParameters)
        {
            switch (dctParameters.QuantizationType)
            {
                case QuantizationType.LargestN:
                    if (!dctParameters.Ny.HasValue || !dctParameters.Nc.HasValue)
                        throw new Exception("Invalid program state");
                    return QuantizeLargest(appliedDct, dctParameters.Ny.Value, dctParameters.Nc.Value);
                case QuantizationType.QuantizationMatrix:
                    var matrixY = GetQuantizationMatrix(dctParameters.GeneratorsY);
                    var matrixC = GetQuantizationMatrix(dctParameters.GeneratorsC);
                    return QuantizeByMatrices(appliedDct, matrixY, matrixC);
                case QuantizationType.DefaultJpegMatrix:
                    return QuantizeByMatrices(appliedDct, DefaultMatrixY, DefaultMatrixC);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Vector<Matrix>[,] QuantizeByMatrices(Vector<Matrix>[,] appliedDct, Matrix matrixY, Matrix matrixC)
        {
            for (var i = 0; i < appliedDct.GetLength(0); ++i)
                for (var j = 0; j < appliedDct.GetLength(1); ++j)
                {
                    appliedDct[i, j][0] = appliedDct[i, j][0]/matrixY;
                    appliedDct[i, j][1] = appliedDct[i, j][1]/matrixC;
                    appliedDct[i, j][2] = appliedDct[i, j][2]/matrixC;
                }
            return appliedDct;
        }

        private static Vector<Matrix>[,] DequantizeByMatrices(Vector<Matrix>[,] appliedDct, Matrix matrixY, Matrix matrixC)
        {
            for (var i = 0; i < appliedDct.GetLength(0); ++i)
                for (var j = 0; j < appliedDct.GetLength(1); ++j)
                {
                    appliedDct[i, j][0] = appliedDct[i, j][0] % matrixY;
                    appliedDct[i, j][1] = appliedDct[i, j][1] % matrixC;
                    appliedDct[i, j][2] = appliedDct[i, j][2] % matrixC;
                }
            return appliedDct;
        }

        private static Matrix GetQuantizationMatrix(MatrixGenerators generators)
        {
            var result = new Matrix(8, 8);
            for (var i = 0; i < 8; ++i)
                for (var j = 0; j < 8; ++j)
                    result[i, j] = generators.Alpha*(1 + generators.Gamma*(i + j + 2));
            return result;
        }

        private static Vector<Matrix>[,] QuantizeLargest(Vector<Matrix>[,] appliedDct, int ny, int nc)
        {
            var list = new List<Zzz>();
            for (var i = 0; i < appliedDct.GetLength(0); ++i)
                for (var j = 0; j < appliedDct.GetLength(1); ++j)
                    for (var k = 0; k < 3; ++k)
                    {
                        list.Clear();
                        for (var x = 0; x < 8; ++x)
                            for (var y = 0; y < 8; ++y)
                            {
                                appliedDct[i, j][k][x, y] = Math.Round(appliedDct[i, j][k][x, y]);
                                list.Add(new Zzz
                                {
                                    I = x,
                                    J = y,
                                    Value = appliedDct[i, j][k][x, y]
                                });
                            }
                        list.Sort((a, b) => a.Value.CompareTo(b.Value));
                        var n = k == 0 ? ny : nc;
                        for (var z = 0; z < 64 - n; ++z)
                        {
                            appliedDct[i, j][k][list[z].I, list[z].J] = 0.0;
                        }
                    }
            return appliedDct;
        }

        private static Vector<Matrix>[,] ApplyDct(Vector<Matrix>[,] blocks, Matrix dct, Matrix transposedDct)
        {
            var n = blocks.GetLength(0);
            var m = blocks.GetLength(1);
            var result = new Vector<Matrix>[n, m];
            for (var i = 0; i < n; ++i)
                for (var j = 0; j < m; ++j)
                {
                    result[i, j] = new Vector<Matrix>(3);
                    for (var k = 0; k < 3; ++k)
                    {
                        result[i, j][k] = dct*blocks[i, j][k]*transposedDct;
                    }
                }
            return result;
        }

        private static Matrix GetDctMatrix()
        {
            var result = new Matrix(8, 8);
            for (var j = 0; j < 8; ++j)
                result[0, j] = 1.0/Math.Sqrt(8);
            for (var i = 1; i < 8; ++i)
                for (var j = 0; j < 8; ++j)
                    result[i, j] = 0.5*Math.Cos((2*j + 1)*i*Math.PI/16.0);
            return result;
        }

        private static Vector<Matrix>[,] SplitToBlocks(Vector<byte>[,] map, int blockSize)
        {
            var result = new Vector<Matrix>[map.GetLength(0)/blockSize, map.GetLength(1)/blockSize];
            for (var i = 0; i < map.GetLength(0); i += blockSize)
            {
                for (var j = 0; j < map.GetLength(1); j += blockSize)
                {
                    var array = new Vector<double>[blockSize, blockSize];
                    for (var x = 0; x < blockSize; ++x)
                        for (var y = 0; y < blockSize; ++y)
                            array[x, y] = map[i + x, j + y].Convert(Convert.ToDouble);
                    var matrices = new Matrix[3];
                    for (var k = 0; k < 3; ++k)
                        matrices[k] = new Matrix(Project(array, k));
                    result[i/blockSize, j/blockSize] = new Vector<Matrix>(matrices);
                }
            }
            return result;
        }

        private static Vector<Matrix>[,] SplitToBlocksInt(Vector<int>[,] map, int blockSize)
        {
            var result = new Vector<Matrix>[map.GetLength(0) / blockSize, map.GetLength(1) / blockSize];
            for (var i = 0; i < map.GetLength(0); i += blockSize)
            {
                for (var j = 0; j < map.GetLength(1); j += blockSize)
                {
                    var array = new Vector<double>[blockSize, blockSize];
                    for (var x = 0; x < blockSize; ++x)
                        for (var y = 0; y < blockSize; ++y)
                            array[x, y] = map[i + x, j + y].Convert(Convert.ToDouble);
                    var matrices = new Matrix[3];
                    for (var k = 0; k < 3; ++k)
                        matrices[k] = new Matrix(Project(array, k));
                    result[i / blockSize, j / blockSize] = new Vector<Matrix>(matrices);
                }
            }
            return result;
        }

        private static double[,] Project(Vector<double>[,] array, int index)
        {
            var n = array.GetLength(0);
            var m = array.GetLength(1);
            var result = new double[n, m];
            for (var i = 0; i < m; ++i)
                for (var j = 0; j < m; ++j)
                    result[i, j] = array[i, j][index];
            return result;
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

        private static byte[] ToRgbBytes(Vector<byte>[,] map)
        {
            var result = new List<byte>();
            for (var i = 0; i < map.GetLength(0); ++i)
            {
                for (var j = 0; j < map.GetLength(1); ++j)
                {
                    var transformed = Transformation.VectorYCbCrToRGB(map[i, j]);
                    result.AddRange(new byte[]{transformed[0], transformed[1], transformed[2], 255});
                }
            }
            return result.ToArray();
        }

        private static Vector<byte>[,] Decimate(Vector<byte>[,] map, DecimationType decimationType)
        {
            if (decimationType == DecimationType.NoDecimation)
                return map;
            if (decimationType == DecimationType.Horizontal)
            {
                var result = new Vector<byte>[map.GetLength(0), map.GetLength(1)/2];
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

        private static Vector<byte>[,] Undecimate(Vector<byte>[,] map, DecimationType decimationType)
        {
            if (decimationType == DecimationType.NoDecimation)
                return map;
            if (decimationType == DecimationType.Horizontal)
            {
                var result = new Vector<byte>[map.GetLength(0), map.GetLength(1) * 2];
                for (var i = 0; i < map.GetLength(0); ++i)
                {
                    for (var j = 0; j < map.GetLength(1); ++j)
                    {
                        result[i, j * 2] = result[i, j * 2 + 1] = map[i, j];
                    }
                }
                return result;
            }
            else if (decimationType == DecimationType.Vertical)
            {
                var result = new Vector<byte>[map.GetLength(0) * 2, map.GetLength(1)];
                for (var i = 0; i < map.GetLength(0); ++i)
                {
                    for (var j = 0; j < map.GetLength(1); ++j)
                    {
                        result[i * 2, j] = result[i * 2 + 1, j] = map[i, j];
                    }
                }
                return result;
            }
            else
            {
                var result = new Vector<byte>[map.GetLength(0) * 2, map.GetLength(1) * 2];
                for (var i = 0; i < map.GetLength(0); ++i)
                {
                    for (var j = 0; j < map.GetLength(1); ++j)
                    {
                        result[i * 2, j * 2] = result[i * 2, j * 2 + 1] = result[i * 2 + 1, j * 2] = result[i * 2 + 1, j * 2 + 1] = map[i, j];
                    }
                }
                return result;
            }
        }

        private static readonly Matrix DefaultMatrixY = new Matrix(new double[8,8]
        {
            {16,  11,  10,  16,  24,  40,  51,  61},
            {12,  12,  14,  19,  26,  58,  60,  55},
            {14,  13,  16,  24,  40,  57,  69,  56},
            {14,  17,  22,  29,  51,  87,  80,  62},
            {18,  22,  37,  56,  68,  109,  103,  77},
            {24,  35,  55,  64,  81,  104,  113,  92},
            {49,  64,  78,  87, 103,  121,  120,  101},
            {72,  92,  95,  98, 112,  100,  103,  99}
        });

        private static readonly Matrix DefaultMatrixC = new Matrix(new double[8,8]
        {
            {17,  18,  24,  47,  99,  99,  99,  99},
            {18,  21,  26,  66,  99,  99,  99,  99},
            {24,  26,  56,  99,  99,  99,  99,  99},
            {47,  66,  99,  99,  99,  99,  99,  99},
            {99,  99,  99,  99,  99,  99,  99,  99},
            {99,  99,  99,  99,  99,  99,  99,  99},
            {99,  99,  99,  99,  99,  99,  99,  99},
            {99,  99,  99,  99,  99,  99,  99,  99},
        });
    }
}
