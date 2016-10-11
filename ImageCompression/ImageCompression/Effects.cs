using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Extensions;

namespace ImageCompression
{
    public static class Effects
    {
        public static BitmapSource ApplyMonochromeGood(BitmapSource bitmap)
        {
            var bytes = bitmap.GetBytes();
            for (var i = 0; i < bytes.Length; i += 4)
            {
                var middle = (77 * bytes[i] + 150 * bytes[i + 1] + 29 * bytes[i + 2]) >> 8;
                bytes[i] = bytes[i + 1] = bytes[i + 2] = (byte)middle;
            }
            return CreateBitmap(bitmap, bytes);
        }

        public static BitmapSource ApplyMonochromeBad(BitmapSource bitmap)
        {
            var bytes = bitmap.GetBytes();
            for (var i = 0; i < bytes.Length; i += 4)
            {
                var middle = (bytes[i] + bytes[i + 1] + bytes[i + 2]) / 3;
                bytes[i] = bytes[i + 1] = bytes[i + 2] = (byte)middle;
            }
            return CreateBitmap(bitmap, bytes);
        }

        private static BitmapSource CreateBitmap(BitmapSource bitmap, byte[] bytes)
        {
            return BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format,
                null, bytes, bitmap.PixelWidth*(bitmap.Format.BitsPerPixel/8));
        }

        public static BitmapSource ApplyMonochromeCr(BitmapSource bitmap)
        {
            var bytes = bitmap.GetBytes();
            var yCbCr = TransformYCbCr(bytes);
            for (var i = 0; i < bytes.Length; i += 4)
            {
                bytes[i] = bytes[i + 1] = bytes[i + 2] = yCbCr[i / 4].Item3;
            }
            return CreateBitmap(bitmap, bytes);
        }

        public static BitmapSource ApplyMonochromeCb(BitmapSource bitmap)
        {
            var bytes = bitmap.GetBytes();
            var yCbCr = TransformYCbCr(bytes);
            for (var i = 0; i < bytes.Length; i += 4)
            {
                bytes[i] = bytes[i + 1] = bytes[i + 2] = yCbCr[i / 4].Item2;
            }
            return CreateBitmap(bitmap, bytes);
        }

        public static BitmapSource ApplyMonochromeY(BitmapSource bitmap)
        {
            var bytes = bitmap.GetBytes();
            var yCbCr = TransformYCbCr(bytes);
            for (var i = 0; i < bytes.Length; i += 4)
            {
                bytes[i] = bytes[i + 1] = bytes[i + 2] = yCbCr[i / 4].Item1;
            }
            return CreateBitmap(bitmap, bytes);
        }

        private static Tuple<byte, byte, byte>[] TransformYCbCr(byte[] bytes)
        {
            var result = new Tuple<byte, byte, byte>[bytes.Length/4];
            for (var i = 0; i < bytes.Length; i += 4)
            {
                var y = (byte)((77*bytes[i] + 150*bytes[i + 1] + 29*bytes[i + 2]) >> 8);
                var cb = (byte) (((-43*bytes[i] - 85*bytes[i + 1] + 128*bytes[i + 2]) >> 8) + (1 << 7));
                var cr = (byte)((((1 << 7)*bytes[i] - 107*bytes[i + 1] - 21*bytes[i + 2]) >> 8) + (1 << 7));
                result[i / 4] = new Tuple<byte, byte, byte>(y, cb, cr);
            }
            return result;
        }

        public static readonly Dictionary<EffectType, Func<BitmapSource, BitmapSource>> EffectByType = new Dictionary<EffectType, Func<BitmapSource, BitmapSource>>
        {
            {EffectType.GrayscaleBad, ApplyMonochromeBad},
            {EffectType.GrayscaleGood, ApplyMonochromeGood},
            {EffectType.Y, ApplyMonochromeY},
            {EffectType.Cb, ApplyMonochromeCb},
            {EffectType.Cr, ApplyMonochromeCr},
        };

        public static bool CanApply(BitmapSource bitmap, EffectType effectType)
        {
            return SupportedFormats.ContainsKey(effectType) && SupportedFormats[effectType].ToList().Contains(bitmap.Format) &&
                   EffectByType.ContainsKey(effectType);
        }

        private static readonly Dictionary<EffectType, PixelFormat[]> SupportedFormats = new Dictionary<EffectType, PixelFormat[]>
        {
            {EffectType.GrayscaleBad, new []{PixelFormats.Bgr32}},
            {EffectType.GrayscaleGood, new []{PixelFormats.Bgr32}},
            {EffectType.Y, new []{PixelFormats.Bgr32}},
            {EffectType.Cb, new []{PixelFormats.Bgr32}},
            {EffectType.Cr, new []{PixelFormats.Bgr32}},
        };
    }
}
