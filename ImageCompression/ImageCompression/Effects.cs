using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Algorithms;
using ImageCompression.Algorithms.DiscreteCosineTransform;
using ImageCompression.Algorithms.JPG;
using ImageCompression.Algorithms.LBG;
using ImageCompression.Algorithms.MedianCutting;
using ImageCompression.Algorithms.Wavelets;
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
            return bitmap.Create(Transformation.RGBToYCbCr(bitmap.GetColors())
                .Select(v => new Vector<byte>(new[] { v[2], v[2], v[2] }))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeCb(BitmapSource bitmap)
        {
            return bitmap.Create(Transformation.RGBToYCbCr(bitmap.GetColors())
                .Select(v => new Vector<byte>(new[] { v[1], v[1], v[1] }))
                .ToArray());
        }

        public static BitmapSource ApplyMonochromeY(BitmapSource bitmap)
        {
            return bitmap.Create(Transformation.RGBToYCbCr(bitmap.GetColors())
                .Select(v => new Vector<byte>(new[] {v[0], v[0], v[0]}))
                .ToArray());
        }

        public static BitmapSource ApplyToYCbCrAndBack(BitmapSource bitmap)
        {
            var sourceColors = bitmap.GetColors();
            var transformed = Transformation.RGBToYCbCr(sourceColors);
            var back = Transformation.YCbCrToRGB(transformed);
            return bitmap.Create(back);
        }

        public static BitmapSource ApplyMedianCut(BitmapSource bitmap, object parameter)
        {
            var paletteSize = (int)parameter;
            Vector<byte>[] palette;
            return bitmap.Create(MedianCut.Build(paletteSize, bitmap.GetColors(), out palette));
        }

        public static BitmapSource ApplyLindeBuzoGray(BitmapSource bitmap, object parameter)
        {
            var paletteSize = (int)parameter;
            Vector<byte>[] palette;
            return bitmap.Create(LbgAlgorithm.Build(paletteSize, bitmap.GetColors(), out palette));
        }

        public static BitmapSource ApplyDiscreteCosineTransform(BitmapSource bitmap, object parameter)
        {
            var dctParameters = (DctParameters) parameter;
            var encoded = DctAlgorithm.ApplyDct(bitmap, dctParameters);
            Jpeg.Save(encoded, dctParameters);
            return null;
        }

        public static BitmapSource ApplyWaveletCompression(BitmapSource bitmap, object parameter)
        {
            return Wavelet.ApplyWavelet(bitmap, WaveletType.D6, 2);
        }

        public static BitmapSource QuantizeInRgb(BitmapSource bitmap, object parameter)
        {
            var quantizationVector = parameter as Vector<byte>;
            var colors = bitmap.GetColors().Select(c => Quantization.QuantizeVector(c, quantizationVector)).ToArray();
            return bitmap.Create(colors);
        }

        public static BitmapSource QuantizeInYCbCr(BitmapSource bitmap, object parameter)
        {
            var quantizationVector = parameter as Vector<byte>;
            var colors = Transformation.YCbCrToRGB(
                Transformation.RGBToYCbCr(bitmap.GetColors())
                .Select(c => Quantization.QuantizeVector(c, quantizationVector)).ToArray()
                );
            return bitmap.Create(colors);
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
            {EffectType.LindeBuzoGray, (b, p) => ApplyLindeBuzoGray(b, p)},
            {EffectType.QuantizationRgb, (b, p) => QuantizeInRgb(b, p)},
            {EffectType.QuantizationYCrCb, (b, p) => QuantizeInYCbCr(b, p)},
            {EffectType.Dct, (b, p) => ApplyDiscreteCosineTransform(b, p)},
            {EffectType.Wavelet, (b, p) => ApplyWaveletCompression(b, p)},
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
            {EffectType.LindeBuzoGray, new []{PixelFormats.Bgr32}},
            {EffectType.QuantizationRgb, new []{PixelFormats.Bgr32}},
            {EffectType.QuantizationYCrCb, new []{PixelFormats.Bgr32}},
            {EffectType.Dct, new []{PixelFormats.Bgr32, PixelFormats.Bgra32}},
            {EffectType.Wavelet, new []{PixelFormats.Bgr32, PixelFormats.Bgra32, PixelFormats.Gray8, }},
        };
    }
}