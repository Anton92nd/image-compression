using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Extensions;
using ImageCompression.Structures;
using SevenZip;

namespace ImageCompression.Algorithms.Wavelets
{
    public static class Wavelet
    {
        public static BitmapSource ApplyWavelet(BitmapSource bitmap, WaveletParameters waveletParameters)
        {
            if (bitmap.Format != PixelFormats.Gray8)
            {
                var colors = bitmap.GetColors();
                //var yCrCb = Transformation.RGBToYCbCr(colors);
                var channels = GetChannels(colors, bitmap.PixelHeight, bitmap.PixelWidth);
                for (var channel = 0; channel < 3; ++channel)
                {
                    DoApplyWavelet(channels[channel], waveletParameters);
                }
                SaveWavelet(bitmap, waveletParameters, channels);
                return bitmap.Create(ToColors(channels));
            }
            var bytes = bitmap.GetBytes();
            var channl = GetChannel(bytes, bitmap.PixelHeight, bitmap.PixelWidth);
            DoApplyWavelet(channl, waveletParameters);
            SaveWavelet(bitmap, waveletParameters, new Vector<double[,]>(new[] {channl}));
            return bitmap.Create(ToGrey(channl));
        }

        public static BitmapSource OpenWavelet(string path)
        {
            using (var inputStream = File.OpenRead(path))
            using (var decodeStream = new LzmaDecodeStream(inputStream))
            using (var reader = new BinaryReader(decodeStream))
            {
                var pixelHeight = reader.ReadInt32();
                var pixelWidth = reader.ReadInt32();
                var dpiX = reader.ReadDouble();
                var dpiY = reader.ReadDouble();
                var waveletType = (WaveletType) reader.ReadByte();
                var iterationsCount = reader.ReadInt32();
                var channelsCount = reader.ReadInt32();
                var channels = new Vector<double[,]>(channelsCount);
                for (var i = 0; i < channelsCount; ++i)
                    channels[i] = DecodeChannel(ReadChannel(reader), waveletType, iterationsCount);
                var pixels = GetBytes(channels);
                return BitmapSource.Create(pixelWidth, pixelHeight, dpiX, dpiY, channelsCount == 1 ? PixelFormats.Gray8 : PixelFormats.Bgr32, null, pixels, pixels.Length / pixelHeight);
            }
        }

        private static void SaveWavelet(BitmapSource bitmap, WaveletParameters parameters, Vector<double[,]> channels)
        {
            using (var outputStream = File.Open(parameters.SavePath, FileMode.Create))
            using (var encodeStream = new LzmaEncodeStream(outputStream))
            using (var writer = new BinaryWriter(encodeStream))
            {
                writer.Write(bitmap.PixelHeight);
                writer.Write(bitmap.PixelWidth);
                writer.Write(bitmap.DpiX);
                writer.Write(bitmap.DpiY);
                writer.Write((byte)parameters.WaveletType);
                writer.Write(parameters.IterationsCount);
                writer.Write(channels.Length);
                for (var i = 0; i < channels.Length; ++i)
                    WriteChannel(writer, channels[i]);
            }
        }

