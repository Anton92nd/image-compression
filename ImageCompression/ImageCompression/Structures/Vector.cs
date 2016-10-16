using System.Collections.Generic;
using System.Linq;

namespace ImageCompression.Structures
{
    public struct Vector<T>
    {
        public Vector(int size)
        {
            elements = new T[size];
        }

        public Vector(IEnumerable<T> elements)
        {
            this.elements = elements.ToArray();
        }

        public T this[int index]
        {
            get { return elements[index]; }
        }

        public int Length
        {
            get { return elements.Length; }
        }

        private readonly T[] elements;
    }
}
