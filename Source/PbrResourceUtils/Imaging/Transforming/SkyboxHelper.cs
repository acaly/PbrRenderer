using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PbrResourceUtils.Imaging.Transforming
{
    public static class SkyboxHelper
    {
        public static Vector3 CubeToDir(int n, int x, int y, int z)
        {
            float cc = (n - 1) * 0.5f;
            float cx = (x - cc) / n * 2;
            float cy = (y - cc) / n * 2;
            switch (z)
            {
                case 0: return new Vector3(1, -cy, -cx);
                case 1: return new Vector3(-1, -cy, cx);
                case 2: return new Vector3(cx, 1, cy);
                case 3: return new Vector3(cx, -1, -cy);
                case 4: return new Vector3(cx, -cy, 1);
                case 5: return new Vector3(-cx, -cy, -1);
                default: return new Vector3();
            }
        }

        public static T SampleEquirectangularMap<T>(SoftwareImage<T> image, Vector3 dir) where T : unmanaged
        {
            var th = Math.Atan2(dir.Y, dir.X) / Math.PI / 2;
            if (th < 0) th += 1;
            var u = (int)Math.Floor(th * image.Width);
            var xy = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);
            var v = (int)Math.Floor(Math.Atan2(xy, dir.Z) / Math.PI * image.Height);
            if (v == image.Height) v -= 1;
            return image.GetPixel(u, v);
        }
    }
}