        private static byte[] GetBytes(Vector<double[,]> channels)
        {
            var n = channels[0].GetLength(0);
            var m = channels[0].GetLength(1);
            var result = new byte[n*m*(channels.Length == 1 ? 1 : 4)];
            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < m; ++j)
                {
                    if (channels.Length == 1)
                        result[i*m + j] = ToByte(channels[0][i, j]);
                    else
                    {
                        var b = (i*m + j)*4;
                        result[b] = ToByte(channels[2][i, j]);
                        result[b + 1] = ToByte(channels[1][i, j]);
                        result[b + 2] = ToByte(channels[0][i, j]);
                        result[b + 3] = 255;
                    }
                }
            }
            return result;
        }

        private static double[,] ReadChannel(BinaryReader reader)
        {
            var n = reader.ReadInt32();
            var m = reader.ReadInt32();
            var channel = new double[n, m];
            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < m; ++j)
                {
                    channel[i, j] = reader.ReadInt32();
                }
            }
            return channel;
        }

        private static void WriteChannel(BinaryWriter writer, double[,] channel)
        {
            var n = channel.GetLength(0);
            var m = channel.GetLength(1);
            writer.Write(n);
            writer.Write(m);
            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < m; ++j)
                {
                    var intValue = Convert.ToInt32(channel[i, j]);
                    writer.Write(intValue);
                }
            }
        }

        private static Vector<byte>[] ToColors(Vector<double[,]> channels)
        {
            var n = channels[0].GetLength(0);
            var m = channels[0].GetLength(1);
            var result = new Vector<byte>[n * m];
            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < m; ++j)
                {
                    result[i * m + j] = new Vector<byte>(ConvertToBytes(new []{channels[0][i, j], channels[1][i, j], channels[2][i, j]}));
                    //result[i * m + j] = Transformation.VectorYCbCrToRGB(new Vector<byte>(ConvertToBytes(new []{channels[0][i, j], channels[1][i, j], channels[2][i, j]})));
                }
            }
            return result;
        }

        private static byte[] ToGrey(double[,] channel)
        {
            var n = channel.GetLength(0);
            var m = channel.GetLength(1);
            var result = new byte[n * m];
            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < m; ++j)
                {
                    result[i*m + j] = ToByte(channel[i, j]);
                }
            }
            return result;
        }

        private static byte[] ConvertToBytes(double[] doubles)
        {
            return doubles.Select(ToByte).ToArray();
        }

        private static byte ToByte(double d)
        {
            return Convert.ToByte(Math.Max(0.0, Math.Min(255.0, Math.Abs(d))));
        }

        private static void DoApplyWavelet(double[,] channel, WaveletParameters waveletParameters)
        {
            var lowerCoefs = GetCoefsByWaveletType(waveletParameters.WaveletType);
            var higherCoefs = GetHighPassFilter(lowerCoefs);
            var height = channel.GetLength(0);
            var width = channel.GetLength(1);
            var result = new double[height, width];
            for (var counter = 0; counter < waveletParameters.IterationsCount; ++counter)
            {
                var line = new double[width];
                for (var i = 0; i < height; ++i)
                {
                    for (var j = 0; j < width; ++j)
                        line[j] = channel[i, j];
                    var resultLine = PairConvolution(line, lowerCoefs, higherCoefs);
                    for (var j = 0; j < width; ++j)
                        result[i, j] = resultLine[j];
                }
                line = new double[height];
                for (var j = 0; j < width; ++j)
                {
                    for (var i = 0; i < height; ++i)
                        line[i] = result[i, j];
                    var resultLine = PairConvolution(line, lowerCoefs, higherCoefs);
                    for (var i = 0; i < height; ++i)
                        result[i, j] = resultLine[i];
                }
                for (var i = 0; i < height; ++i)
                {
                    for (var j = 0; j < width; ++j)
                    {
                        channel[i%2 == 0 ? i/2 : height/2 + i/2, j%2 == 0 ? j/2 : width/2 + j/2] = result[i, j];
                    }
                }
                height >>= 1;
                width >>= 1;
            }
            height = channel.GetLength(0);
            width = channel.GetLength(1);
            for (var i = 0; i < height; ++i)
                for (var j = 0; j < width; ++j)
                    channel[i, j] = Math.Abs(channel[i, j]) < waveletParameters.Threshold ? 0.0 : channel[i, j]*255.0;
        }

        private static double[,] DecodeChannel(double[,] channel, WaveletType waveletType, int iterationsCount)
        {
            var lowerCoefs = GetCoefsByWaveletType(waveletType);
            var higherCoefs = GetHighPassFilter(lowerCoefs);
            double[] iLowerCoefs, iHigherCoefs;
            GetInverseCoefs(lowerCoefs, higherCoefs, out iLowerCoefs, out iHigherCoefs);

            var height = channel.GetLength(0);
            var width = channel.GetLength(1);
            var orderedChannel = new double[height, width];

            height >>= iterationsCount - 1;
            width >>= iterationsCount - 1;

            for (var counter = 0; counter < iterationsCount; ++counter)
            {
                for (var i = 0; i < height; ++i)
                {
                    for (var j = 0; j < width; ++j)
                    {
                        orderedChannel[i, j] = channel[i%2 == 0 ? i/2 : height/2 + i/2, j%2 == 0 ? j/2 : width/2 + j/2];
                    }
                }
                var line = new double[height];
                for (var j = 0; j < width; ++j)
                {
                    for (var i = 0; i < height; ++i)
                        line[i] = orderedChannel[i, j];
                    var resultLine = PairConvolution(line, iLowerCoefs, iHigherCoefs, iLowerCoefs.Length - 2);
                    for (var i = 0; i < height; ++i)
                        channel[i, j] = resultLine[i];
                }
                line = new double[width];
                for (var i = 0; i < height; ++i)
                {
                    for (var j = 0; j < width; ++j)
                        line[j] = channel[i, j];
                    var resultLine = PairConvolution(line, iLowerCoefs, iHigherCoefs, iLowerCoefs.Length - 2);
                    for (var j = 0; j < width; ++j)
                        channel[i, j] = resultLine[j];
                }

                height <<= 1;
                width <<= 1;
            }
            return channel;
        }

        private static double[] GetCoefsByWaveletType(WaveletType waveletType)
        {
            switch (waveletType)
            {
                case WaveletType.Haar1:
                    return new[]
                    {
                        1.0/2,
                        1.0/2,
                    };
                case WaveletType.Haar2:
                    return new[]
                    {
                        1.0/Math.Sqrt(2.0),
                        1.0/Math.Sqrt(2.0)
                    };
                case WaveletType.D4:
                    return new[]
                    {
                        (1 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                        (3 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                        (3 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                        (1 - Math.Sqrt(3))/(4*Math.Sqrt(2))
                    };
                case WaveletType.D6:
                    return new[]
                    {
                        0.47046721 / Math.Sqrt(2),
                        1.14111692 / Math.Sqrt(2),
                        0.650365 / Math.Sqrt(2),
                        -0.19093442 / Math.Sqrt(2),
                        -0.12083221 / Math.Sqrt(2),
                        0.0498175 / Math.Sqrt(2),
                    };
                default:
                    throw new ArgumentOutOfRangeException("waveletType", waveletType, null);
            }
        }

        private static Vector<double[,]> GetChannels(Vector<byte>[] colors, int height, int width)
        {
            var channels = new Vector<double[,]>(Enumerable.Range(0, 3).Select(x => new double[height, width]));
            for (var i = 0; i < height; ++i)
                for (var j = 0; j < width; ++j)
                    for (var k = 0; k < 3; ++k)
                        channels[k][i, j] = colors[i*width + j][k] / 255.0;
            return channels;
        }

        private static double[,] GetChannel(byte[] colors, int height, int width)
        {
            var channel = new double[height, width];
            for (var i = 0; i < height; ++i)
                for (var j = 0; j < width; ++j)
                    channel[i, j] = colors[i * width + j] / 255.0;
            return channel;
        }

        private static double[] GetHighPassFilter(double[] lowPassFilter)
        {
            var result = new double[lowPassFilter.Length];
            var deg = 1;
            for (var i = 0; i < lowPassFilter.Length; ++i)
            {
                result[i] = lowPassFilter[lowPassFilter.Length - 1 - i]*deg;
                deg *= -1;
            }
            return result;
        }

        private static double[] PairConvolution(double[] data, double[] lowerCoefs, double[] higherCoefs, int delta = 0)
        {
            if (lowerCoefs.Length != higherCoefs.Length)
                throw new Exception("Invalid program state");
            var n = lowerCoefs.Length;
            var m = data.Length;
            var result = new double[m];
            for (var i = 0; i < m; i += 2)
            {
                var sumLower = 0.0;
                var sumHigher = 0.0;
                for (var k = 0; k < n; ++k)
                {
                    sumLower += data[(i + k - delta + m)%m]*lowerCoefs[k];
                    sumHigher += data[(i + k - delta + m)%m]*higherCoefs[k];
                }
                result[i] = sumLower;
                result[i + 1] = sumHigher;
            }
            return result;
        }

        private static void GetInverseCoefs(double[] lowerCoefs, double[] higherCoefs, out double[] iLowerCoefs, out double[] iHigherCoefs)
        {
            if (lowerCoefs.Length != higherCoefs.Length)
                throw new Exception("Invalid program state");
            var n = lowerCoefs.Length;
            iLowerCoefs = new double[n];
            iHigherCoefs = new double[n];
            for (var i = 0; i < n; i += 2)
            {
                iLowerCoefs[i] = lowerCoefs[(i - 2 + n)%n];
                iLowerCoefs[i + 1] = higherCoefs[(i - 2 + n)%n];
                iHigherCoefs[i] = lowerCoefs[(i - 1 + n)%n];
                iHigherCoefs[i + 1] = higherCoefs[(i - 1 + n)%n];
            }
        }
    }
}
