using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Model
{
    static class SegmentedCylinder
    {
        //All directions are assumed to be normalized
        public class Segment
        {
            public Vector3 Center;
            public Vector3 ForwardDirection;
            public Vector3 ForwardBefore, ForwardAfter; //Used only when !ContinuousNormal
            public Vector3 UpDirection;
            public float RadiusX, RadiusY;
            public float Rotation0Fraction; //In fraction of 2Pi
            public float RotationStepFraction;
            public float UStep;
            public bool ContinuousNormal;
        }

        private class GeneratingModel
        {
            //public List<RayTracingVertex> rtv = new List<RayTracingVertex>();
            public List<ModelVertex> rdv = new List<ModelVertex>();
            public List<Triangle> triangles = new List<Triangle>();

            public SingleMaterialModel ToModel()
            {
                return new SingleMaterialModel
                {
                    Vertices = rdv.ToArray(),
                    Triangles = triangles.ToArray(),
                };
            }

            public void UpdateNormal(int rdvIndex, Vector3 n)
            {
                ModelVertex v = rdv[rdvIndex];
                v.Normal = n / n.Length();
                rdv[rdvIndex] = v;
            }

            //public int AddRTV(RayTracingVertex v)
            //{
            //    var ret = rtv.Count;
            //    rtv.Add(v);
            //    return ret;
            //}

            public int AddRDV(ModelVertex v)
            {
                var ret = rdv.Count;
                rdv.Add(v);
                return ret;
            }

            public void AddTriangle(int v1, int v2, int v3)
            {
                triangles.Add(new Triangle
                {
                    V1 = v1,
                    V2 = v2,
                    V3 = v3,
                });
            }
        }

        public static SingleMaterialModel Generate(List<Segment> segments)
        {
            if (segments.Count < 2)
            {
                throw new InvalidOperationException();
            }
            var totalUDistance = segments.Sum(x => x.UStep);
            var currentU = 0f;

            GeneratingModel model = new GeneratingModel();
            List<int> lastSegment = new List<int>(); //rdv index
            List<int> thisSegment = new List<int>();
            int lastWrapPoint, thisWrapPoint;

            int GetLast(int index)
            {
                if (index == lastSegment.Count) return lastWrapPoint;
                return lastSegment[index];
            }
            int GetThis(int index)
            {
                if (index == thisSegment.Count) return thisWrapPoint;
                return thisSegment[index];
            }

            CreateVertices(segments, 0, currentU / totalUDistance, model, lastSegment, out lastWrapPoint);

            for (int i = 1; i < segments.Count; ++i)
            {
                //Creating new vertices
                currentU += segments[i].UStep;
                CreateVertices(segments, i, currentU / totalUDistance, model, thisSegment, out thisWrapPoint);

                //Clone old vertices
                if (!segments[i - 1].ContinuousNormal)
                {
                    CloneVertices(segments, i - 1, model, lastSegment, ref lastWrapPoint);
                }

                //Check index mapping
                bool lastF = segments[i - 1].Rotation0Fraction != 0;
                bool thisF = segments[i].Rotation0Fraction != 0;
                int lastN = lastSegment.Count - (lastF ? 1 : 0);
                int thisN = thisSegment.Count - (thisF ? 1 : 0);
                if (lastN != 1 && thisN != 1 && lastN != thisN)
                {
                    throw new ArgumentException("Vertices number mismatch");
                }
                if (lastN == 1 && thisN == 1)
                {
                    throw new ArgumentException("Invalid segment");
                }
                if (segments[i].Rotation0Fraction > segments[i].RotationStepFraction)
                {
                    throw new ArgumentException("Invalid rotation");
                }

                //Create triangles
                if (lastN == 1)
                {
                    //TODO make the order same for F and non-F
                    if (thisF)
                    {
                        model.AddTriangle(lastSegment[0], GetThis(0), GetThis(thisSegment.Count - 1));
                        for (int j = 0; j < thisSegment.Count - 2; ++j)
                        {
                            model.AddTriangle(lastSegment[0], GetThis(j + 1), GetThis(j));
                        }
                        model.AddTriangle(lastSegment[0], GetThis(thisSegment.Count), GetThis(thisSegment.Count - 2));
                    }
                    else
                    {
                        for (int j = 0; j < thisSegment.Count; ++j)
                        {
                            model.AddTriangle(lastSegment[0], GetThis(j + 1), GetThis(j));
                        }
                    }
                }
                else if (thisN == 1)
                {
                    if (lastF)
                    {
                        model.AddTriangle(thisSegment[0], GetLast(lastSegment.Count - 1), GetLast(0));
                        for (int j = 0; j < lastSegment.Count - 2; ++j)
                        {
                            model.AddTriangle(thisSegment[0], GetLast(j), GetLast(j + 1));
                        }
                        model.AddTriangle(thisSegment[0], GetLast(lastSegment.Count - 2), GetLast(lastSegment.Count));
                    }
                    else
                    {
                        for (int j = 0; j < lastSegment.Count; ++j)
                        {
                            model.AddTriangle(thisSegment[0], GetLast(j), GetLast(j + 1));
                        }
                    }
                }
                else
                {
                    bool old0new1 = lastF && !thisF;
                    int normalN = thisN;
                    if (lastF || thisF) normalN -= 1;
                    for (int j = 0; j < normalN; ++j)
                    {
                        var old0 = GetLast(j);
                        var old1 = GetLast(j + 1);
                        var new0 = GetThis(j);
                        var new1 = GetThis(j + 1);
                        if (old0new1)
                        {
                            model.AddTriangle(old0, old1, new1);
                            model.AddTriangle(old0, new1, new0);
                        }
                        else
                        {
                            model.AddTriangle(old0, old1, new0);
                            model.AddTriangle(new0, old1, new1);
                        }
                    }
                    if (lastF && thisF)
                    {
                        //ignore old0new1 flag
                        var old0 = lastSegment[normalN];
                        var old1 = lastSegment[normalN + 1];
                        var old1r = lastWrapPoint;
                        var old2 = lastSegment[0];
                        var new0 = thisSegment[normalN];
                        var new1 = thisSegment[normalN + 1];
                        var new1r = thisWrapPoint;
                        var new2 = thisSegment[0];
                        //ignore old0new1 flag
                        model.AddTriangle(old0, old1r, new1r);
                        model.AddTriangle(old0, new1r, new0);
                        model.AddTriangle(old1, old2, new2);
                        model.AddTriangle(old1, new2, new1);
                    }
                    else if (lastF)
                    {
                        var old0 = lastSegment[normalN];
                        var old1 = lastSegment[normalN + 1];
                        var old1r = lastWrapPoint;
                        var old2 = lastSegment[0];
                        var new0 = thisSegment[normalN];
                        var new2 = thisSegment[0];
                        var new2r = thisWrapPoint;
                        model.AddTriangle(old0, old1r, new2r);
                        model.AddTriangle(old1, old2, new2);
                        model.AddTriangle(new0, old0, new2r);
                    }
                    else if (thisF)
                    {
                        var old0 = lastSegment[normalN];
                        var old2 = lastSegment[0];
                        var old2r = lastWrapPoint;
                        var new0 = thisSegment[normalN];
                        var new1 = thisSegment[normalN + 1];
                        var new1r = thisWrapPoint;
                        var new2 = thisSegment[0];
                        model.AddTriangle(old0, old2r, new0);
                        model.AddTriangle(old2, new2, new1);
                        model.AddTriangle(old2r, new1r, new0);
                    }
                }

                //Move vertex list
                {
                    var e = lastSegment;
                    lastSegment = thisSegment;
                    thisSegment = e;
                    lastWrapPoint = thisWrapPoint;
                }
            }

            return model.ToModel();
        }

        private static void CreateVertices(List<Segment> segments, int segIndex, float u, GeneratingModel model, List<int> output, out int wrapPoint)
        {
            var seg = segments[segIndex];
            output.Clear();
            for (int i = 0; seg.Rotation0Fraction + seg.RotationStepFraction * i < 1; ++i)
            {
                var frac = seg.Rotation0Fraction + seg.RotationStepFraction * i;
                var pos = CalculatePosition(seg, frac);
                var rdx = model.AddRDV(new ModelVertex
                {
                    Position = pos,
                    Normal = CalculateNormal(segments, segIndex, frac, false),
                    UV = new Vector2(u, frac),
                });
                output.Add(rdx);
            }
            if (seg.Rotation0Fraction != 0)
            {
                if (output.Count < 2)
                {
                    throw new ArgumentException("Invalid segment");
                }
                var pos = (model.rdv[output[0]].Position + model.rdv[output[output.Count - 1]].Position) / 2;
                var rdx = model.AddRDV(new ModelVertex
                {
                    Position = pos,
                    Normal = CalculateNormal(segments, segIndex, 0, false),
                    UV = new Vector2(u, 0),
                });
                output.Add(rdx);
            }
            if (output.Count == 1)
            {
                wrapPoint = -1;

                //Move v to 0.5
                var rtv = model.rdv[output[0]];
                rtv.UV.Y = 0.5f;
                model.rdv[output[0]] = rtv;
            }
            else
            {
                var zero = output.Where(ii => model.rdv[ii].UV.Y == 0).First();
                var rdx = model.AddRDV(new ModelVertex
                {
                    Position = model.rdv[zero].Position,
                    Normal = model.rdv[zero].Normal,
                    UV = new Vector2(u, 1),
                });
                wrapPoint = rdx;
            }
        }

        private static void CloneVertices(List<Segment> segments, int segIndex, GeneratingModel model, List<int> output, ref int wrapPoint)
        {
            var input = output.ToArray();
            output.Clear();
            var seg = segments[segIndex];
            bool wrapProcessed = false;
            for (int i = 0; i < input.Length; ++i)
            {
                var rdv = model.rdv[input[i]];
                var deg = seg.Rotation0Fraction + seg.RotationStepFraction * i;
                if (deg >= 1)
                {
                    if (i != input.Length - 1) continue; //should never happen
                    deg = 0;
                }
                rdv.Normal = CalculateNormal(segments, segIndex, deg, true);
                output.Add(model.AddRDV(rdv));
                if (rdv.UV.Y == 0 && wrapPoint != -1)
                {
                    if (wrapProcessed) throw new Exception();
                    var rdv2 = new ModelVertex
                    {
                        Position = model.rdv[wrapPoint].Position,
                        Normal = rdv.Normal,
                        UV = model.rdv[wrapPoint].UV,
                    };
                    wrapPoint = model.AddRDV(rdv2);
                    wrapProcessed = true;
                }
            }

            if (!wrapProcessed && wrapPoint != -1) throw new Exception();
        }

        private static Vector3 CalculateNormal(List<Segment> segments, int segIndex, float rotationFrac, bool overrideContinuous)
        {
            Segment seg = segments[segIndex];
            if (seg.RotationStepFraction + seg.Rotation0Fraction >= 1)
            {
                //Single vertex
                var f = seg.ForwardDirection;
                int otherIndex;
                if (segIndex == 0)
                {
                    otherIndex = 1;
                }
                else if (segIndex == segments.Count - 1)
                {
                    otherIndex = segIndex - 1;
                }
                else
                {
                    //In the middle, return (0, 0, 0)
                    return new Vector3();
                }
                if (f.LengthSquared() == 0)
                {
                    f = segments[otherIndex].ForwardDirection;
                }
                if (otherIndex > segIndex)
                {
                    return -f;
                }
                return f;
            }

            //Normal case
            var nn = new Vector3();
            var surfaceF = new Vector3();

            if (segIndex > 0 && !overrideContinuous)
            {
                surfaceF += CalculateForward(segments, segIndex, rotationFrac);
                nn += CalculateRingNormal(seg, rotationFrac, true);
            }
            if (overrideContinuous ||
                segIndex < segments.Count - 1 && segments[segIndex].ContinuousNormal)
            {
                surfaceF += CalculateForward(segments, segIndex + 1, rotationFrac);
                nn += CalculateRingNormal(seg, rotationFrac, false);
            }

            var sflen = surfaceF.Length();
            if (sflen == 0) return new Vector3();
            surfaceF = surfaceF / sflen;

            var realn = nn - Vector3.Dot(nn, surfaceF) * surfaceF;
            var nnlen = realn.Length();
            if (nnlen == 0)
            {
                return new Vector3();
            }
            realn /= nnlen;
            return realn;
        }

        //surface forward direction segIndex-1 -> segIndex
        private static Vector3 CalculateForward(List<Segment> segments, int segIndex, float rotationFrac)
        {
            var seg1 = segments[segIndex - 1];
            var seg2 = segments[segIndex];
            var f1 = seg1.ForwardDirection;
            var ry1 = seg1.RadiusY;
            if (!seg1.ContinuousNormal)
            {
                f1 = seg1.ForwardAfter;
                ry1 *= Vector3.Dot(f1, seg1.ForwardDirection);
            }
            var f2 = seg2.ForwardDirection;
            var ry2 = seg2.RadiusY;
            if (!seg2.ContinuousNormal)
            {
                f2 = seg2.ForwardBefore;
                ry2 *= Vector3.Dot(f2, seg2.ForwardDirection);
            }
            var x1 = seg1.UpDirection;
            var tr1 = Matrix4x4.CreateFromAxisAngle(f1, (float)Math.PI / 2);
            var y41 = Vector4.Transform(x1, tr1);
            var y1 = new Vector3(y41.X, y41.Y, y41.Z);
            var x2 = seg2.UpDirection;
            var tr2 = Matrix4x4.CreateFromAxisAngle(f2, (float)Math.PI / 2);
            var y42 = Vector4.Transform(x2, tr2);
            var y2 = new Vector3(y42.X, y42.Y, y42.Z);

            var sin = (float)Math.Sin(rotationFrac * Math.PI * 2);
            var cos = (float)Math.Cos(rotationFrac * Math.PI * 2);

            var pos1 = x1 * seg1.RadiusX * cos + y1 * ry1 * sin;
            var pos2 = x2 * seg2.RadiusX * cos + y2 * ry2 * sin;

            var a = seg1.Center + pos1;
            var b = seg2.Center + pos2;
            var dir = b - a;
            return dir / dir.Length();
        }

        //isBefore=true: use 'before'; isBefore=false: use 'after'
        private static Vector3 CalculateRingNormal(Segment seg, float rotationFrac, bool isBefore)
        {
            var x = seg.UpDirection;
            var f = seg.ForwardDirection;
            var ry = seg.RadiusY;
            if (!seg.ContinuousNormal)
            {
                f = isBefore ? seg.ForwardBefore : seg.ForwardAfter;
                ry *= Vector3.Dot(f, seg.ForwardDirection);
            }
            var tr = Matrix4x4.CreateFromAxisAngle(f, (float)Math.PI / 2);
            var y4 = Vector4.Transform(x, tr);
            var y = new Vector3(y4.X, y4.Y, y4.Z);
            var sin = (float)Math.Sin(rotationFrac * Math.PI * 2);
            var cos = (float)Math.Cos(rotationFrac * Math.PI * 2);
            return x * ry * cos + y * seg.RadiusX * sin;
        }

        private static Vector3 CalculatePosition(Segment seg, float rotationFrac)
        {
            var x = seg.UpDirection;
            var tr = Matrix4x4.CreateFromAxisAngle(seg.ForwardDirection, (float)Math.PI / 2);
            var y4 = Vector4.Transform(x, tr);
            var y = new Vector3(y4.X, y4.Y, y4.Z);
            var sin = (float)Math.Sin(rotationFrac * Math.PI * 2);
            var cos = (float)Math.Cos(rotationFrac * Math.PI * 2);
            return seg.Center + x * seg.RadiusX * cos + y * seg.RadiusY * sin;
        }
    }
}
