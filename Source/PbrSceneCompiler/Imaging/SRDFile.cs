using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Imaging
{
    struct SRDFileHeader
    {
        public uint Magic;
        public uint Format;
        public ushort MipLevel;
        public ushort ArraySize;
    }

    struct SRDSegmentHeader
    {
        public uint Offset;
        public ushort Width;
        public ushort Height;
        public ushort Depth;
        public ushort Stride;
        public uint Slice;
    }

    class SRDFile
    {
        public static uint[] Magic = new uint[]
        {
            0x30445253,
            0x31445253,
            0x32445253,
            0x33445253,
        };

        private FileStream _file;
        private BinaryWriter _bw;

        public SRDFile(string dest)
        {
            if (File.Exists(dest))
            {
                File.Delete(dest);
            }
            _file = File.Create(dest);
            _bw = new BinaryWriter(_file);
        }

        public void WriteHeaders(SRDFileHeader header, SRDSegmentHeader[] segments)
        {
            _bw.Write(header.Magic);
            _bw.Write(header.Format);
            _bw.Write(header.MipLevel);
            _bw.Write(header.ArraySize);

            foreach (var s in segments)
            {
                _bw.Write(0);
                _bw.Write(s.Width);
                _bw.Write(s.Height);
                _bw.Write(s.Depth);
                _bw.Write(s.Stride);
                _bw.Write(s.Slice);
            }
        }

        public void WriteOffset(int i)
        {
            _file.Position = 12 + 16 * i;
            _bw.Write((uint)_file.Length);
            _file.Seek(0, SeekOrigin.End);
        }

        public BinaryWriter GetWriter()
        {
            return _bw;
        }

        public void Close()
        {
            _bw.Flush();
            _file.Flush();
            _bw.Close();
            _file.Close();
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct HackWriteFloatArray
        {
            [FieldOffset(0)] public byte[] ByteArray;
            [FieldOffset(0)] public Array SourceArray;
            [FieldOffset(0)] public HackWriteFloatModifyLength ModifyLength;
        }

        private class HackWriteFloatModifyLength
        {
            public int Length;
        }

        public void Write<T>(T[] data) where T : unmanaged
        {
            var len = Marshal.SizeOf<T>();

            //.NET does not allow us to write float[]. We make a fake byte[] using explicit-layout struct.
            var conv = new HackWriteFloatArray
            {
                SourceArray = data
            };
            conv.ModifyLength.Length *= len;
            GetWriter().Write(conv.ByteArray, 0, conv.ModifyLength.Length);
            conv.ModifyLength.Length /= len;
        }
    }
}
