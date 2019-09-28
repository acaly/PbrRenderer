using PbrSceneCompiler.Imaging.Sampling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Imaging.Transforming
{
    class SkyboxSphericalToCube<T> where T : unmanaged
    {
        private readonly SoftwareImage<T> _image;

        public SkyboxSphericalToCube(SoftwareImage<T> image)
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
                        //1. calculate center and a radius
                        //2. randomly generate samples within that radus
                        //   use stratify method and jittering
                        //3. check whether the sample is in the pixel (by scaling the already known maximum dimension to 1)
                        //4. if check succeeds, write the point sample to destination
                        var center = GetCenter(newSize, x, y, z);
                        var r = (float)Math.Atan(0.5 / newSize) * 0.8f;
                        var sampler = new RectDirectionalSampler(rand, center, r, 4);
                        T total = default;
                        for (int sample = 0; sample < 16; ++sample)
                        {
                            var sampleDir = sampler.Sample(sample);
                            //TODO check in range
                            var color = SampleOriginal(sampleDir);
                            total = p.Add(total, color);
                        }
                        faces[z].GetPixel(x, y) = p.Scale(total, 1 / 16f);
                    }
                }
            }
            return faces;
        }

        private static Vector3 GetCenter(int n, int x, int y, int z)
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

        private T SampleOriginal(Vector3 dir)
        {
            var th = Math.Atan2(dir.Y, dir.X) / Math.PI / 2;
            if (th < 0) th += 1;
            var u = (int)Math.Floor(th * _image.Width);
            var xy = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);
            var v = (int)Math.Floor(Math.Atan2(xy, dir.Z) / Math.PI * _image.Height);
            if (v == _image.Height) v -= 1;
            return _image.GetPixel(u, v);
        }

        //TODO move this to a common helper class
        public static void WriteSRD(SRDFile file, SoftwareImage<T>[] faces)
        {
            var size = faces[0].Width;
            file.WriteHeaders(new SRDFileHeader
            {
                Magic = SRDFile.Magic[2],
                Format = faces[0].PixelTransformer.DXGIFormat,
                MipLevel = 1,
                ArraySize = 1,
            }, new[] {
                new SRDSegmentHeader
                {
                    Offset = 0,
                    Width = (ushort)size,
                    Height = (ushort)size,
                    Depth = 6,
                    Stride = (ushort)(Marshal.SizeOf<T>() * size),
                    Slice = (uint)(Marshal.SizeOf<T>() * size * size),
                },
            });
            file.WriteOffset(0);
            file.Write(faces[0].RawData);
            file.Write(faces[1].RawData);
            file.Write(faces[2].RawData);
            file.Write(faces[3].RawData);
            file.Write(faces[4].RawData);
            file.Write(faces[5].RawData);
            file.Close();
        }
    }
}
