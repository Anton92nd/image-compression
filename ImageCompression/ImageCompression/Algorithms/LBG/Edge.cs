using System;

namespace ImageCompression.Algorithms.LBG
{
    public struct Edge : IComparable<Edge>
    {
        public readonly int U, V, Length;

        public Edge(int u, int v, int length)
        {
            U = u;
            V = v;
            Length = length;
        }

        public int CompareTo(Edge other)
        {
            return Length.CompareTo(other.Length);
        }
    }
}
