namespace ImageCompression.Algorithms.MedianCutting
{
    public class MCNode
    {
        public MCNode(Cube cube)
        {
            Cube = cube;
            L = R = null;
        }

        public override string ToString()
        {
            return Cube.ToString();
        }

        public readonly Cube Cube;
        public MCNode L, R;
    }
}
