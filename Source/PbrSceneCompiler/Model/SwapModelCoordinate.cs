using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Model
{
    class SwapModelCoordinate
    {
        public static void SwapXZ(SingleMaterialModel model)
        {
            for (int i = 0; i < model.Vertices.Length; ++i)
            {
                var v = model.Vertices[i];
                SwapFloat(ref v.Position.Z, ref v.Position.Y);
                SwapFloat(ref v.Normal.Z, ref v.Normal.Y);
                model.Vertices[i] = v;
            }
        }

        private static void SwapFloat(ref float a, ref float b)
        {
            float c = a;
            a = b;
            b = c;
        }
    }
}
