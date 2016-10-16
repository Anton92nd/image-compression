using System.Diagnostics.Contracts;
using ImageCompression.Structures;

namespace ImageCompression.Algorithms.MedianCutting
{
    public struct Cube
    {
        public Cube(Segment ox, Segment oy, Segment oz)
        {
            Ox = ox;
            Oy = oy;
            Oz = oz;
        }

        [Pure]
        public bool Contains(Vector<byte> color)
        {
            return Ox.Start <= color[0] && color[0] <= Ox.End &&
                   Oy.Start <= color[1] && color[1] <= Oy.End &&
                   Oz.Start <= color[2] && color[2] <= Oz.End;
        }

        public override string ToString()
        {
            return string.Format("<{0}, {1}, {2}>", Ox, Oy, Oz);
        }

        public readonly Segment Ox, Oy, Oz;
    }
}
