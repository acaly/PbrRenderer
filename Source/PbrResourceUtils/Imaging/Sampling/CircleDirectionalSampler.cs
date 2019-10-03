using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PbrResourceUtils.Imaging.Sampling
{
    public struct CircleDirectionalSampler
    {
        public CircleDirectionalSampler(Random rand, Vector3 center, float r, int n)
        {
            _rand = rand;
            _center = center;
            _n = n;
            CalculateXY(center, out _x, out _y);
            _x *= r;
            _y *= r;
        }

        private readonly Vector3 _center;
        private readonly Vector3 _x, _y;
        private readonly int _n;

        private readonly Random _rand;

        public int SampleCount => _n * _n;

        private static void CalculateXY(Vector3 center, out Vector3 x, out Vector3 y)
        {
            center = Vector3.Normalize(center);

            var dotX = center.X;
            var dotY = center.Y;
            if (dotX > dotY)
            {
                x = new Vector3(0, 1, 0);
            }
            else
            {
                x = new Vector3(1, 0, 0);
            }
            y = Vector3.Normalize(Vector3.Cross(center, x));
            x = Vector3.Cross(center, y);
        }

        public Vector3 Sample(int i)
        {
            int ix = i / _n;
            int iy = i - ix * _n;

            var aa = (ix + _rand.Next(32) / 32.0) / _n;
            var bb = (iy + _rand.Next(32) / 32.0) / _n;
            aa = aa * Math.PI * 2;
            bb = Math.Sqrt(bb);

            var xx = (float)(Math.Sin(aa) * bb);
            var yy = (float)(Math.Cos(aa) * bb);

            return Vector3.Normalize(_center + _x * xx + _y * yy);
        }
    }
}
