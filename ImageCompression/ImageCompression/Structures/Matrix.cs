using System;

namespace ImageCompression.Structures
{
    public class Matrix
    {
        private readonly double[,] array;

        public Matrix(int n, int m)
        {
            array = new double[n, m];
        }

        public Matrix(double[,] array)
        {
            this.array = array;
        }

        public int N
        {
            get { return array.GetLength(0); }
        }

        public int M
        {
            get { return array.GetLength(1); }
        }

        public double this[int i, int j]
        {
            get { return array[i, j]; }
            set { array[i, j] = value; }
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.M != b.N)
                throw new Exception("Can't multiply matrices");
            var n = a.N;
            var m = b.M;
            var l = a.M;
            var c = new Matrix(n, m);
            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < m; ++j)
                {
                    c[i, j] = 0;
                    for (var k = 0; k < l; ++k)
                        c[i, j] += a[i, k]*b[k, j];
                }
            }
            return c;
        }

        public static Matrix operator/(Matrix a, Matrix b)
        {
            if (a.N != b.N || a.M != b.M)
                throw new Exception("Matrices sizes don't match");
                
            var result = new Matrix(a.N, a.M);
            for (var i = 0; i < a.N; ++i)
                for (var j = 0; j < a.M; ++j)
                    result[i, j] = Math.Round(a[i, j]/b[i, j]);
            return result;
        }

        public static Matrix operator %(Matrix a, Matrix b)
        {
            if (a.N != b.N || a.M != b.M)
                throw new Exception("Matrices sizes don't match");

            var result = new Matrix(a.N, a.M);
            for (var i = 0; i < a.N; ++i)
                for (var j = 0; j < a.M; ++j)
                    result[i, j] = Math.Round(a[i, j] * b[i, j]);
            return result;
        }

        public Matrix Transpose()
        {
            var result = new Matrix(M, N);
            for (var i = 0; i < M; ++i)
                for (var j = 0; j < N; ++j)
                    result[i, j] = array[j, i];
            return result;
        }
    }
}
