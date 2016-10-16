namespace ImageCompression
{
    public enum EffectType
    {
        [Text("Grayscale (bad)")]
        GrayscaleBad,

        [Text("Grayscale (good)")]
        GrayscaleGood,

        [Text("Grayscale Y")]
        Y,

        [Text("Grayscale Cb")]
        Cb,

        [Text("Grayscale Cr")]
        Cr,

        [Text("To [Y, Cb, Cr] and back")]
        ToYCbCrAndBack,
    }
}
