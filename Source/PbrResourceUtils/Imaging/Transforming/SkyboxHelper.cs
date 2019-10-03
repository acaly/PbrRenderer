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

        private static T GetImagePixel<T>(SoftwareImage<T> image, int x, int y) where T : unmanaged
        {
            x = (x + image.Width) % image.Width;
            y = (y + image.Height) % image.Height;
            return image.GetPixel(x, y);
        }

        private static T SampleLinear<T>(SoftwareImage<T> image, float x, float y) where T : unmanaged
        {
            float rx = x - 0.5f;
            float ry = y - 0.5f;
            int x0 = (int)Math.Floor(rx);
            int y0 = (int)Math.Floor(ry);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tx = rx - x0;
            float ty = ry - y0;
            var c00 = GetImagePixel(image, x0, y0);
            var c10 = GetImagePixel(image, x1, y0);
            var c01 = GetImagePixel(image, x0, y1);
            var c11 = GetImagePixel(image, x1, y1);

            var p = image.PixelTransformer;
            var cc = p.Scale(c00, (1 - tx) * (1 - ty));
            cc = p.Add(cc, p.Scale(c10, tx * (1 - ty)));
            cc = p.Add(cc, p.Scale(c01, (1 - tx) * ty));
            cc = p.Add(cc, p.Scale(c11, tx * ty));
            return cc;
        }

        public static T SampleEquirectangularMap<T>(SoftwareImage<T> image, Vector3 dir) where T : unmanaged
        {
            var th = Math.Atan2(dir.Y, dir.X) / Math.PI / 2;
            if (th < 0) th += 1;
            var u = (float)(th * image.Width);
            var xy = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);
            var v = (float)(Math.Atan2(xy, dir.Z) / Math.PI * image.Height);
            return SampleLinear(image, u, v);
        }
    }
}
