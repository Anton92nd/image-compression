using System.Collections.Generic;
using System.Linq;

namespace ImageCompression.Structures
{
    public class Vector<T>
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

        public override bool Equals(object obj)
        {
            if (!(obj is Vector<T>))
            {
                return false;
            }
            var other = (Vector<T>) obj;
            if (Length != other.Length)
                return false;
            for (var i = 0; i < Length; ++i)
                if (!elements[i].Equals(other.elements[i]))
                    return false;
            return true;
        }

        public override int GetHashCode()
        {
            return elements.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}", string.Join(", ", elements));
        }

        private readonly T[] elements;
    }
}
