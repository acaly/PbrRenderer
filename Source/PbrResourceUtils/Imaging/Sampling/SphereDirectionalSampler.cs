using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PbrResourceUtils.Imaging.Sampling
{
    public struct SphereDirectionalSampler
    {
        public SphereDirectionalSampler(Random rand, Vector3 normalizedCenter, float th, int nLatitude)
        {
            _th = th;
            _nlati = nLatitude;
            _rand = rand;

            var tatiTotal = 1 - (float)Math.Cos(th);
            _deltaLati = tatiTotal / nLatitude;
            float nLongi = nLatitude / tatiTotal * 2;
            _deltaLongi = (float)Math.PI * 2 / nLongi;
            SampleCount = nLatitude * (int)nLongi;

            _center = normalizedCenter;
            CalculateXY(normalizedCenter, out _x, out _y);
        }

        private readonly float _th;
        private readonly int _nlati;
        private readonly Random _rand;

        private readonly float _deltaLati, _deltaLongi;

        private readonly Vector3 _center;
        private readonly Vector3 _x, _y;

        public int SampleCount { get; }

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
            y = Vector3.Normalize(Vector3.Cross(center, x));
            x = Vector3.Cross(center, y);
        }

        private void SampleRaw(int i, out float lati, out float longi)
        {
            int ix = i / _nlati;
            int iy = i - ix * _nlati;

            longi = ix * _deltaLongi;
            longi += _rand.Next(32) * _deltaLongi / 32;

            var lati_p = iy * _deltaLati;
            lati_p += _rand.Next(32) * _deltaLati / 32 + 0.0001f;
            if (lati_p > 0.999f) lati_p = 0.999f;
            lati = (float)Math.Sqrt(1 / (1 - lati_p) / (1 - lati_p) - 1); //1-y=cos, longi=tan
        }

        public Vector3 Sample(int i)
        {
            SampleRaw(i, out var lati, out var longi);
            var xx = lati * (float)Math.Sin(longi);
            var yy = lati * (float)Math.Cos(longi);
            return Vector3.Normalize(_center + xx * _x + yy * _y);
        }
    }
}
