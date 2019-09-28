using PbrSceneCompiler.Imaging;
using PbrSceneCompiler.Imaging.Hdr;
using PbrSceneCompiler.Imaging.Transforming;
using PbrSceneCompiler.Model;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler
{
    class Program
    {
        private static readonly string TestScenePath = @"..\..\..\..\Example\TestScene"; //Assume Proj/bin/{Debug|Release}

        static void Main(string[] args)
        {
            //Convert skybox texture.
            using (var file = File.OpenRead(Path.Combine(TestScenePath, @"Resource\473-free-hdri-skies-com.hdr")))
            {
                var image = new HdrImageLoader().Read(file);
                image.WriteSRDFile(Path.Combine(TestScenePath, @"Compiled\473-free-hdri-skies-com.srd"));
                var cubeImages = new SkyboxSphericalToCube<R32G32B32A32F>(image).Generate(256);
                SkyboxSphericalToCube<R32G32B32A32F>.WriteSRD(new SRDFile(Path.Combine(TestScenePath, @"Compiled\sphere_cube.srd")), cubeImages);
            }

            //Generate
            Sphere.WriteSRD(Path.Combine(TestScenePath, @"Compiled\sphere.vb"), Path.Combine(TestScenePath, @"Compiled\sphere.ib"));
        }
    }
}
