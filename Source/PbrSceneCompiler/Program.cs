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
            //TODO move to separate C# project
            //{
            //    var conv = new SoftwareImage<R32G32B32A32F>(800, 600, new R32G32B32A32FNormalTransformer());
            //    conv.LoadRawDataFile(TestSceneCompiled("OutputViewDir.raw"));
            //    conv.ToBitmap().Save(TestSceneCompiled("OutputViewDir.png"));
            //}
            //return;

            //Convert skybox texture.
            using (var file = File.OpenRead(TestSceneResource("473-free-hdri-skies-com.hdr")))
            {
                SoftwareImage<R32G32B32A32F> image = new HdrImageLoader().Read(file);
                {
                    var cubeImages = new SkyboxUniformCircleFilter<R32G32B32A32F>(image).Generate(32);
                    SoftwareImage<R32G32B32A32F>.WriteSRDFileCube(TestSceneCompiled("sphere_cube_specular.srd"), cubeImages);
                }
                {
                    var s1 = new HalfSize<R32G32B32A32F>(image).Generate();
                    s1 = new HalfSize<R32G32B32A32F>(s1).Generate();
                    s1 = new HalfSize<R32G32B32A32F>(s1).Generate();
                    s1 = new HalfSize<R32G32B32A32F>(s1).Generate();
                    s1 = new HalfSize<R32G32B32A32F>(s1).Generate();
                    image = s1;
                }
                image.WriteSRDFile(TestSceneCompiled("473-free-hdri-skies-com.srd"));
                {
                    var cubeImages = new SkyboxLambertianDiffuseFilter<R32G32B32A32F>(image).Generate(16, 64);
                    for (int i = 0; i < 6; ++i)
                    {
                        cubeImages[i] = new HalfSize<R32G32B32A32F>(cubeImages[i]).Generate();
                        cubeImages[i] = new HalfSize<R32G32B32A32F>(cubeImages[i]).Generate();
                    }
                    SoftwareImage<R32G32B32A32F>.WriteSRDFileCube(TestSceneCompiled("sphere_cube_diffuse.srd"), cubeImages);
                }
            }

            //Generate model.
            {
                var model = Sphere.Generate();
                model.WriteSRD(TestSceneCompiled("sphere.vb"), TestSceneCompiled("sphere.ib"));
            }
            {
                var model = ObjModelLoader.Load(TestSceneResource("bunny.obj.dat"));
                SwapModelCoordinate.SwapXZ(model);
                model.WriteSRD(TestSceneCompiled("bunny.vb"), TestSceneCompiled("bunny.ib"));
            }
        }
    }
}
