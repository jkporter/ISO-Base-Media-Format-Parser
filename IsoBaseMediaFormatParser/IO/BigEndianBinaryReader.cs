using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace IsoBaseMediaFileFormatParser.IO
{
    public class BigEndianBinaryReader:BinaryReader
    {
        public BigEndianBinaryReader(Stream input)
            : base(input)
        {
        }

        public BigEndianBinaryReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }

        public override double ReadDouble()
        {
            return base.ReadDouble();
        }

        public override short ReadInt16()
        {
            return BitConverter.ToInt16(ReadBytesInBigEndian(2), 0);
        }

        public override int ReadInt32()
        {
            return BitConverter.ToInt32(ReadBytesInBigEndian(4), 0);
        }

        public override long ReadInt64()
        {
            return BitConverter.ToInt64(ReadBytesInBigEndian(8), 0);
        }

        public override float ReadSingle()
        {
            return base.ReadSingle();
        }

        public override ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadBytesInBigEndian(2), 0);
        }

        public override uint ReadUInt32()
        {
            return BitConverter.ToUInt32(ReadBytesInBigEndian(4), 0);
        }

        public override ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(ReadBytesInBigEndian(8), 0);
        }

        private byte[] ReadBytesInBigEndian(int count)
        {
            byte[] buffer = base.ReadBytes(count);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);
            return buffer;
        }
    }
}
