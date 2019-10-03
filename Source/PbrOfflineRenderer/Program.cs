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
                        var viewDir = viewDirMap.GetPixel(x, y);
                        var normal = normalMap.GetPixel(x, y);
                        var color = SkyboxHelper.SampleEquirectangularMap(skybox, new Vector3(normal.R, normal.G, normal.B));
                        var rgb = skybox.PixelTransformer.ToColor(color);
                        bitmap.SetPixel(x, y, rgb);
                    }
                }
                bitmap.Save(TestSceneCompiled("Offline.png"));
            }
        }
    }
}
