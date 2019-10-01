using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Imaging.Hdr
{
    class HdrImage : SoftwareImage<R32G32B32A32F>
    {
        public HdrImage(int w, int h) : base(w, h, new R32G32B32A32FTransformer())
        {
        }

        public Bitmap CreateGrayFromLogAlpha()
        {
            var ret = new Bitmap(Width, Height);
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    var p = GetPixel(x, y);
                    var c = Math.Max(p.R, Math.Max(p.G, p.B));
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
                    var p = GetPixel(x, y);
                    var fr = (int)(Math.Log10(p.R) * 30 + 128);
                    var fg = (int)(Math.Log10(p.G) * 30 + 128);
                    var fb = (int)(Math.Log10(p.B) * 30 + 128);
                    ret.SetPixel(x, y, Color.FromArgb(fr, fg, fb));
                }
            }
            return ret;
        }

        public Bitmap CreateLinearBitmap(float factor = 1)
        {
            var ret = new Bitmap(Width, Height);
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    var p = GetPixel(x, y);
                    var fr = Math.Min((int)(p.R * factor * 255), 255);
                    var fg = Math.Min((int)(p.G * factor * 255), 255);
                    var fb = Math.Min((int)(p.B * factor * 255), 255);
                    ret.SetPixel(x, y, Color.FromArgb(fr, fg, fb));
                }
            }
            return ret;
        }
    }
}
