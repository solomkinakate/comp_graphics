using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace raytracer
{
    class Polyhedron
    {
        public List<Face> Faces { get; set; } = null;
        public Point3d Center { get; set; } = new Point3d(0, 0, 0);
        public float Cube_size { get; set; }
        public bool is_sphere = false;
        public float radius = 0;
        public Color color = Color.White;
        public float specular = 0;//зеркальность
        public float reflective = 0;//отражение
        public float transparent = 0;//прозрачность

        public void FindNormals()
        {
            for (int i = 0; i < Faces.Count; ++i)
            {
                Faces[i].FindNormal(Center);
            }
        }

        public Polyhedron(List<Face> fs = null)
        {
            if (fs != null)
            {
                Faces = fs.Select(face => new Face(face)).ToList();
                FindCenter();
            }
        }

        public Polyhedron(Polyhedron polyhedron)
        {
            Faces = polyhedron.Faces.Select(face => new Face(face)).ToList();
            Center = new Point3d(polyhedron.Center);
            Cube_size = polyhedron.Cube_size;
            is_sphere = polyhedron.is_sphere;
            radius = polyhedron.radius;
            color = polyhedron.color;
            specular = polyhedron.specular;
            reflective = polyhedron.reflective;
            transparent = polyhedron.transparent;
        }

        private void FindCenter()
        {
            Center.X = 0;
            Center.Y = 0;
            Center.Z = 0;
            foreach (Face f in Faces)
            {
                Center.X += f.Center.X;
                Center.Y += f.Center.Y;
                Center.Z += f.Center.Z;
            }
            Center.X /= Faces.Count;
            Center.Y /= Faces.Count;
            Center.Z /= Faces.Count;
        }

        public void Translate(float x, float y, float z)
        {
            foreach (Face f in Faces)
            {
                f.Translate(x, y, z);
            }
            FindCenter();
        }

        public void MakeHexahedron(float cube_half_size = 50)
        {
            Face f = new Face(
                new List<Point3d>
                {
                    new Point3d(-cube_half_size, cube_half_size, cube_half_size),
                    new Point3d(cube_half_size, cube_half_size, cube_half_size),
                    new Point3d(cube_half_size, -cube_half_size, cube_half_size),
                    new Point3d(-cube_half_size, -cube_half_size, cube_half_size)
                }
            );
            Faces = new List<Face> { f }; // front face
            List<Point3d> l1 = new List<Point3d>();
            // back face
            foreach (var point in f.Points)
            {
                l1.Add(new Point3d(point.X, point.Y, point.Z - 2 * cube_half_size));
            }
            Face f1 = new Face(
                    new List<Point3d>
                    {
                        new Point3d(-cube_half_size, cube_half_size, -cube_half_size),
                        new Point3d(-cube_half_size, -cube_half_size, -cube_half_size),
                        new Point3d(cube_half_size, -cube_half_size, -cube_half_size),
                        new Point3d(cube_half_size, cube_half_size, -cube_half_size)
                    });
            Faces.Add(f1);
            // down face
            List<Point3d> l2 = new List<Point3d>
            {
                new Point3d(f.Points[2]),
                new Point3d(f1.Points[2]),
                new Point3d(f1.Points[1]),
                new Point3d(f.Points[3]),
            };
            Face f2 = new Face(l2);
            Faces.Add(f2);
            // top face
            List<Point3d> l3 = new List<Point3d>
            {
                new Point3d(f1.Points[0]),
                new Point3d(f1.Points[3]),
                new Point3d(f.Points[1]),
                new Point3d(f.Points[0]),
            };
            Face f3 = new Face(l3);
            Faces.Add(f3);
            // left face
            List<Point3d> l4 = new List<Point3d>
            {
                new Point3d(f1.Points[0]),
                new Point3d(f.Points[0]),
                new Point3d(f.Points[3]),
                new Point3d(f1.Points[1])
            };
            Face f4 = new Face(l4);
            Faces.Add(f4);
            // right face
            List<Point3d> l5 = new List<Point3d>
            {
                new Point3d(f1.Points[3]),
                new Point3d(f1.Points[2]),
                new Point3d(f.Points[2]),
                new Point3d(f.Points[1])
            };
            Face f5 = new Face(l5);
            Faces.Add(f5);
            Cube_size = 2 * cube_half_size;
            FindCenter();
        }

        public void MakeSphere(Point3d center, float radius)
        {
            is_sphere = true;
            this.radius = radius;
            Face f = new Face(new List<Point3d> { new Point3d(center.X, center.Y, center.Z) });
            Faces = new List<Face> { f };
            FindCenter();
        }
    }
}
