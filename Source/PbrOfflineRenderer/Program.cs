using PbrResourceUtils.Imaging;
using PbrResourceUtils.Imaging.Hdr;
using PbrResourceUtils.Imaging.Sampling;
using PbrResourceUtils.Imaging.Transforming;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PbrOfflineRenderer
{
    class Program
    {
        private static readonly string TestScenePath = @"..\..\..\..\Example\TestScene"; //Assume Proj/bin/{Debug|Release}

        private static string TestSceneResource(string rel)
        {
            return Path.Combine(TestScenePath, "Resource", rel);
        }

        private static string TestSceneCompiled(string rel)
        {
            return Path.Combine(TestScenePath, "Compiled", rel);
        }

        private static ThreadLocal<Random> _rand = new ThreadLocal<Random>(() => new Random());

        static void Main(string[] args)
        {
            var normalMap = new SoftwareImage<R32G32B32A32F>(800, 600, new R32G32B32A32FTransformer());
            normalMap.LoadRawDataFile(TestSceneCompiled("OutputNormal.raw"));
            var viewDirMap = new SoftwareImage<R32G32B32A32F>(800, 600, new R32G32B32A32FTransformer());
            viewDirMap.LoadRawDataFile(TestSceneCompiled("OutputViewDir.raw"));

            SoftwareImage<R32G32B32A32F> skybox = HdrImageLoader.Read(TestSceneResource("473-free-hdri-skies-com.hdr"));

            SoftwareImage<R32G32B32A32F> skyboxSpecular = skybox;
            {
                skyboxSpecular = new HalfSize<R32G32B32A32F>(skyboxSpecular).Generate();
                skyboxSpecular = new HalfSize<R32G32B32A32F>(skyboxSpecular).Generate();
                skyboxSpecular = new HalfSize<R32G32B32A32F>(skyboxSpecular).Generate();
            }

            SoftwareImage<R32G32B32A32F> skyboxDiffuse = skybox;
            {
                skyboxDiffuse = new HalfSize<R32G32B32A32F>(skyboxDiffuse).Generate();
                skyboxDiffuse = new HalfSize<R32G32B32A32F>(skyboxDiffuse).Generate();
                skyboxDiffuse = new HalfSize<R32G32B32A32F>(skyboxDiffuse).Generate();
                skyboxDiffuse = new HalfSize<R32G32B32A32F>(skyboxDiffuse).Generate();
                skyboxDiffuse = new HalfSize<R32G32B32A32F>(skyboxDiffuse).Generate();
                skyboxDiffuse = new HalfSize<R32G32B32A32F>(skyboxDiffuse).Generate();
            }

            var p = skybox.PixelTransformer;

            //System.Drawing.Bitmap does not support concurrent SetPixel. Use our own instead.
            var bitmap = new SoftwareImage<R8G8B8A8>(800, 600, new R8G8B8A8Transformer());

            Parallel.For(0, 600, (int y) =>
            //for (int y = 0; y < 600; ++y)
            {
                var rand = _rand.Value;
                for (int x = 0; x < 800; ++x)
                {
                    var normal = normalMap.GetPixel(x, y);
                    if (normal.R == 0 && normal.G == 0 && normal.B == 0) continue;
                    var viewDir = viewDirMap.GetPixel(x, y);

                    var n = new Vector3(normal.R, normal.G, normal.B);
                    var e = Vector3.Normalize(-new Vector3(viewDir.R, viewDir.G, viewDir.B));
                    var l = 2 * Vector3.Dot(n, e) * n - e;

                    R32G32B32A32F color = default;

                    //Perfect reflection
                    {
                        color = p.Add(color, SkyboxHelper.SampleEquirectangularMap(skyboxSpecular, l));
                    }
                    //Diffuse
                    {
                        const int DiffuseSampleCount = 20;
                        var sampler = new SphereDirectionalSampler(rand, n, (float)Math.PI / 2, DiffuseSampleCount);
                        R32G32B32A32F total = default;
                        for (int sample = 0; sample < sampler.SampleCount; ++sample)
                        {
                            var sampleDir = sampler.Sample(sample);
                            var cc = SkyboxHelper.SampleEquirectangularMap(skyboxDiffuse, sampleDir);
                            total = p.Add(total, p.Scale(cc, Vector3.Dot(n, sampleDir) / (float)Math.PI));
                        }
                        color = p.Add(color, p.Scale(total, 1f / sampler.SampleCount * 5));
                    }

                    var rgb = p.ToColor(color);
                    bitmap.GetPixel(x, y) = new R8G8B8A8
                    {
                        R = rgb.R,
                        G = rgb.G,
                        B = rgb.B,
                        A = 255,
                    };
                }
            //}
            });
            bitmap.ToBitmap().Save(TestSceneCompiled("Offline.png"));
        }
    }
}
