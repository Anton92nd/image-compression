using System;
using System.Collections.Generic;

namespace ImageCompression.Structures
{
    public class PersistentStack<T>
    {
        public PersistentStack()
        {
            stack = new List<T>();
            index = 0;
        }

        public void Add(T item)
        {
            while (stack.Count > index)
                stack.RemoveAt(stack.Count - 1);
            stack.Add(item);
            index++;
        }

        public T Current()
        {
            if (IsEmpty())
                throw new Exception("Current on empty stack");
            return stack[index - 1];
        }

        public void Pop()
        {
            if (index == 0)
                throw new Exception("Pop from empty stack");
            index--;
        }

        public void Unpop()
        {
            if (index == stack.Count)
                throw new Exception("Unpop from stack head");
            index++;
        }

        public bool IsAtHead()
        {
            return index == stack.Count;
        }

        public bool IsEmpty()
        {
            return index == 0;
        }

        public void Clear()
        {
            stack.Clear();
            index = 0;
        }

        public int Count {get { return index; }}

        private int index;
        private readonly List<T> stack;
    }
}
