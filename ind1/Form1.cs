using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Ind1
{
    public partial class MBO : Form
    {
        private readonly Graphics g;
        private readonly Pen p;
        private readonly SolidBrush b;
        private readonly List<Point> points;
        private const int MIN_COUNT = 3;
        private const int FLOOR = 6;
        
        public MBO()
        {
            InitializeComponent();
            g = CreateGraphics();
            p = new Pen(Color.Black);
            b = new SolidBrush(Color.Black);
            points = new List<Point>();
            MouseClick += new MouseEventHandler(MBO_MouseClick);
            button1.MouseClick += new MouseEventHandler(ButtonClear_MouseClick);
            button2.MouseClick += new MouseEventHandler(ButtonGo_MouseClick);
        }

        private void MBO_MouseClick(object sender, MouseEventArgs e)
        {
            var point = new Point(e.X, e.Y);
            DrawPoint(point);
            points.Add(point);
        }

        private void ButtonClear_MouseClick(object sender, MouseEventArgs e)
        {   
            points.Clear();
            g.Clear(Color.White);
        }

        private void ButtonGo_MouseClick(object sender, MouseEventArgs e)
        {
            if (points.Count < MIN_COUNT)
                return;
            g.Clear(Color.White);
            foreach (var el in points)
                DrawPoint(el);
            var result = DoRecurse(points.ToArray());
            DrawHull(new Hull(result));
        }

        private void DrawPoint(Point point)
        {
            g.DrawEllipse(p, point.X - 1, point.Y - 1, 3, 3);
            g.FillEllipse(b, point.X - 1, point.Y - 1, 3, 3);
        }

        private void DrawHull(Hull ps)
        {
            for (int i = 1; i <= ps.Count; ++i)
                g.DrawLine(p, ps[i - 1].X, ps[i - 1].Y, ps[i].X, ps[i].Y);
        }

        private Point[] DoRecurse(Point[] ps)
        {
            if (ps.Length < FLOOR)
                return HullTools.GetConvexHull(ps).ToArray();
            var hull = new Hull(ps);
            var twoSets = HullTools.Split(hull);
            var first = DoRecurse(twoSets.Item1.Points);
            var second = DoRecurse(twoSets.Item2.Points);
            return CreatUnion(new Hull(first), new Hull(second));
        }

        private Point[] CreatUnion(Hull first, Hull second)
        {
            var aPoint = HullTools.GetInnerPoint(first);
            Point[] merged;
            if (HullTools.IsWithinPolygon(aPoint, second))
                merged = HullTools.MergePointsByPolarAngle(first.Points, second.Points);
            else
            {
                var twoLines = HullTools.GetSupportPoints(aPoint, second);
                var tempHull = new Hull(new[] { twoLines.Item1, twoLines.Item2, aPoint });
                merged = HullTools.MergePointsByPolarAngle(
                    first.Points.Where(x => !HullTools.IsWithinPolygon(x, tempHull)).ToArray(),
                    second.Points.Where(x => !HullTools.IsWithinPolygon(x, tempHull)).ToArray()
                    );
            }
            return HullTools.GetConvexHull(merged).ToArray();
        }
    }

    public class Hull
    {
        public Point[] Points { get; }
        public int Count { get; }

        public Hull(Point[] points)
        {
            Points = points;
            Count = Points.Length;
        }

        public Point this[int i] => Points[i % Count];
    }

    public static class HullTools
    {
        private static float eps = 0.001f;
        private static int maxX = 100000;

        public static Tuple<Hull, Hull> Split(Hull hull)
        {
            var firstSet = new List<Point>();
            var secondSet = new List<Point>();
            for (var i = 0; i < hull.Count; ++i)
            {
                var point = hull[i];
                if (i % 2 == 0)
                    firstSet.Add(point);
                else
                    secondSet.Add(point);
            }
            return Tuple.Create(
                new Hull(firstSet.ToArray()),
                new Hull(secondSet.ToArray())
                );
        }

        public static Point GetInnerPoint(Hull hull)
        {
            if (hull.Count < 3)
                throw new InvalidOperationException("hull has less than 3 points");
            var x = (hull[0].X + hull[1].X + hull[2].X) / 3;
            var y = (hull[0].Y + hull[1].Y + hull[2].Y) / 3;
            return new Point(x, y);
        }

        private static bool IsIntersectedSegments(Point a, Point b, Point c, Point d)
        {
            return IsIntersectedCoordinates(a.X, b.X, c.X, d.X)
                && IsIntersectedCoordinates(a.Y, b.Y, c.Y, d.Y)
                && GetOrientedArea(a, b, c) * GetOrientedArea(a, b, d) <= eps
                && GetOrientedArea(c, d, a) * GetOrientedArea(c, d, b) <= eps;
        }

        private static bool IsIntersectedCoordinates(double a, double b, double c, double d)
        {
            if (a > b)
                Swap(ref a, ref b);
            if (c > d)
                Swap(ref c, ref d);
            return Math.Max(a, c) <= Math.Min(b, d);
        }

        private static void Swap(ref double a, ref double b)
        {
            var t = a;
            a = b;
            b = t;
        }

        private static float GetOrientedArea(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        public static bool IsWithinPolygon(Point point, Hull hull)
        {
            var endLinePoint = new Point(maxX, point.Y);
            var intersectionCount = GetIntersectionCount(point, endLinePoint, hull);
            return intersectionCount % 2 == 1;
        }

        private static int GetIntersectionCount(Point startPoint, Point endPoint, Hull hull)
        {
            var intersectionCount = 0;
            for (var i = 0; i < hull.Count; ++i)
            {
                var isIntersection = IsIntersectedSegments(startPoint, endPoint, hull[i], hull[i + 1]);
                if (isIntersection)
                    ++intersectionCount;
            }
            return intersectionCount;
        }

        public static Tuple<Point, Point> GetSupportPoints(Point point, Hull hull)
        {
            var supportPoints = new List<Point>();
            for (var i = 0; i < hull.Count; ++i)
                if (IsSupportVertex(point, hull, i))
                    supportPoints.Add(hull[i]);
            if (supportPoints.Count < 2)
                throw new InvalidOperationException("wrong hull");
            return Tuple.Create(supportPoints[0], supportPoints[1]);
        }

        private static bool IsSupportVertex(Point point, Hull hull, int vertexIndex)
        {
            var intersectionCount = GetIntersectionCount(point, hull[vertexIndex], hull);
            return intersectionCount % 2 == 0;
        }

        private static double GetPolarAngle(Point point)
        {
            double alpha = Math.Atan2(point.Y, point.X);
            if (alpha < 0)
                alpha += 2 * Math.PI;
            return alpha;
        }

        public static Point[] MergePointsByPolarAngle(Point[] points1, Point[] points2)
        {
            var points = points1.Concat(points2);
            return points.OrderBy(GetPolarAngle).ToArray();
        }

        public static Stack<Point> GetConvexHull(Point[] points)
        {
            var sorted = points.OrderBy(p => p.Y).ThenBy(p => p.X).ToArray();
            var leftDown = sorted.First();
            sorted = sorted.Skip(1).OrderBy(p => p, new RootPointComparer(leftDown)).ToArray();
            points = new[] { leftDown }.Concat(sorted).ToArray();
            var stack = new Stack<Point>();
            stack.Push(points[0]);
            stack.Push(points[1]);
            stack.Push(points[2]);
            for (int i = 3; i < points.Length; i++)
            {
                while (stack.Count > 1 && stack.NextToTop().Orientation(stack.Peek(), points[i]) != 2)
                    stack.Pop();
                stack.Push(points[i]);
            }
            return stack;
        }
    }

    public static class StackExtensions
    {
        public static Point NextToTop(this Stack<Point> source)
        {
            var top = source.Pop();
            var next = source.Peek();
            source.Push(top);
            return next;
        }
    }

    public static class PointExtensions
    {
        public static int Orientation(this Point root, Point q, Point r)
        {
            int val = (q.Y - root.Y) * (r.X - q.X) - (q.X - root.X) * (r.Y - q.Y);
            if (val == 0) return 0;
            return (val > 0) ? 1 : 2;
        }

        public static int DistanceTo(this Point source, Point target)
        {
            return (source.X - target.X) * (source.X - target.X) + (source.Y - target.Y) * (source.Y - target.Y);
        }
    }

    public class RootPointComparer : Comparer<Point>
    {
        private readonly Point Root;
        public RootPointComparer(Point root)
        {
            Root = root;
        }

        public override int Compare(Point x, Point y)
        {
            if (x == y)
                return 0;
            var temp = Root.Orientation(x, y);
            if (temp == 0)
                return (Root.DistanceTo(y) >= Root.DistanceTo(x)) ? -1 : 1;
            return (temp == 2) ? -1 : 1;
        }
    }
}
