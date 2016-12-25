namespace ImageCompression.Algorithms.DiscreteCosineTransform
{
    public class DctParameters
    {
        public DecimationType DecimationType { get; set; }

        public QuantizationType QuantizationType { get; set; }

        public int? N { get; set; }

        public MatrixGenerators GeneratorsY { get; set; }

        public MatrixGenerators GeneratorsC { get; set; }
    }
}
