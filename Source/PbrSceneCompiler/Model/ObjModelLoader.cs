using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Model
{
    static class ObjModelLoader
    {
        private class ObjModel
        {
            public List<Vector3> Position = new List<Vector3>();
            public List<Vector3> Normal = new List<Vector3>();
            public List<Vector2> TexCoord = new List<Vector2>();

            public List<ModelVertex> ModelVertices = new List<ModelVertex>();
            public Dictionary<string, int> ModelVertexString = new Dictionary<string, int>();

            public List<Triangle> Triangles = new List<Triangle>();
        }

        public static SingleMaterialModel Load(string filename)
        {
            using (var reader = new StreamReader(filename))
            {
                var model = new ObjModel();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line.Length == 0 || line[0] == '#') continue;

                    var seg = line.Split(' ');
                    if (seg[0] == "v")
                    {
                        model.Position.Add(new Vector3
                        {
                            X = float.Parse(seg[1]),
                            Y = float.Parse(seg[2]),
                            Z = float.Parse(seg[3]),
                        });
                    }
                    else if (seg[0] == "vt")
                    {
                        model.TexCoord.Add(new Vector2
                        {
                            X = float.Parse(seg[1]),
                            Y = float.Parse(seg[2]),
                        });
                    }
                    else if (seg[0] == "vn")
                    {
                        model.Normal.Add(new Vector3
                        {
                            X = float.Parse(seg[1]),
                            Y = float.Parse(seg[2]),
                            Z = float.Parse(seg[3]),
                        });
                    }
                    else if (seg[0] == "f")
                    {
                        model.Triangles.Add(new Triangle
                        {
                            V1 = GetVertexIndex(model, seg[1]),
                            V2 = GetVertexIndex(model, seg[2]),
                            V3 = GetVertexIndex(model, seg[3]),
                        });
                    }
                    else
                    {
                        throw new IOException("invalid OBJ file");
                    }
                }
                return new SingleMaterialModel
                {
                    Vertices = model.ModelVertices.ToArray(),
                    Triangles = model.Triangles.ToArray(),
                };
            }
        }

        private static int GetVertexIndex(ObjModel model, string str)
        {
            if (!model.ModelVertexString.TryGetValue(str, out var index))
            {
                ModelVertex mv = new ModelVertex();
                var ci = str.Split('/');
                mv.Position = model.Position[int.Parse(ci[0]) - 1];
                if (ci.Length >= 2 && ci[1].Length > 0)
                {
                    mv.UV = model.TexCoord[int.Parse(ci[1]) - 1];
                }
                if (ci.Length >= 3 && ci[2].Length > 0)
                {
                    mv.Normal = model.Normal[int.Parse(ci[2]) - 1];
                }

                index = model.ModelVertices.Count;
                model.ModelVertices.Add(mv);
                model.ModelVertexString.Add(str, index);
            }
            return index;
        }
    }
}
