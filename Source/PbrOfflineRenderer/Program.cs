using PbrResourceUtils.Imaging;
using PbrResourceUtils.Imaging.Hdr;
using PbrResourceUtils.Imaging.Transforming;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
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

        static void Main(string[] args)
        {
            var normalMap = new SoftwareImage<R32G32B32A32F>(800, 600, new R32G32B32A32FTransformer());
            normalMap.LoadRawDataFile(TestSceneCompiled("OutputNormal.raw"));
            var viewDirMap = new SoftwareImage<R32G32B32A32F>(800, 600, new R32G32B32A32FTransformer());
            viewDirMap.LoadRawDataFile(TestSceneCompiled("OutputViewDir.raw"));
            var skybox = HdrImageLoader.Read(TestSceneResource("473-free-hdri-skies-com.hdr"));

            using (var bitmap = new Bitmap(800, 600))
            {
                for (int y = 0; y < 600; ++y)
                {
                    for (int x = 0; x < 800; ++x)
                    {
                        var normal = normalMap.GetPixel(x, y);
                        if (normal.R == 0 && normal.G == 0 && normal.B == 0) continue;
                        var viewDir = viewDirMap.GetPixel(x, y);

                        var n = new Vector3(normal.R, normal.G, normal.B);

                        var e = Vector3.Normalize(-new Vector3(viewDir.R, viewDir.G, viewDir.B));
                        var l = 2 * Vector3.Dot(n, e) * n - e;
                        var color = SkyboxHelper.SampleEquirectangularMap(skybox, l);
                        var rgb = skybox.PixelTransformer.ToColor(color);
                        bitmap.SetPixel(x, y, rgb);
                    }
                }
                bitmap.Save(TestSceneCompiled("Offline.png"));
            }
        }
    }
}
