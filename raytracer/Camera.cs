namespace raytracer
{
    class Camera
    {
        public Edge view = new Edge(new Point3d(0, 0, 0), new Point3d(0, 0, -1));
        public int Width { get; set; }
        public int Height { get; set; }

        public Camera(Point3d p, int w, int h)
        {
            view.P1.X = p.X;
            view.P1.Y = p.Y;
            view.P1.Z = p.Z;
            view.P2.X = view.P1.X;
            view.P2.Y = view.P1.Y;
            view.P2.Z = view.P1.Z + 1;
            Width = w;
            Height = h;
        }
    }
}
