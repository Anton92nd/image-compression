using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Extensions;
using ImageCompression.Structures;

namespace ImageCompression.Algorithms.Wavelets
{
    public static class Wavelet
    {
        public static BitmapSource ApplyWavelet(BitmapSource bitmap, WaveletType waveletType, int count)
        {
            if (bitmap.Format != PixelFormats.Gray8)
            {
                var colors = bitmap.GetColors();
                //var yCrCb = Transformation.RGBToYCbCr(colors);
                var channels = GetChannels(colors, bitmap.PixelHeight, bitmap.PixelWidth);
                for (var channel = 0; channel < 3; ++channel)
                {
                    DoApplyWavelet(channels[channel], waveletType, count);
                }
                return bitmap.Create(ToColors(channels));
            }
            var bytes = bitmap.GetBytes();
            var channl = GetChannel(bytes, bitmap.PixelHeight, bitmap.PixelWidth);
            DoApplyWavelet(channl, waveletType, count);
            return bitmap.Create(ToGrey(channl));
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
            return Convert.ToByte(Math.Max(0.0, Math.Min(255.0, d * 255.0)));
        }

        private static void DoApplyWavelet(double[,] channel, WaveletType waveletType, int count)
        {
            var lowerCoefs = GetCoefsByWaveletType(waveletType);
            var higherCoefs = GetHighPassFilter(lowerCoefs);
            var height = channel.GetLength(0);
            var width = channel.GetLength(1);
            var result = new double[height, width];
            for (var counter = 0; counter < count; ++counter)
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
                    for (var j = 0; j < width; j += 2)
                    {
                        channel[i%2 == 0 ? i/2 : height/2 + i/2, j/2] = result[i, j];
                    }
                }
                for (var i = 0; i < height; ++i)
                {
                    for (var j = 1; j < width; j += 2)
                    {
                        channel[i%2 == 0 ? i/2 : height/2 + i/2, width/2 + j/2] = result[i, j];
                    }
                }
                height >>= 1;
                width >>= 1;
            }
        }

        private static double[] GetCoefsByWaveletType(WaveletType waveletType)
        {
            switch (waveletType)
            {
                case WaveletType.Hoar:
                    return new[]
                    {
                        1.0/2,//1.0/Math.Sqrt(2.0),
                        1.0/2,//1.0/Math.Sqrt(2)
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
                    for (var k = 0; k < 3; ++k)
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
            var result = new double[data.Length];
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
