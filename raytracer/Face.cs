using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace raytracer
{
    class Face
    {
        public List<Point3d> Points { get; }
        public Point3d Center { get; set; } = new Point3d(0, 0, 0);
        public List<float> Normal { get; set; }
        public bool IsVisible { get; set; }
        private bool xconst = true;
        private bool yconst = true;
        private bool zconst = true;
        private readonly float eps = 1e-6f;

        public Face(Face face)
        {
            Points = face.Points.Select(pt => new Point3d(pt.X, pt.Y, pt.Z)).ToList();
            Center = new Point3d(face.Center);
            if (Normal != null)
            {
                Normal = new List<float>(face.Normal);
            }
            IsVisible = face.IsVisible;
            xconst = face.xconst;
            yconst = face.yconst;
            zconst = face.zconst;
        }

        public Face(List<Point3d> points = null)
        {
            if (points == null)
            {
                return;
            }
            Points = new List<Point3d>(points);
            FindCenter();
            for (int i = 1; i < Points.Count; ++i)
            {
                if (Points[i].X != Points[i - 1].X)
                {
                    xconst = false;
                    break;
                }
            }
            for (int i = 1; i < Points.Count; ++i)
            {
                if (Points[i].Y != Points[i - 1].Y)
                {
                    yconst = false;
                    break;
                }
            }
            for (int i = 1; i < Points.Count; ++i)
            {
                if (Points[i].Z != Points[i - 1].Z)
                {
                    zconst = false;
                    break;
                }
            }
        }
        
        bool Eq(float d1, float d2)
        {
            return Math.Abs(d1 - d2) < eps;
        }

        bool Less(float d1, float d2)
        {
            return (d1 < d2) && (Math.Abs(d1 - d2) >= eps);
        }

        bool LEq(float b1, float b2)
        {
            return Less(b1, b2) || Eq(b1, b2);
        }

        private int PointBelongs(PointF e1, PointF e2, PointF pt)
        {
            float a = e1.Y - e2.Y;
            float b = e2.X - e1.X;
            float c = e1.X * e2.Y - e2.X * e1.Y;
            if (Math.Abs(a * pt.X + b * pt.Y + c) > eps)
            {
                return -1;
            }
            bool toedge = LEq(Math.Min(e1.X, e2.X), pt.X) && LEq(pt.X, Math.Max(e1.X, e2.X))
                        && LEq(Math.Min(e1.Y, e2.Y), pt.Y) && LEq(pt.Y, Math.Max(e1.Y, e2.Y));
            if (toedge)
            {
                return 1;
            }
            return -1;
        }

        private bool IsCrossed(PointF first1, PointF first2, PointF second1, PointF second2)
        {
            float a1 = first1.Y - first2.Y;
            float b1 = first2.X - first1.X;
            float c1 = first1.X * first2.Y - first2.X * first1.Y;
            float a2 = second1.Y - second2.Y;
            float b2 = second2.X - second1.X;
            float c2 = second1.X * second2.Y - second2.X * second1.Y;
            float zn = a1 * b2 - a2 * b1;
            if (Math.Abs(zn) < eps)
            {
                return false;
            }                
            float x = (-1) * (c1 * b2 - c2 * b1) / zn;
            float y = (-1) * (a1 * c2 - a2 * c1) / zn;
            if (Eq(x, 0))
            {
                x = 0;
            }                
            if (Eq(y, 0))
            {
                y = 0;
            }
            bool tofirst = LEq(Math.Min(first1.X, first2.X), x) && LEq(x, Math.Max(first1.X, first2.X)) && LEq(Math.Min(first1.Y, first2.Y), y) && LEq(y, Math.Max(first1.Y, first2.Y));
            bool tosecond = LEq(Math.Min(second1.X, second2.X), x) && LEq(x, Math.Max(second1.X, second2.X)) && LEq(Math.Min(second1.Y, second2.Y), y) && LEq(y, Math.Max(second1.Y, second2.Y));
            return tofirst && tosecond;
        }

        public bool Inside(Point3d p)
        {
            int cnt = 0;
            if (zconst == true)
            {
                PointF pt = new PointF(p.X, p.Y);
                PointF ray = new PointF(100000, pt.Y);
                for (int i = 1; i <= Points.Count; ++i)
                {
                    PointF tmp1 = new PointF(Points[i - 1].X, Points[i - 1].Y);
                    PointF tmp2 = new PointF(Points[i % Points.Count].X, Points[i % Points.Count].Y);
                    if (PointBelongs(tmp1, tmp2, pt) == 1)
                    {
                        return true;
                    }
                    if (Eq(tmp1.Y, tmp2.Y))
                    {
                        continue;
                    }
                    if (Eq(pt.Y, Math.Min(tmp1.Y, tmp2.Y)))
                    {
                        continue;
                    }
                    if (Eq(pt.Y, Math.Max(tmp1.Y, tmp2.Y)) && Less(pt.X, Math.Min(tmp1.X, tmp2.X)))
                    {
                        ++cnt;
                    }
                    else if (IsCrossed(tmp1, tmp2, pt, ray))
                    {
                        ++cnt;
                    }
                }
                return cnt % 2 == 0 ? false : true;
            }
            else if (yconst == true)
            {
                PointF pt = new PointF(p.X, p.Z);
                PointF ray = new PointF(100000, pt.Y);
                for (int i = 1; i <= Points.Count; ++i)
                {
                    PointF tmp1 = new PointF(Points[i - 1].X, Points[i - 1].Z);
                    PointF tmp2 = new PointF(Points[i % Points.Count].X, Points[i % Points.Count].Z);
                    if (PointBelongs(tmp1, tmp2, pt) == 1)
                    {
                        return true;
                    }
                    if (Eq(tmp1.Y, tmp2.Y))
                    {
                        continue;
                    }
                    if (Eq(pt.Y, Math.Min(tmp1.Y, tmp2.Y)))
                    {
                        continue;
                    }
                    if (Eq(pt.Y, Math.Max(tmp1.Y, tmp2.Y)) && Less(pt.X, Math.Min(tmp1.X, tmp2.X)))
                    {
                        ++cnt;
                    }
                    else if (IsCrossed(tmp1, tmp2, pt, ray))
                    {
                        ++cnt;
                    }
                }
                return cnt % 2 == 0 ? false : true;
            }
            else if (xconst == true)
            {
                PointF pt = new PointF(p.Y, p.Z);
                PointF ray = new PointF(100000, pt.Y);
                for (int i = 1; i <= Points.Count; ++i)
                {
                    PointF tmp1 = new PointF(Points[i - 1].Y, Points[i - 1].Z);
                    PointF tmp2 = new PointF(Points[i % Points.Count].Y, Points[i % Points.Count].Z);
                    if (PointBelongs(tmp1, tmp2, pt) == 1)
                    {
                        return true;
                    }
                    if (Eq(tmp1.Y, tmp2.Y))
                    {
                        continue;
                    }
                    if (Eq(pt.Y, Math.Min(tmp1.Y, tmp2.Y)))
                    {
                        continue;
                    }
                    if (Eq(pt.Y, Math.Max(tmp1.Y, tmp2.Y)) && Less(pt.X, Math.Min(tmp1.X, tmp2.X)))
                    {
                        ++cnt;
                    }
                    else if (IsCrossed(tmp1, tmp2, pt, ray))
                    {
                        ++cnt;
                    }
                }
                return cnt % 2 == 0 ? false : true;
            }
            return false;
        }

        private void FindCenter()
        {
            Center.X = 0;
            Center.Y = 0;
            Center.Z = 0;
            foreach (Point3d p in Points)
            {
                Center.X += p.X;
                Center.Y += p.Y;
                Center.Z += p.Z;
            }
            Center.X /= Points.Count;
            Center.Y /= Points.Count;
            Center.Z /= Points.Count;
        }

        public void FindNormal(Point3d p_center)
        {
            Point3d Q = Points[1], R = Points[2], S = Points[0];
            List<float> QR = new List<float> { R.X - Q.X, R.Y - Q.Y, R.Z - Q.Z };
            List<float> QS = new List<float> { S.X - Q.X, S.Y - Q.Y, S.Z - Q.Z };
            Normal = new List<float> { QR[1] * QS[2] - QR[2] * QS[1],
                                       -(QR[0] * QS[2] - QR[2] * QS[0]),
                                       QR[0] * QS[1] - QR[1] * QS[0] }; // cross product
            List<float> CQ = new List<float> { Q.X - p_center.X, Q.Y - p_center.Y, Q.Z - p_center.Z };
            if (Point3d.MulMatrix(Normal, 1, 3, CQ, 3, 1)[0] > eps)
            {
                Normal[0] *= -1;
                Normal[1] *= -1;
                Normal[2] *= -1;
            }
        }

        public void Translate(float x, float y, float z)
        {
            foreach (Point3d p in Points)
            {
                p.Translate(x, y, z);
            }                
            FindCenter();
        }
    }
}
