using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Structures;
using JetBrains.Annotations;

namespace ImageCompression.Extensions
{
    public static class BitmapImageExtensions
    {
        [NotNull]
        public static byte[] GetBytes([NotNull] this BitmapSource bitmap)
        {
            var stride = bitmap.PixelWidth*(bitmap.Format.BitsPerPixel/8);
            var result = new byte[bitmap.PixelHeight*stride];
            bitmap.CopyPixels(result, stride, 0);
            return result;
        }

        [NotNull]
        public static Vector[] GetColors([NotNull] this BitmapSource bitmap)
        {
            if (bitmap.Format != PixelFormats.Bgr32)
                throw new NotSupportedException(string.Format("Getting RGB colors for '{0}' is not supported", bitmap.Format));
            var bytes = bitmap.GetBytes();
            var result = new Vector[bytes.Length/4];
            for (var i = 0; i < bytes.Length; i += 4)
            {
                result[i/4] = new Vector(new []{bytes[i], bytes[i + 1], bytes[i + 2]});
            }
            return result;
        }

        public static BitmapSource Create([NotNull] this BitmapSource bitmap, [NotNull] byte[] bytes)
        {
            return BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format,
                null, bytes, bitmap.PixelWidth*(bitmap.Format.BitsPerPixel/8));
        }

        public static BitmapSource Create([NotNull] this BitmapSource bitmap, [NotNull] Vector[] colors)
        {
            if (bitmap.Format != PixelFormats.Bgr32)
                throw new NotSupportedException(string.Format("Creating bitmap source for '{0}' is not supported", bitmap.Format));
            var bytes = colors.SelectMany(c => new byte[] {c[0], c[1], c[2], 255}).ToArray();
            return BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format,
                null, bytes, bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8));
        }
    }
}