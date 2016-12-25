using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Structures;

namespace ImageCompression.Algorithms.DiscreteCosineTransform
{
    public static class BitmapSourceExtensions
    {
        public static Vector<byte>[,] GetMap(this BitmapSource bitmap)
        {
            if (bitmap.Format != PixelFormats.Bgr32)
                throw new NotSupportedException();
            var stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
            var bytes = new byte[bitmap.PixelHeight * stride];
            bitmap.CopyPixels(bytes, stride, 0);
            var result = new Vector<byte>[bitmap.PixelHeight, bitmap.PixelWidth];
            for (var i = 0; i < bitmap.PixelHeight; ++i)
            {
                for (var j = 0; j < bitmap.PixelWidth; ++j)
                {
                    var offset = 4 * (i*bitmap.PixelWidth + j);
                    result[i, j] = new Vector<byte>(new []{bytes[offset], bytes[offset + 1], bytes[offset + 2]});
                }
            }
            return result;
        }
    }
}
