namespace ImageCompression.Algorithms.MedianCutting
{
    public struct Segment
    {
        public Segment(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Length {get { return End - Start + 1; }}

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", Start, End);
        }

        public readonly int Start, End;
    }
}
