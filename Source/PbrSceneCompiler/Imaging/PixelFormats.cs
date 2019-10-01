using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Imaging
{
    struct R8G8B8A8
    {
        public byte R, G, B, A;
    }

    class R8G8B8A8Transformer : ISoftwarePixelFormatTranform<R8G8B8A8>
    {
        public uint DXGIFormat => 28 /* DXGI_FORMAT_R8G8B8A8_UNORM */;

        public R8G8B8A8 Add(R8G8B8A8 a, R8G8B8A8 b)
        {
            return new R8G8B8A8
            {
                R = (byte)Math.Min(255, a.R + b.R),
                G = (byte)Math.Min(255, a.G + b.G),
                B = (byte)Math.Min(255, a.B + b.B),
                A = Math.Max(a.A, b.A),
            };
        }

        public R8G8B8A8 Scale(R8G8B8A8 val, float factor)
        {
            return new R8G8B8A8
            {
                R = (byte)Math.Min(255, val.R * factor),
                G = (byte)Math.Min(255, val.G * factor),
                B = (byte)Math.Min(255, val.B * factor),
                A = val.A,
            };
        }

        public Color ToColor(R8G8B8A8 val)
        {
            return Color.FromArgb(val.A, val.R, val.G, val.B);
        }
    }

    struct R32G32B32A32F
    {
        public float R, G, B, A;
    }

    class R32G32B32A32FTransformer : ISoftwarePixelFormatTranform<R32G32B32A32F>
    {
        public uint DXGIFormat => 2 /* DXGI_FORMAT_R32G32B32A32_FLOAT */;

        public R32G32B32A32F Add(R32G32B32A32F a, R32G32B32A32F b)
        {
            return new R32G32B32A32F
            {
                R = a.R + b.R,
                G = a.G + b.G,
                B = a.B + b.B,
                A = Math.Max(a.A, b.A),
            };
        }

        public R32G32B32A32F Scale(R32G32B32A32F val, float factor)
        {
            return new R32G32B32A32F
            {
                R = val.R * factor,
                G = val.G * factor,
                B = val.B * factor,
                A = val.A,
            };
        }

        public Color ToColor(R32G32B32A32F val)
        {
            var a = (int)Math.Min(255, val.A * 255);
            var r = (int)Math.Min(255, val.R * 255);
            var g = (int)Math.Min(255, val.G * 255);
            var b = (int)Math.Min(255, val.B * 255);
            return Color.FromArgb(a, r, g, b);
        }
    }
}
