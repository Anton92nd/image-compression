using ImageCompression.Attributes;

namespace ImageCompression
{
    public enum EffectType
    {
        [Text("Grayscale (Mean)")]
        GrayscaleBad,

        [Text("Grayscale (CCIR 601-1)")]
        GrayscaleGood,

        [Text("Grayscale Y")]
        Y,

        [Text("Grayscale Cb")]
        Cb,

        [Text("Grayscale Cr")]
        Cr,

        [Text("To [Y, Cb, Cr] and back")]
        ToYCbCrAndBack,

        [Text("Median cut")]
        [Parameter("Palette size:", 1024)]
        MedianCut,

        [Text("Quantization [R, G, B]")]
        [Parameter("Quantization bits:", "3x4x3")]
        QuantizationRgb,

        [Text("Quantization [Y, Cb, Cr]")]
        [Parameter("Quantization bits:", "2x4x4")]
        QuantizationYCrCb,

        [Text("Linde-Buzo-Gray algorithm")]
        [Parameter("Palette size:", 256)]
        LindeBuzoGray,

        [Text("Discrete cosine transform")]
        Dct,

        [Text("Wavelet compression")]
        Wavelet,
    }
}
