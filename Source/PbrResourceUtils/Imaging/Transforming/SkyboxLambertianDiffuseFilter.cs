using PbrResourceUtils.Imaging.Sampling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PbrResourceUtils.Imaging.Transforming
{
    public class SkyboxLambertianDiffuseFilter<T> where T : unmanaged
    {
        private readonly SoftwareImage<T> _image;

        public SkyboxLambertianDiffuseFilter(SoftwareImage<T> image)
        {
            _image = image;
        }

        public SoftwareImage<T>[] Generate(int newSize, int nSample)
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
                        var center = SkyboxHelper.CubeToDir(newSize, x, y, z);
                        center = Vector3.Normalize(center);

                        var sampler = new SphereDirectionalSampler(rand, center, (float)Math.PI / 2, nSample);
                        T total = default;
                        for (int sample = 0; sample < sampler.SampleCount; ++sample)
                        {
                            var sampleDir = sampler.Sample(sample);
                            var color = SkyboxHelper.SampleEquirectangularMap(_image, sampleDir);
                            total = p.Add(total, p.Scale(color, Vector3.Dot(center, sampleDir)));
                        }
                        faces[z].GetPixel(x, y) = p.Scale(total, 1f / sampler.SampleCount / (float)Math.PI);
                    }
                }
            }
            return faces;
        }
    }
}
