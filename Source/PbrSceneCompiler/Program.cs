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
                SoftwareImage<R32G32B32A32F> image = new HdrImageLoader().Read(file);
                {
                    var s1 = new HalfSize<R32G32B32A32F>(image).Generate();
                    s1 = new HalfSize<R32G32B32A32F>(s1).Generate();
                    s1 = new HalfSize<R32G32B32A32F>(s1).Generate();
                    s1 = new HalfSize<R32G32B32A32F>(s1).Generate();
                    s1 = new HalfSize<R32G32B32A32F>(s1).Generate();
                    image = s1;
                }
                image.WriteSRDFile(Path.Combine(TestScenePath, @"Compiled\473-free-hdri-skies-com.srd"));
                {
                    var cubeImages = new SkyboxUniformCircleFilter<R32G32B32A32F>(image).Generate(16);
                    SoftwareImage<R32G32B32A32F>.WriteSRDFileCube(Path.Combine(TestScenePath, @"Compiled\sphere_cube_specular.srd"), cubeImages);
                }
                {
                    var cubeImages = new SkyboxLambertianDiffuseFilter<R32G32B32A32F>(image).Generate(16, 64);
                    for (int i = 0; i < 6; ++i)
                    {
                        cubeImages[i] = new HalfSize<R32G32B32A32F>(cubeImages[i]).Generate();
                        cubeImages[i] = new HalfSize<R32G32B32A32F>(cubeImages[i]).Generate();
                    }
                    SoftwareImage<R32G32B32A32F>.WriteSRDFileCube(Path.Combine(TestScenePath, @"Compiled\sphere_cube_diffuse.srd"), cubeImages);
                }
            }

            //Generate model.
            Sphere.WriteSRD(Path.Combine(TestScenePath, @"Compiled\sphere.vb"), Path.Combine(TestScenePath, @"Compiled\sphere.ib"));
        }
    }
}
