using System;
using System.Collections.Generic;

namespace raytracer
{
    class Point3d
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Point3d(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3d(Point3d p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        static public List<float> MulMatrix(List<float> matr1, int m1, int n1, List<float> matr2, int m2, int n2)
        {
            if (n1 != m2)
            {
                return new List<float>();
            }                
            int l = m1;
            int m = n1;
            int n = n2;
            List<float> c = new List<float>();
            for (int i = 0; i < l * n; ++i)
            {
                c.Add(0f);
            }
            for (int i = 0; i < l; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    for (int r = 0; r < m; ++r)
                        c[i * l + j] += matr1[i * m1 + r] * matr2[r * n2 + j];
                }
            }
            return c;
        }

        public void Translate(float x, float y, float z)
        {
            List<float> T = new List<float> { 1, 0, 0, 0,
                                              0, 1, 0, 0,
                                              0, 0, 1, 0,
                                              x, y, z, 1 };
            List<float> xyz = new List<float> { X, Y, Z, 1 };
            List<float> c = MulMatrix(xyz, 1, 4, T, 4, 4);
            X = c[0];
            Y = c[1];
            Z = c[2];
        }
    }
}
