namespace ImageCompression.Algorithms.MedianCutting
{
    public class MCNode
    {
        public MCNode(Cube cube)
        {
            Cube = cube;
            L = R = null;
        }

        public readonly Cube Cube;
        public MCNode L, R;
    }
}
