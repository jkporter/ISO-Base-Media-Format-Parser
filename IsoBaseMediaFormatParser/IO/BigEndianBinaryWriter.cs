using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace IsoBaseMediaFileFormatParser.IO
{
    public class BigEndianBinaryWriter:BinaryWriter
    {
        public BigEndianBinaryWriter()
            : base()
        {
        }

        public BigEndianBinaryWriter(Stream output)
            : base(output)
        {
        }

        public BigEndianBinaryWriter(Stream output, Encoding encoding)
            : base(output, encoding)
        {
        }

        public override void Write(decimal value)
        {
            base.Write(value);
        }

        public override void Write(double value)
        {
            WriteInBigEndianOrder(BitConverter.GetBytes(value)); 
        }

        public override void Write(float value)
        {
            WriteInBigEndianOrder(BitConverter.GetBytes(value)); 
        }

        public override void Write(int value)
        {
            WriteInBigEndianOrder(BitConverter.GetBytes(value)); 
        }

        public override void Write(long value)
        {
            WriteInBigEndianOrder(BitConverter.GetBytes(value)); 
        }

        public override void Write(short value)
        {
            WriteInBigEndianOrder(BitConverter.GetBytes(value)); 
        }

        public override void Write(uint value)
        {
            WriteInBigEndianOrder(BitConverter.GetBytes(value)); 
        }

        public override void Write(ulong value)
        {
            WriteInBigEndianOrder(BitConverter.GetBytes(value));
        }

        public override void Write(ushort value)
        {
            WriteInBigEndianOrder(BitConverter.GetBytes(value));
        }

        private void WriteInBigEndianOrder(byte[] buffer)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);
            Write(buffer);
        }
    }
}
