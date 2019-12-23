namespace raytracer
{
    class Program
    {
        static void Main(string[] args)
        {
            RayTrace scene = new RayTrace(480, 480);
            scene.ShowScene();
            scene.Save("image.jpg");
        }
    }
}
