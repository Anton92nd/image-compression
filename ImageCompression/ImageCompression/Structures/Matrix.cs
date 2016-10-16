using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageCompression.Structures
{
    public class Matrix<T>
    {
        public Matrix(int n, int m)
        {
            matrix = new T[n, m];
        }

        public Matrix(T[,] array)
        {
            this.matrix = array;
        }

        public Matrix(IList<Vector<T>> vectors)
        {
            matrix = new T[vectors.Count, vectors[0].Length];
            for (var i = 0; i < vectors.Count; ++i)
            {
                for (var j = 0; j < vectors[0].Length; ++j)
                {
                    matrix[i, j] = vectors[i][j];
                }
            }
        }

        public int N {get { return matrix.GetLength(0); }}
        public int M {get { return matrix.GetLength(1); }}

        public T this[int i, int j]
        {
            get { return matrix[i, j]; }
        }

        public static Matrix<T> Multiply(Matrix<T> A, Matrix<T> B, Func<T, T, T> sum, Func<T, T, T> multiply)
        {
            if (A.M != B.N)
                throw new Exception("Can't multiply matrices with different dimensions");
            var result = new Matrix<T>(A.N, B.M);
            for (var i = 0; i < A.N; ++i)
            {
                for (var j = 0; j < B.M; ++j)
                {
                    result.matrix[i, j] = Enumerable.Range(0, A.M)
                        .Select(k => multiply(A[i, k], B[k, j]))
                        .Aggregate(sum);
                }
            }
            return result;
        }

        private readonly T[,] matrix;
    }
}
