namespace raytracer
{
    class Edge
    {
        public Point3d P1 { get; set; }
        public Point3d P2 { get; set; }

        public Edge(Point3d pt1, Point3d pt2)
        {
            P1 = new Point3d(pt1);
            P2 = new Point3d(pt2);
        }
    }
}
