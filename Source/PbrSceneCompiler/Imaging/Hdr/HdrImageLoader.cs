using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Imaging.Hdr
{
    class HdrImageLoader
    {
        public HdrImageLoader()
        {
        }

        private byte[] _scanline;
        private byte[] _readBuffer = new byte[4];
        private StringBuilder _headerLine = new StringBuilder();
        private Stream _stream;
        private HdrImage _data;

        public HdrImage Read(Stream stream)
        {
            _stream = stream;
            ReadHeader();
            ReadData();
            return _data;
        }

        private void ReadHeader()
        {
            _data = new HdrImage();
            while (true)
            {
                var line = ReadHeaderLine();
                if (line.Length > 0 && (line[0] == '+' || line[0] == '-'))
                {
                    var xy = line.Split(' ');
                    _data.Width = int.Parse(xy[3]);
                    _data.Height = int.Parse(xy[1]);
                    break;
                }
            }
        }

        private void ReadData()
        {
            _scanline = new byte[4 * _data.Width];
            _data.RawData = new float[3 * _data.Width * _data.Height];
            for (int i = 0; i < _data.Height; ++i)
            {
                ReadScanline();
                CopyScanline(i);
            }
        }

        private byte ReadSingleByte()
        {
            int data = _stream.ReadByte();
            if (data == -1) throw new IOException();
            return (byte)data;
        }

        private string ReadHeaderLine()
        {
            _headerLine.Clear();
            byte data = ReadSingleByte();
            while (data != 0x0A)
            {
                _headerLine.Append((char)data);
                data = ReadSingleByte();
            }
            return _headerLine.ToString();
        }

        private void CopyScanline(int y)
        {
            var dataStride = 3 * _data.Width;
            for (int x = 0; x < _data.Width; ++x)
            {
                ConvertColor(x * 4, x * 3 + dataStride * y);
            }
        }

        private int lastWrite = 0;

        private void ConvertColor(int srcOffset, int destOffset)
        {
            int r = _scanline[srcOffset + 0];
            int g = _scanline[srcOffset + 1];
            int b = _scanline[srcOffset + 2];
            int e = _scanline[srcOffset + 3];
            if (e == 0)
            {
                _data.RawData[destOffset + 0] = 0;
                _data.RawData[destOffset + 1] = 0;
                _data.RawData[destOffset + 2] = 0;
            }
            else
            {
                var f = (float)Math.Pow(2, e - 128 - 8);
                _data.RawData[destOffset + 0] = r * f;
                _data.RawData[destOffset + 1] = g * f;
                _data.RawData[destOffset + 2] = b * f;
                lastWrite = destOffset;
                if (lastWrite == 25165812)
                {

                }
            }
        }

        private void ReadScanline()
        {
            if (4 != _stream.Read(_readBuffer, 0, 4))
            {
                throw new IOException();
            }
            if (_readBuffer[0] == 2 &&
                _readBuffer[1] == 2 &&
                _readBuffer[2] * 256 + _readBuffer[3] == _data.Width)
            {
                //compressed
                ReadCompressedChannel(0);
                ReadCompressedChannel(1);
                ReadCompressedChannel(2);
                ReadCompressedChannel(3);
            }
            else
            {
                //uncompressed
                _scanline[0] = _readBuffer[0];
                _scanline[1] = _readBuffer[1];
                _scanline[2] = _readBuffer[2];
                _scanline[3] = _readBuffer[3];
                _stream.Read(_scanline, 4, _scanline.Length - 4);
            }
        }

        private void ReadCompressedChannel(int ch)
        {
            int read = 0;
            while (read < _data.Width)
            {
                int len = ReadSingleByte();
                if (len > 128)
                {
                    //run
                    byte c = ReadSingleByte();
                    for (int i = 0; i < len - 128; ++i)
                    {
                        _scanline[ch + 4 * read] = c;
                        read += 1;
                    }
                }
                else
                {
                    for (int i = 0; i < len; ++i)
                    {
                        _scanline[ch + 4 * read] = ReadSingleByte();
                        read += 1;
                    }
                }
            }
            if (read != _data.Width)
            {
                throw new IOException();
            }
        }
    }
}
