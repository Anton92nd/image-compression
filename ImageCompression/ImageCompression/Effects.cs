using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Extensions;
using ImageCompression.Structures;

namespace ImageCompression
{
    public static class Effects
    {
        public static BitmapSource ApplyMonochromeGood(BitmapSource bitmap)
        {
            return bitmap.Create(bitmap
                .GetColors()
                .Select(v => (byte)((v[0]*77 + v[1]*150 + v[2]*29) >> 8))
                .Select(c => new Vector(new []{c, c, c}))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeBad(BitmapSource bitmap)
        {
            return bitmap.Create(bitmap
                .GetColors()
                .Select(v => (byte)((v[0]+v[1]+v[2])/3))
                .Select(c => new Vector(new[] { c, c, c }))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeCr(BitmapSource bitmap)
        {
            return bitmap.Create(TransformYCbCr(bitmap.GetColors())
                .Select(v => new Vector(new[] { v[2], v[2], v[2] }))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeCb(BitmapSource bitmap)
        {
            return bitmap.Create(TransformYCbCr(bitmap.GetColors())
                .Select(v => new Vector(new[] { v[1], v[1], v[1] }))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeY(BitmapSource bitmap)
        {
            return bitmap.Create(TransformYCbCr(bitmap.GetColors())
                .Select(v => new Vector(new[] {v[0], v[0], v[0]}))
                .ToArray());
        }

        private static Vector[] TransformYCbCr(Vector[] colors)
        {
            var result = new Vector[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var y = (byte) ((77*colors[i][0] + 150*colors[i][1] + 29*colors[i][2]) >> 8);
                var cb = (byte) (((-43*colors[i][0] - 85*colors[i][1] + 128*colors[i][2]) >> 8) + (1 << 7));
                var cr = (byte) ((((1 << 7)*colors[i][0] - 107*colors[i][1] - 21*colors[i][2]) >> 8) + (1 << 7));
                result[i] = new Vector(new[] {y, cb, cr});
            }
            return result;
        }

        public static readonly Dictionary<EffectType, Func<BitmapSource, BitmapSource>> EffectByType = new Dictionary
            <EffectType, Func<BitmapSource, BitmapSource>>
        {
            {EffectType.GrayscaleBad, ApplyMonochromeBad},
            {EffectType.GrayscaleGood, ApplyMonochromeGood},
            {EffectType.Y, ApplyMonochromeY},
            {EffectType.Cb, ApplyMonochromeCb},
            {EffectType.Cr, ApplyMonochromeCr},
        };

        public static bool CanApply(BitmapSource bitmap, EffectType effectType)
        {
            return SupportedFormats.ContainsKey(effectType) &&
                   SupportedFormats[effectType].ToList().Contains(bitmap.Format) &&
                   EffectByType.ContainsKey(effectType);
        }

        private static readonly Dictionary<EffectType, PixelFormat[]> SupportedFormats = new Dictionary
            <EffectType, PixelFormat[]>
        {
            {EffectType.GrayscaleBad, new[] {PixelFormats.Bgr32}},
            {EffectType.GrayscaleGood, new[] {PixelFormats.Bgr32}},
            {EffectType.Y, new[] {PixelFormats.Bgr32}},
            {EffectType.Cb, new[] {PixelFormats.Bgr32}},
            {EffectType.Cr, new[] {PixelFormats.Bgr32}},
        };
    }
}