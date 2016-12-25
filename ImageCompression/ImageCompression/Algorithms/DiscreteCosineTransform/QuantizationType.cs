using ImageCompression.Attributes;

namespace ImageCompression.Algorithms.DiscreteCosineTransform
{
    public enum QuantizationType
    {
        [Text("Largest N")]
        LargestN,

        [Text("Quantization matrices")]
        QuantizationMatrix,

        [Text("Default JPEG matrices")]
        DefaultJpegMatrix,
    }
}
