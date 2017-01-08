using ImageCompression.Attributes;

namespace ImageCompression.Algorithms.Wavelets
{
    public enum WaveletType
    {
        [Text("Hoar 0.5")]
        Hoar1,

        [Text("Hoar 1/sqrt(2)")]
        Hoar2,

        [Text("Daubechies 4")]
        D4,

        [Text("Daubechies 6")]
        D6,
    }
}
