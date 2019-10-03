using PbrResourceUtils.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbrResourceUtils.Model
{
    public class SingleMaterialModel
    {
        public ModelVertex[] Vertices;
        public Triangle[] Triangles;

        private static ushort High(int val)
        {
            return (ushort)((uint)val >> 16);
        }

        private static ushort Low(int val)
        {
            return (ushort)((uint)val & 0xFFFF);
        }

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
                        Width = Low(Vertices.Length),
                        Height = High(Vertices.Length),
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
                        Width = Low(Triangles.Length * 3),
                        Height = High(Triangles.Length * 3),
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
