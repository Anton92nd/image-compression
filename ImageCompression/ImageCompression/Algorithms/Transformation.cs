using System;
using System.Linq;
using ImageCompression.Structures;

namespace ImageCompression.Algorithms
{
    public static class Transformation
    {
        public static Vector<byte>[] RGBToYCbCr(Vector<byte>[] colors)
        {
            return colors.Select(VectorRGBToYCbCr).ToArray();
        }

        public static Vector<byte> VectorRGBToYCbCr(Vector<byte> color)
        {
            var y = (byte)Math.Min(255, Math.Max(0, (77 * color[0] + 150 * color[1] + 29 * color[2]) >> 8));
            var cb = (byte)Math.Min(255, Math.Max(0, ((-43 * color[0] - 85 * color[1] + 128 * color[2]) >> 8) + (1 << 7)));
            var cr = (byte)Math.Min(255, Math.Max(0, (((1 << 7) * color[0] - 107 * color[1] - 21 * color[2]) >> 8) + (1 << 7)));
            return new Vector<byte>(new[] { y, cb, cr });
        }

        public static Vector<byte> VectorYCbCrToRGB(Vector<byte> color)
        {
            var r = (byte)Math.Min(255, Math.Max(0, color[0] + ((color[2] - 128) << 8) / 183));
            var g = (byte)Math.Min(255, Math.Max(0, color[0] - (5329 * (color[1] - 128) + 11103 * (color[2] - 128)) / 15481));
            var b = (byte)Math.Min(255, Math.Max(0, color[0] + ((color[1] - 128) << 8) / 144));
            return new Vector<byte>(new[] { r, g, b });
        }

        public static Vector<byte>[] YCbCrToRGB(Vector<byte>[] colors)
        {
            return colors.Select(VectorYCbCrToRGB).ToArray();
        }
    }
}
