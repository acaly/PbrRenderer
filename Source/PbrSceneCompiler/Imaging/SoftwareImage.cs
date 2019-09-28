using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Imaging
{
    interface ISoftwarePixelFormatTranform<T> where T : unmanaged
    {
        T Scale(T val, float factor);
        T Add(T a, T b);
        uint DXGIFormat { get; }
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
    }
}
