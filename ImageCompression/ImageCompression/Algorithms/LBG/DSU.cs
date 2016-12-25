using System.Linq;

namespace ImageCompression.Algorithms.LBG
{
    public class DSU
    {
        public DSU(int size)
        {
            parent = Enumerable.Range(0, size).ToArray();
            rank = new int[size];
        }

        public void Link(int x, int y)
        {
            x = Get(x);
            y = Get(y);
            if (rank[x] == rank[y])
            {
                parent[x] = y;
                rank[y]++;
            }
            else
            {
                if (rank[x] > rank[y])
                    parent[y] = x;
                else
                    parent[x] = y;
            }
        }

        public int Get(int x)
        {
            return parent[x] == x ? x : parent[x] = Get(parent[x]);
        }

        private readonly int[] parent;
        private readonly int[] rank;
    }
}
