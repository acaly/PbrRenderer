using PbrSceneCompiler.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Model
{
    class SingleMaterialModel
    {
        public ModelVertex[] Vertices;
        public Triangle[] Triangles;

        public void WriteSRD(string vbfile, string ibfile)
        {
            {
                var srd = new SRDFile(vbfile);
                srd.WriteHeaders(new SRDFileHeader
                {
                    Magic = SRDFile.Magic[0],
                    Format = 0,
                    ArraySize = 1,
                    MipLevel = 1,
                }, new[] {
                    new SRDSegmentHeader
                    {
                        Offset = 0,
                        Width = (ushort)Vertices.Length,
                        Height = 1,
                        Depth = 1,
                        Stride = 32,
                        Slice = 0,
                    },
                });
                srd.WriteOffset(0);
                srd.Write(Vertices);
                srd.Close();
            }
            {
                var srd = new SRDFile(ibfile);
                srd.WriteHeaders(new SRDFileHeader
                {
                    Magic = SRDFile.Magic[0],
                    Format = 42 /* DXGI_FORMAT_R32_UINT */,
                    ArraySize = 1,
                    MipLevel = 1,
                }, new[] {
                    new SRDSegmentHeader
                    {
                        Offset = 0,
                        Width = checked((ushort)(Triangles.Length * 3)),
                        Height = 1,
                        Depth = 1,
                        Stride = 4,
                        Slice = 0,
                    },
                });
                srd.WriteOffset(0);
                srd.Write(Triangles);
                srd.Close();
            }
        }
    }
}
