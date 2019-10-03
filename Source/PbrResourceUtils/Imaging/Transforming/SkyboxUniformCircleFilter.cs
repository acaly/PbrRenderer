using PbrResourceUtils.Imaging.Sampling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PbrResourceUtils.Imaging.Transforming
{
    public class SkyboxUniformCircleFilter<T> where T : unmanaged
    {
        private readonly SoftwareImage<T> _image;

        public SkyboxUniformCircleFilter(SoftwareImage<T> image)
        {
            _image = image;
        }

        public SoftwareImage<T>[] Generate(int newSize)
        {
            var faces = new SoftwareImage<T>[6];
            faces[0] = new SoftwareImage<T>(newSize, newSize, _image.PixelTransformer);
            faces[1] = new SoftwareImage<T>(newSize, newSize, _image.PixelTransformer);
            faces[2] = new SoftwareImage<T>(newSize, newSize, _image.PixelTransformer);
            faces[3] = new SoftwareImage<T>(newSize, newSize, _image.PixelTransformer);
            faces[4] = new SoftwareImage<T>(newSize, newSize, _image.PixelTransformer);
            faces[5] = new SoftwareImage<T>(newSize, newSize, _image.PixelTransformer);

            var p = faces[0].PixelTransformer;
            var rand = new Random();
            for (int z = 0; z < 6; ++z)
            {
                for (int y = 0; y < newSize; ++y)
                {
                    for (int x = 0; x < newSize; ++x)
                    {
                        ////1. calculate center and a radius
                        ////2. randomly generate samples within that radus
                        ////   use stratify method and jittering
                        ////3. check whether the sample is in the pixel (by scaling the already known maximum dimension to 1)
                        ////4. if check succeeds, write the point sample to destination
                        var center = SkyboxHelper.CubeToDir(newSize, x, y, z);
                        var r = (float)Math.Atan(0.5 / newSize);
                        var sampler = new CircleDirectionalSampler(rand, center, r, 4);
                        T total = default;
                        for (int sample = 0; sample < sampler.SampleCount; ++sample)
                        {
                            var sampleDir = sampler.Sample(sample);
                            var color = SkyboxHelper.SampleEquirectangularMap(_image, sampleDir);
                            total = p.Add(total, color);
                        }
                        faces[z].GetPixel(x, y) = p.Scale(total, 1f / sampler.SampleCount);
                    }
                }
            }
            return faces;
        }
    }
}
