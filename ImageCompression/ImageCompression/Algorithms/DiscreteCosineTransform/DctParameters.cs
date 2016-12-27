namespace ImageCompression.Algorithms.DiscreteCosineTransform
{
    public class DctParameters
    {
        public DecimationType DecimationType { get; set; }

        public QuantizationType QuantizationType { get; set; }

        public int? Ny { get; set; }

        public int? Nc { get; set; }

        public MatrixGenerators GeneratorsY { get; set; }

        public MatrixGenerators GeneratorsC { get; set; }
        
        public string SavePath { get; set; }
    }
}
