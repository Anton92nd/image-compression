using System.Windows.Media.Imaging;
using JetBrains.Annotations;

namespace ImageCompression.Extensions
{
    public static class BitmapImageExtensions
    {
        [NotNull]
        public static byte[] GetBytes([NotNull] this BitmapSource bitmapImage)
        {
            var stride = bitmapImage.PixelWidth*(bitmapImage.Format.BitsPerPixel/8);
            var result = new byte[bitmapImage.PixelHeight*stride];
            bitmapImage.CopyPixels(result, stride, 0);
            return result;
        }
    }
}
