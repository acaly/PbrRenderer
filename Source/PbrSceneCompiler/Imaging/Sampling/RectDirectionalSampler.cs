using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Imaging.Sampling
{
    //Take samples around a directional 3D vector. The length of this direction or samples has no effect.
    struct RectDirectionalSampler
    {
        public RectDirectionalSampler(Random rand, Vector3 center, float r, int n)
        {
            _center = Vector3.Normalize(center);
            _r = (float)Math.Tan(r);
            _n = n;
            _rand = rand;
            CalculateXY(_center, out _x, out _y);
            _x *= r;
            _y *= r;
            _ss = _center - _n * 0.5f * _x - _n * 0.5f * _y;
        }

        private readonly Vector3 _center;
        private readonly float _r;
        private readonly int _n;

        private readonly Vector3 _x;
        private readonly Vector3 _y;

        private readonly Vector3 _ss;

        private readonly Random _rand;

        private static void CalculateXY(Vector3 center, out Vector3 x, out Vector3 y)
        {
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
            y = Vector3.Cross(center, x);
            x = Vector3.Cross(center, y);
        }

        //i must go through [0, n*n-1]
        public Vector3 Sample(int i)
        {
            var iy = i / _n;
            var ix = i - iy * _n;
            var o = _ss + ix * _x + iy * _y;

            var randx = _rand.Next(32) / 32f;
            var randy = _rand.Next(32) / 32f;

            o += randx * _x + randy * _y;
            return o;
        }
    }
}
