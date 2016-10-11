using System.Collections.Generic;
using System.Linq;

namespace ImageCompression.Structures
{
    public struct Vector
    {
        public Vector(int size)
        {
            bytes = new byte[size];
        }

        public Vector(IEnumerable<byte> bytes)
        {
            this.bytes = bytes.ToArray();
        }

        public byte this[int index]
        {
            get { return bytes[index]; }
        }

        private readonly byte[] bytes;
    }
}
