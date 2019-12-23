using System;
using System.Collections.Generic;
using System.Drawing;

namespace raytracer
{
    class RayTrace
    {
        private readonly int width;
        private readonly int height;
        private readonly List<Polyhedron> polyhedrons;
        private List<Light> lights;
        private Camera camera;
        private readonly int viewport_w = 1;
        private readonly int viewport_h = 1;
        private readonly int projection_plane_d = 1;
        private readonly int inf = 1000000;
        private readonly float eps = 1E-3f;
        private readonly float recurse_depth = 5;
        private readonly Color background_color = Color.Black;
        private Bitmap image;

        public RayTrace(int Width, int Height)
        {
            width = Width;
            height = Height;
            image = new Bitmap(Width, Height);
            polyhedrons = new List<Polyhedron>();
            camera = new Camera(new Point3d(0, 3, -15), Width, Height);
            GenScene();
            lights = new List<Light>
            {
                //источник света 1-фоновый 2-точечных
                //new Light(LightType.lAmbient, 0.2f, new Point3d(0, 0, 0)),
                //new Light(LightType.lPoint, 0.2f, new Point3d(0, 9, 0)),
                new Light(LightType.lPoint, 0.6f, new Point3d(-9, 9, -9))
            };
        }

        private Color Increase(float k, Color c)
        {
            int a = c.A;
            int r = Math.Min(255, Math.Max(0, (int)(c.R * k + 0.5)));
            int g = Math.Min(255, Math.Max(0, (int)(c.G * k + 0.5)));
            int b = Math.Min(255, Math.Max(0, (int)(c.B * k + 0.5)));
            return Color.FromArgb(a, r, g, b);
        }

        private Color Increase(Point3d k, Color c)
        {
            int a = c.A;
            int r = Math.Min(255, Math.Max(0, (int)(c.R * k.X + 0.5)));
            int g = Math.Min(255, Math.Max(0, (int)(c.G * k.Y + 0.5)));
            int b = Math.Min(255, Math.Max(0, (int)(c.B * k.Z + 0.5)));
            return Color.FromArgb(a, r, g, b);
        }

        private Color Sum(Color c1, Color c2)
        {
            int a = c1.A;
            int r = Math.Max(0, Math.Min(255, c1.R + c2.R));
            int g = Math.Max(0, Math.Min(255, c1.G + c2.G));
            int b = Math.Max(0, Math.Min(255, c1.B + c2.B));
            return Color.FromArgb(a, r, g, b);
        }

