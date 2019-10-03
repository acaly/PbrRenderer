using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbrResourceUtils.Imaging.Transforming
{
    public class HalfSize<T> where T : unmanaged
    {
        public HalfSize(SoftwareImage<T> image)
        {
            Image = image;
        }

        private SoftwareImage<T> Image { get; }

        public SoftwareImage<T> Generate()
        {
            const float ratio = 0.25f;
            var tr = Image.PixelTransformer;
            var ret = new SoftwareImage<T>(Image.Width / 2, Image.Height / 2, tr);
            for (int y = 0; y < Image.Height; ++y)
            {
                for (int x = 0; x < Image.Width; ++x)
                {
                    ref var p = ref ret.GetPixel(x / 2, y / 2);
                    p = tr.Add(p, tr.Scale(Image.GetPixel(x, y), ratio));
                }
            }
            return ret;
        }
    }
}
