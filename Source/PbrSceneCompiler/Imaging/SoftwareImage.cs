using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Imaging
{
    interface ISoftwarePixelFormatTranform<T> where T : unmanaged
    {
        T Scale(T val, float factor);
        T Add(T a, T b);
        uint DXGIFormat { get; }
        Color ToColor(T val);
    }

    class SoftwareImage<T> where T : unmanaged
    {
        public SoftwareImage(int w, int h, ISoftwarePixelFormatTranform<T> p)
        {
            Width = w;
            Height = h;
            PixelTransformer = p;
            RawData = new T[w * h];
        }

        public readonly T[] RawData; //TODO block optimization?
        public ISoftwarePixelFormatTranform<T> PixelTransformer { get; }
        public int Width { get; }
        public int Height { get; }

        public ref T GetPixel(int x, int y)
        {
            return ref RawData[x + y * Width];
        }

        public Bitmap ToBitmap()
        {
            var ret = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    ret.SetPixel(x, y, PixelTransformer.ToColor(GetPixel(x, y)));
                }
            }
            return ret;
        }

        public void WriteSRDFile(string filename)
        {
            var file = new SRDFile(filename);
            file.WriteHeaders(new SRDFileHeader
            {
                Magic = SRDFile.Magic[2],
                Format = PixelTransformer.DXGIFormat,
                MipLevel = 1,
                ArraySize = 1,
            }, new[] {
                new SRDSegmentHeader
                {
                    Offset = 0,
                    Width = (ushort)Width,
                    Height = (ushort)Height,
                    Depth = 1,
                    Stride = (ushort)(16 * Width),
                    Slice = 0,
                },
            });
            file.WriteOffset(0);
            file.Write(RawData);
            file.Close();
        }

        public static void WriteSRDFileCube(string filename, SoftwareImage<T>[] images)
        {
            var file = new SRDFile(filename);
            var size = images[0].Width;
            file.WriteHeaders(new SRDFileHeader
            {
                Magic = SRDFile.Magic[2],
                Format = images[0].PixelTransformer.DXGIFormat,
                MipLevel = 1,
                ArraySize = 1,
            }, new[] {
                new SRDSegmentHeader
                {
                    Offset = 0,
                    Width = (ushort)size,
                    Height = (ushort)size,
                    Depth = 6,
                    Stride = (ushort)(Marshal.SizeOf<T>() * size),
                    Slice = (uint)(Marshal.SizeOf<T>() * size * size),
                },
            });
            file.WriteOffset(0);
            file.Write(images[0].RawData);
            file.Write(images[1].RawData);
            file.Write(images[2].RawData);
            file.Write(images[3].RawData);
            file.Write(images[4].RawData);
            file.Write(images[5].RawData);
            file.Close();
        }
    }
}
