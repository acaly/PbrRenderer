using PbrSceneCompiler.Imaging.Hdr;
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
            var input = Path.Combine(TestScenePath, @"Resource\473-free-hdri-skies-com.hdr");
            var output = Path.Combine(TestScenePath, @"Compiled\473-free-hdri-skies-com.srd");
            using (var file = File.OpenRead(input))
            {
                new HdrImageLoader().Read(file).WriteSRDFile(output);
            }

            //Generate
            Sphere.WriteSRD(Path.Combine(TestScenePath, @"Compiled\sphere.vb"), Path.Combine(TestScenePath, @"Compiled\sphere.ib"));
        }
    }
}