        //для вычисления геометрии
        private float Dot(Point3d v1, Point3d v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        private float Length(Point3d vec)
        {
            return (float)Math.Sqrt(Dot(vec, vec));
        }

        private Point3d Mul(float k, Point3d vec)
        {
            return new Point3d(k * vec.X, k * vec.Y, k * vec.Z);
        }

        private Point3d Sum(Point3d vec1, Point3d vec2)
        {
            return new Point3d(vec1.X + vec2.X, vec1.Y + vec2.Y, vec1.Z + vec2.Z);
        }

        private Point3d Sub(Point3d vec1, Point3d vec2)
        {
            return new Point3d(vec1.X - vec2.X, vec1.Y - vec2.Y, vec1.Z - vec2.Z);
        }
        
        //отраженный луч
        private Point3d ReflectRay(Point3d R, Point3d N)
        {
            return Sub(Mul(2 * Dot(R, N), N), R);
        }

        private void AddSphere(Point3d point, float radius, Color color, float specular = 0, float reflective = 0, float transparent = 0)
        {
            var sphere = new Polyhedron();
            sphere.MakeSphere(point, radius);
            sphere.color = color;
            sphere.specular = specular;
            sphere.reflective = reflective;
            sphere.transparent = transparent;
            polyhedrons.Add(sphere);
        }

        private void AddCube(float cube_half_size, Point3d point, Color color, float specular = 0, float reflecive = 0, float transparent = 0)
        {
            var cube = new Polyhedron();
            cube.MakeHexahedron(cube_half_size);
            cube.Translate(point.X, point.Y, point.Z);
            cube.color = color;
            cube.specular = specular;
            cube.reflective = reflecive;
            cube.transparent = transparent;
            cube.FindNormals();
            polyhedrons.Add(cube);
        }

        private void AddWall(List<Point3d> points, List<float> normal, Color color, float specular = 0, float reflective = 0)
        {
            Face f = new Face(points);
            var wall = new Polyhedron(new List<Face>() { f });
            wall.Faces[0].Normal = normal;
            wall.color = color;
            wall.specular = specular;
            wall.reflective = reflective;
            polyhedrons.Add(wall);
        }
           //генерация сцены
        private void GenScene()
        {
            AddSphere(new Point3d(-3, 2, 1), 1, Color.Red, 500);
            AddSphere(new Point3d(0, 3, -3), 1, Color.White, 500, 1);
            AddSphere(new Point3d(3, 0, -4), 0.5f, Color.White, 500, 0, 1);

            AddCube(0.75f, new Point3d(3, 1, 1), Color.Blue, 500);
            AddCube(0.75f, new Point3d(-1, 0, -3), Color.White, 500, 1);
            AddCube(0.5f, new Point3d(-3, 0, -5), Color.White, 500, 0, 1);
            AddCube(0.5f, new Point3d(-1, 0, -7), Color.Green, 500);

            List<Point3d> points = new List<Point3d>() { new Point3d(-10, -1, -10), new Point3d(-10, 10, -10), new Point3d(10, 10, -10), new Point3d(10, -1, -10) };
            AddWall(points, new List<float>() { 0, 0, -1 }, Color.DeepPink);
            points = new List<Point3d>() { new Point3d(-10, -1, 10), new Point3d(-10, 10, 10), new Point3d(10, 10, 10), new Point3d(10, -1, 10) };
            AddWall(points, new List<float>() { 0, 0, 1 }, Color.Chocolate);            
            points = new List<Point3d>() { new Point3d(-10, -1, -10), new Point3d(-10, -1, 10), new Point3d(10, -1, 10), new Point3d(10, -1, -10) };
            AddWall(points, new List<float>() { 0, -1, 0 }, Color.Yellow);
            points = new List<Point3d>() { new Point3d(-10, 10, -10), new Point3d(-10, 10, 10), new Point3d(10, 10, 10), new Point3d(10, 10, -10) };
            AddWall(points, new List<float>() { 0, 1, 0 }, Color.Green);
            points = new List<Point3d>() { new Point3d(-10, -1, -10), new Point3d(-10, 10, -10), new Point3d(-10, 10, 10), new Point3d(-10, -1, 10) };
            AddWall(points, new List<float>() { -1, 0, 0 }, Color.Red);
            points = new List<Point3d>() { new Point3d(10, -1, -10), new Point3d(10, 10, -10), new Point3d(10, 10, 10), new Point3d(10, -1, 10) };
            AddWall(points, new List<float>() { 1, 0, 0 }, Color.Blue);
        }
        //точка обзора
        private Point3d CanvasToViewport(int x, int y, float width, float height)
        {
            float X = (float)x * viewport_w / width;
            float Y = (float)y * viewport_h / height;
            return new Point3d(X, Y, projection_plane_d);
        }

        private void ClosestIntersection(Point3d camera, Point3d D, float t_min, float t_max, ref Polyhedron closest, ref float closest_t, ref Point3d norm)
        {
            closest_t = inf;
            closest = null;
            norm = null;
            foreach (var polyhedron in polyhedrons)
            {
                if (polyhedron.is_sphere)
                {
                    PointF t = IntersectRaySphere(camera, D, polyhedron);
                    if (t.X < closest_t && t_min < t.X && t.X < t_max)
                    {
                        closest_t = t.X;
                        closest = polyhedron;
                    }
                    if (t.Y < closest_t && t_min < t.Y && t.Y < t_max)
                    {
                        closest_t = t.Y;
                        closest = polyhedron;
                    }
                }
                else
                {
                    Point3d norm_res = null;
                    float t = IntersectRay(camera, D, polyhedron, ref norm_res);
                    if (t < closest_t && t_min < t && t < t_max)
                    {
                        closest_t = t;
                        closest = polyhedron;
                        norm = norm_res;
                    }
                }
            }
            if (closest != null && closest.is_sphere)
            {
                var point = Sum(camera, Mul(closest_t, D));
                norm = Sub(point, closest.Center);
            }
        }
        //цвет и тени
        private Color TraceRay(Point3d camera, Point3d D, float t_min, float t_max, float depth, int step)
        {
            float closest_t = inf;
            Polyhedron closest = null;
            Point3d normal = null;
            Point3d point = null;
            ClosestIntersection(camera, D, t_min, t_max, ref closest, ref closest_t, ref normal);
            if (closest == null)
            {
                return background_color;
            }
            normal = Mul(1.0f / Length(normal), normal);
            point = Sum(camera, Mul(closest_t, D));
            var light_k = ComputeLighting(point, normal, Mul(-1, D), closest.specular);
            Color local = Increase(light_k, closest.color);
            if (step > recurse_depth || depth <= eps)
            {
                return local;
            }
            var r = ReflectRay(Mul(-1, D), normal);
            var refl_color = TraceRay(point, r, eps, inf, depth * closest.reflective, step + 1);
            Color reflected = Sum(Increase(1 - closest.reflective, local), Increase(closest.reflective, refl_color));
            if (closest.transparent <= 0)
            {
                return Increase(depth, reflected);
            }
            var refracted = Refract(D, normal, 1.5f);
            var tr_color = TraceRay(point, refracted, eps, inf, depth * closest.transparent, step + 1);
            Color transp = Sum(Increase(1 - closest.transparent, reflected), Increase(closest.transparent, tr_color));
            return Increase(depth, transp);
        }

        public float Clip(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private Point3d Refract(Point3d I, Point3d N, float ior)
        {
            Point3d res = new Point3d(0, 0, 0);
            float cosi = Clip(Dot(I, N), -1, 1);
            float etai = 1, etat = ior;
            Point3d n = new Point3d(N.X, N.Y, N.Z);
            if (cosi < 0)
            {
                cosi = -cosi;
            }
            else
            {
                etai = ior;
                etat = 1;
                n = new Point3d(-N.X, -N.Y, -N.Z);
            }
            float eta = etai / etat;
            float k = 1 - eta * eta * (1 - cosi * cosi);
            if (k < 0)
            {
                return res;
            }
            return Sum(Mul(eta, I), Mul((float)(eta * cosi - Math.Sqrt(k)), n));
        }
        //обработка теней
        private Point3d ComputeLighting(Point3d point, Point3d normal, Point3d view, float specular)
        {
            Point3d intensity = new Point3d(0, 0, 0);
            float length_n = Length(normal);
            float length_v = Length(view);
            float t_max = 0;
            for (int i = 0; i < lights.Count; ++i)
            {
                Light light = lights[i];
                if (light.type == LightType.lAmbient)
                {
                    intensity.X += light.r_intensity;
                    intensity.Y += light.g_intensity;
                    intensity.Z += light.b_intensity;
                }
                else
                {
                    Point3d vecrot_light;
                    if (light.type == LightType.lPoint)
                    {
                        vecrot_light = Sub(light.position, point);
                        t_max = 1f;
                    }
                    else
                    {
                        vecrot_light = light.position;
                        t_max = inf;
                    }
                    Polyhedron blocker = null;
                    float closest_t = 0f;
                    Point3d norm = null;
                    ClosestIntersection(point, vecrot_light, eps, t_max, ref blocker, ref closest_t, ref norm);
                    float tr = 1;
                    if (blocker != null)
                    {
                        continue;
                    }
                    var n_dot_l = Dot(normal, vecrot_light);
                    if (n_dot_l > 0)
                    {
                        intensity.X += tr * light.r_intensity * n_dot_l / (length_n * Length(vecrot_light));
                        intensity.Y += tr * light.g_intensity * n_dot_l / (length_n * Length(vecrot_light));
                        intensity.Z += tr * light.b_intensity * n_dot_l / (length_n * Length(vecrot_light));
                    }
                    if (specular > 0)
                    {
                        var vec_r = ReflectRay(vecrot_light, normal);
                        var r_dot_v = Dot(vec_r, view);
                        if (r_dot_v > 0)
                        {
                            intensity.X += tr * light.r_intensity * (float)Math.Pow(r_dot_v / (Length(vec_r) * length_v), specular);
                            intensity.Y += tr * light.g_intensity * (float)Math.Pow(r_dot_v / (Length(vec_r) * length_v), specular);
                            intensity.Z += tr * light.b_intensity * (float)Math.Pow(r_dot_v / (Length(vec_r) * length_v), specular);
                        }
                    }
                }
            }
            return intensity;
        }

        private float IntersectRay(Point3d camera, Point3d D, Polyhedron polyhedron, ref Point3d norm)
        {
            float res = inf;
            norm = null;
            for (int i = 0; i < polyhedron.Faces.Count; ++i)
            {
                var n = polyhedron.Faces[i].Normal;
                Point3d normal = new Point3d(n[0], n[1], n[2]);
                Mul(1f / Length(normal), normal);
                var d_n = Dot(D, normal);
                if (d_n < eps)
                {
                    continue;
                }
                var d = Dot(Sub(polyhedron.Faces[i].Center, camera), normal) / d_n;
                if (d < 0)
                {
                    continue;
                }
                var point = Sum(camera, Mul(d, D));
                if (res > d && polyhedron.Faces[i].Inside(point))
                {
                    res = d;
                    norm = Mul(-1, normal);
                }
            }
            return res;
        }

        private PointF IntersectRaySphere(Point3d camera, Point3d D, Polyhedron sphere)
        {
            PointF res = new Point(inf, inf);
            float r = sphere.radius;
            Point3d OC = Sub(camera, sphere.Center);
            float k1 = Dot(D, D);
            float k2 = 2 * Dot(OC, D);
            float k3 = Dot(OC, OC) - r * r;
            float discriminant = k2 * k2 - 4 * k1 * k3;
            if (discriminant < 0)
            {
                return res;
            }
            double t1 = (-k2 + Math.Sqrt(discriminant)) / (2 * k1);
            double t2 = (-k2 - Math.Sqrt(discriminant)) / (2 * k1);
            res.X = (float)t1;
            res.Y = (float)t2;
            return res;
        }

        public void ShowScene()
        {
            for (int y = -height / 2; y < height / 2; ++y)
            {
                for (int x = -width / 2; x < width / 2; ++x)
                {
                    Point3d D = CanvasToViewport(x, y, width, height);
                    Color c = TraceRay(camera.view.P1, D, 1, inf, 1, 0);
                    int bmpx = x + width / 2;
                    int bmpy = height / 2 - y - 1;
                    if (bmpx < 0 || bmpx >= width || bmpy < 0 || bmpy >= height)
                    {
                        continue;
                    }
                    image.SetPixel(bmpx, bmpy, c);
                }
            }
        }
        
        public void Save(string filename)
        {
            image.Save(filename);
        }
    }
}
