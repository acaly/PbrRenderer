using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Imaging.Hdr
{
    class HdrImage
    {
        public int Width, Height;
        public float[] RawData;

        public Bitmap CreateGrayFromLogAlpha()
        {
            var ret = new Bitmap(Width, Height);
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    var r = RawData[0 + x * 3 + y * Width * 3];
                    var g = RawData[1 + x * 3 + y * Width * 3];
                    var b = RawData[2 + x * 3 + y * Width * 3];
                    var c = Math.Max(r, Math.Max(g, b));
                    var f = (int)(Math.Log10(c) * 3 + 128);
                    ret.SetPixel(x, y, Color.FromArgb(f, f, f));
                }
            }
            return ret;
        }

        public Bitmap CreateLogBitmap()
        {
            var ret = new Bitmap(Width, Height);
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    var r = RawData[0 + x * 3 + y * Width * 3];
                    var g = RawData[1 + x * 3 + y * Width * 3];
                    var b = RawData[2 + x * 3 + y * Width * 3];
                    var fr = (int)(Math.Log10(r) * 30 + 128);
                    var fg = (int)(Math.Log10(g) * 30 + 128);
                    var fb = (int)(Math.Log10(b) * 30 + 128);
                    ret.SetPixel(x, y, Color.FromArgb(fr, fg, fb));
                }
            }
            return ret;
        }

        public Bitmap CreateLinearBitmap()
        {
            var max = RawData.Max() / 100;
            var ret = new Bitmap(Width, Height);
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    var r = RawData[0 + x * 3 + y * Width * 3];
                    var g = RawData[1 + x * 3 + y * Width * 3];
                    var b = RawData[2 + x * 3 + y * Width * 3];
                    var fr = Math.Min((int)(r / max * 255), 255);
                    var fg = Math.Min((int)(g / max * 255), 255);
                    var fb = Math.Min((int)(b / max * 255), 255);
                    ret.SetPixel(x, y, Color.FromArgb(fr, fg, fb));
                }
            }
            return ret;
        }

        public void WriteSRDFile(string file)
        {
            var srd = new SRDFile(file);
            srd.WriteHeaders(new SRDFileHeader
            {
                Magic = SRDFile.Magic[2],
                Format = 6 /* DXGI_FORMAT_R32G32B32_FLOAT */,
                MipLevel = 1,
                ArraySize = 1,
            }, new[] {
                new SRDSegmentHeader
                {
                    Offset = 0,
                    Width = (ushort)Width,
                    Height = (ushort)Height,
                    Depth = 1,
                    Stride = (ushort)(12 * Width),
                    Slice = 0,
                },
            });
            srd.WriteOffset(0);
            srd.Write(RawData);
            srd.Close();
        }
    }
}
