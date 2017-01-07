using ImageCompression.Attributes;

namespace ImageCompression.Algorithms.Wavelets
{
    public enum WaveletType
    {
        [Text("Hoar")]
        Hoar,

        [Text("Daubechies 4")]
        D4,

        [Text("Daubechies 6")]
        D6,
    }
}
