using PbrSceneCompiler.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Model
{
    class Sphere
    {
        private static List<SegmentedCylinder.Segment> PostProcess(params IEnumerable<SegmentedCylinder.Segment>[] input)
        {
            bool fix = false;
            List<SegmentedCylinder.Segment> ret = new List<SegmentedCylinder.Segment>();
            var inputArray = input.SelectMany(x => x).ToArray();
            for (int i = 0; i < inputArray.Length; ++i)
            {
                var s = inputArray[i];
                if (i != 0 && i != inputArray.Length - 1)
                {
                    s.Rotation0Fraction = s.RotationStepFraction / 2 * (fix ? 1 : 0);
                }
                //Can't fix adjacent non-continuous segments (in which case we must specify when creating the segments).
                if (!s.ContinuousNormal)
                {
                    if (s.ForwardBefore == Vector3.Zero && i != 0)
                    {
                        s.ForwardBefore = inputArray[i - 1].ForwardDirection;
                    }
                    if (s.ForwardAfter == Vector3.Zero && i != inputArray.Length - 1)
                    {
                        s.ForwardAfter = inputArray[i + 1].ForwardDirection;
                    }
                }
                ret.Add(s);
                fix = !fix;
            }
            return ret;
        }

        private static IEnumerable<SegmentedCylinder.Segment> GenSphere(Vector3 center, Vector3 z, Vector3 x,
            float r, float fracStart, float fracEnd, float utotal)
        {
            const float FractionalStep = 1f / 30;

            var fracDiff = fracEnd - fracStart;
            int steps = (int)Math.Ceiling(fracDiff / FractionalStep);
            var fracStep = fracDiff / steps;
            for (int i = 0; i <= steps; ++i)
            {
                var frac = fracStart + i * fracStep;
                var cos = (float)Math.Cos(frac * Math.PI);
                var sin = (float)Math.Sin(frac * Math.PI);
                var rot = FractionalStep;
                if (i == 0 && fracStart == 0)
                {
                    sin = 0;
                    cos = 1;
                    rot = 1;
                }
                else if (i == steps && fracEnd == 1)
                {
                    sin = 0;
                    cos = -1;
                    rot = 1;
                }
                yield return new SegmentedCylinder.Segment
                {
                    Center = center - cos * z * r,
                    ForwardDirection = z,
                    UpDirection = x,
                    RadiusX = sin * r,
                    RadiusY = sin * r,
                    ContinuousNormal = i != 0 && i != steps,
                    UStep = utotal / steps,
                    RotationStepFraction = rot,
                };
            }
        }

        public static void Generate(out SegmentedCylinder.Vertex[] vb, out short[] ib)
        {
            var s = (float)Math.Sqrt(1f / 2);
            var model = SegmentedCylinder.Generate(PostProcess(GenSphere(new Vector3(), new Vector3(0, 0, 1), new Vector3(1, 0, 0), 1, 0, 1, 1)));
            vb = model.Vertices;
            for (int i = 0; i < vb.Length; ++i)
            {
                var vv = vb[i];
                vv.UV = new Vector2(vv.UV.Y, 1 - vv.UV.X);
                vb[i] = vv;
            }
            List<short> ibList = new List<short>();
            foreach (var tri in model.Triangles)
            {
                ibList.Add((short)tri.V1);
                ibList.Add((short)tri.V2);
                ibList.Add((short)tri.V3);
            }
            ib = ibList.ToArray();
        }

        public static void WriteSRD(string vbfile, string ibfile)
        {
            Generate(out var vb, out var ib);
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
                        Width = (ushort)vb.Length,
                        Height = 1,
                        Depth = 1,
                        Stride = 32,
                        Slice = 0,
                    },
                });
                srd.WriteOffset(0);
                srd.Write(vb);
                srd.Close();
            }
            {
                var srd = new SRDFile(ibfile);
                srd.WriteHeaders(new SRDFileHeader
                {
                    Magic = SRDFile.Magic[0],
                    Format = 57 /* DXGI_FORMAT_R16_UINT */,
                    ArraySize = 1,
                    MipLevel = 1,
                }, new[] {
                    new SRDSegmentHeader
                    {
                        Offset = 0,
                        Width = (ushort)ib.Length,
                        Height = 1,
                        Depth = 1,
                        Stride = 2,
                        Slice = 0,
                    },
                });
                srd.WriteOffset(0);
                srd.Write(ib);
                srd.Close();
            }
        }
    }
}
