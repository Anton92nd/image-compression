using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Algorithms.MedianCutting;
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
                .Select(c => new Vector<byte>(new []{c, c, c}))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeBad(BitmapSource bitmap)
        {
            return bitmap.Create(bitmap
                .GetColors()
                .Select(v => (byte)((v[0]+v[1]+v[2])/3))
                .Select(c => new Vector<byte>(new[] { c, c, c }))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeCr(BitmapSource bitmap)
        {
            return bitmap.Create(TransformRGBToYCbCr(bitmap.GetColors())
                .Select(v => new Vector<byte>(new[] { v[2], v[2], v[2] }))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeCb(BitmapSource bitmap)
        {
            return bitmap.Create(TransformRGBToYCbCr(bitmap.GetColors())
                .Select(v => new Vector<byte>(new[] { v[1], v[1], v[1] }))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeY(BitmapSource bitmap)
        {
            return bitmap.Create(TransformRGBToYCbCr(bitmap.GetColors())
                .Select(v => new Vector<byte>(new[] {v[0], v[0], v[0]}))
                .ToArray());
        }

        public static BitmapSource ApplyToYCbCrAndBack(BitmapSource bitmap)
        {
            var sourceColors = bitmap.GetColors();
            var transformed = TransformRGBToYCbCr(sourceColors);
            var back = TransformYCbCrToRGB(transformed);
            return bitmap.Create(back);
        }

        public static BitmapSource ApplyMedianCut(BitmapSource bitmap, object parameter)
        {
            var paletteSize = (int)parameter;
            Vector<byte>[] palette;
            return bitmap.Create(MedianCut.Build(paletteSize, bitmap.GetColors(), out palette));
        }

        public static BitmapSource QuantizeInRgb(BitmapSource bitmap, object parameter)
        {
            var quantizationVector = parameter as Vector<byte>;
            var colors = bitmap.GetColors().Select(c => QuantizeVector(c, quantizationVector)).ToArray();
            return bitmap.Create(colors);
        }

        public static BitmapSource QuantizeInYCbCr(BitmapSource bitmap, object parameter)
        {
            var quantizationVector = parameter as Vector<byte>;
            var colors = TransformYCbCrToRGB(
                TransformRGBToYCbCr(bitmap.GetColors())
                .Select(c => QuantizeVector(c, quantizationVector)).ToArray()
                );
            return bitmap.Create(colors);
        }

        private static Vector<byte> QuantizeVector(Vector<byte> color, Vector<byte> quantizationVector)
        {
            var bytes = new byte[3];
            for (var i = 0; i < 3; i++)
            {
                bytes[i] = QuantizeByte(color[i], quantizationVector[i]);
            }
            return new Vector<byte>(bytes);
        }

        private static byte QuantizeByte(byte value, byte bits)
        {
            return (byte) (((value >> (8 - bits)) << (8 - bits)) + (bits == 8 ? 0 : 1 << (8 - bits - 1)));
        }


        private static Vector<byte>[] TransformRGBToYCbCr(Vector<byte>[] colors)
        {
            var result = new Vector<byte>[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var y = (byte) ((77*colors[i][0] + 150*colors[i][1] + 29*colors[i][2]) >> 8);
                var cb = (byte) (((-43*colors[i][0] - 85*colors[i][1] + 128*colors[i][2]) >> 8) + (1 << 7));
                var cr = (byte) ((((1 << 7)*colors[i][0] - 107*colors[i][1] - 21*colors[i][2]) >> 8) + (1 << 7));
                result[i] = new Vector<byte>(new[] {y, cb, cr});
            }
            return result;
        }

        private static Vector<byte>[] TransformYCbCrToRGB(Vector<byte>[] colors)
        {
            var result = new Vector<byte>[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var r = (byte) Math.Max(0, colors[i][0] + ((colors[i][2] - 128) << 8) / 183);
                var g = (byte) Math.Max(0, colors[i][0] - (5329 * (colors[i][1] - 128) + 11103 * (colors[i][2] - 128)) / 15481);
                var b = (byte) Math.Max(0, colors[i][0] + ((colors[i][1] - 128) << 8) / 144);
                result[i] = new Vector<byte>(new[] { r, g, b });
            }
            return result;
        }


        public static readonly Dictionary<EffectType, Func<BitmapSource, object, BitmapSource>> EffectByType = new Dictionary
            <EffectType, Func<BitmapSource, object, BitmapSource>>
        {
            {EffectType.GrayscaleBad, (b, p) => ApplyMonochromeBad(b)},
            {EffectType.GrayscaleGood, (b, p) => ApplyMonochromeGood(b)},
            {EffectType.Y, (b, p) => ApplyMonochromeY(b)},
            {EffectType.Cb, (b, p) => ApplyMonochromeCb(b)},
            {EffectType.Cr, (b, p) => ApplyMonochromeCr(b)},
            {EffectType.ToYCbCrAndBack, (b, p) => ApplyToYCbCrAndBack(b)},
            {EffectType.MedianCut, (b, p) => ApplyMedianCut(b, p)},
            {EffectType.QuantizationRgb, (b, p) => QuantizeInRgb(b, p)},
            {EffectType.QuantizationYCrCb, (b, p) => QuantizeInYCbCr(b, p)},
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
            {EffectType.ToYCbCrAndBack, new []{PixelFormats.Bgr32}},
            {EffectType.MedianCut, new []{PixelFormats.Bgr32}},
            {EffectType.QuantizationRgb, new []{PixelFormats.Bgr32}},
            {EffectType.QuantizationYCrCb, new []{PixelFormats.Bgr32}},
        };
    }
}