using System;
using System.IO;
using ImageCompression.Algorithms.DiscreteCosineTransform;
using ImageCompression.Structures;
using SevenZip;

namespace ImageCompression.Algorithms.JPG
{
    public static class Jpeg
    {
        public static void Save(Vector<int>[,] data, DctParameters parameters)
        {
            using (var fileStream = File.Open(parameters.SavePath, FileMode.Create))
            using (var encodeStream = new LzmaEncodeStream(fileStream))
            using (var writer = new BinaryWriter(encodeStream))
            {
                writer.Write((byte)parameters.DecimationType);
                writer.Write((byte)parameters.QuantizationType);
                switch (parameters.QuantizationType)
                {
                    case QuantizationType.LargestN:
                        writer.Write(parameters.Ny.Value);
                        writer.Write(parameters.Nc.Value);
                        break;
                    case QuantizationType.QuantizationMatrix:
                        writer.Write(parameters.GeneratorsY.Alpha);
                        writer.Write(parameters.GeneratorsY.Gamma);
                        writer.Write(parameters.GeneratorsC.Alpha);
                        writer.Write(parameters.GeneratorsC.Gamma);
                        break;
                    case QuantizationType.DefaultJpegMatrix:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var n = data.GetLength(0);
                var m = data.GetLength(1);
                writer.Write(n);
                writer.Write(m);
                for (var i = 0; i < n; ++i)
                {
                    for (var j = 0; j < m; ++j)
                    {
                        for (var k = 0; k < 3; ++k)
                        {
                            writer.Write(data[i, j][k]);
                        }
                    }
                }
            }
        }

        public static Vector<int>[,] Load(DctParameters parameters)
        {
            using (var fileStream = File.Open(parameters.SavePath, FileMode.Open))
            using (var decodeStream = new LzmaDecodeStream(fileStream))
            using (var reader = new BinaryReader(decodeStream))
            {
                parameters.DecimationType = (DecimationType) reader.ReadByte();
                parameters.QuantizationType = (QuantizationType) reader.ReadByte();
                switch (parameters.QuantizationType)
                {
                    case QuantizationType.LargestN:
                        parameters.Ny = reader.ReadInt32();
                        parameters.Nc = reader.ReadInt32();
                        break;
                    case QuantizationType.QuantizationMatrix:
                        var a = reader.ReadInt32();
                        var b = reader.ReadInt32();
                        var c  = reader.ReadInt32();
                        var d = reader.ReadInt32();
                        parameters.GeneratorsY = new MatrixGenerators
                        {
                            Alpha = a,
                            Gamma = b,
                        };
                        parameters.GeneratorsC = new MatrixGenerators
                        {
                            Alpha = c,
                            Gamma = d,
                        };
                        break;
                    case QuantizationType.DefaultJpegMatrix:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var n = reader.ReadInt32();
                var m = reader.ReadInt32();

                var result = new Vector<int>[n, m];
                for (var i = 0; i < n; ++i)
                {
                    for (var j = 0; j < m; ++j)
                    {
                        var a = reader.ReadInt32();
                        var b = reader.ReadInt32();
                        var c = reader.ReadInt32();
                        result[i, j] = new Vector<int>(new []{a, b, c});
                    }
                }
                return result;
            }
        }
    }
}
