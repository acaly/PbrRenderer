using PbrResourceUtils.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PbrResourceUtils.Model
{
    public class Sphere
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

        public static SingleMaterialModel Generate()
        {
            return SegmentedCylinder.Generate(PostProcess(GenSphere(new Vector3(), new Vector3(0, 0, 1), new Vector3(1, 0, 0), 1, 0, 1, 1)));
        }
    }
}
