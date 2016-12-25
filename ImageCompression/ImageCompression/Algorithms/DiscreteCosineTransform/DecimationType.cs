using ImageCompression.Attributes;

namespace ImageCompression.Algorithms.DiscreteCosineTransform
{
    public enum DecimationType
    {
        [Text("No decimation")]
        NoDecimation = 0,

        [Text("Horizontal")]
        Horizontal = 1,

        [Text("Vertical")]
        Vertical = 2,

        [Text("Horizontal and vertical")]
        HorizontalAndVertical = 4,
    }
}
