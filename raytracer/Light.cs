using System.Drawing;

namespace raytracer
{
    public enum LightType { lAmbient, lPoint };

    class Light
    {
        public LightType type;
        public float intensity;
        public Point3d position;
        public float r_intensity;
        public float g_intensity;
        public float b_intensity;
        public Color color = Color.White;

        public Light(LightType t, float intens, Point3d pos)
        {
            type = t;
            r_intensity = g_intensity = b_intensity = intensity = intens;
            color = Color.FromArgb((int)(255 * intens), (int)(255 * intens), (int)(255 * intens));
            position = new Point3d(pos.X, pos.Y, pos.Z);
        }
    }
}
