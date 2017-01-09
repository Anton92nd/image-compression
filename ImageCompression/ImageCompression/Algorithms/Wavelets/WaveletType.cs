using ImageCompression.Attributes;

namespace ImageCompression.Algorithms.Wavelets
{
    public enum WaveletType
    {
        [Text("Haar 0.5")]
        Haar1,

        [Text("Haar 1/sqrt(2)")]
        Haar2,

        [Text("Daubechies 4")]
        D4,

        [Text("Daubechies 6")]
        D6,
    }
}
