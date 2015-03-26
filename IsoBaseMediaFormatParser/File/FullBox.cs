using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using IsoBaseMediaFileFormatParser;

namespace IsoBaseMediaFileFormat.File
{
    public abstract class FullBox:Box
    {
        public FullBox(uint boxType, byte v, BitArray f)
            : base(boxType)
        {
            Version = v;
            Flags = f;
        }
        
        public FullBox(uint boxType, byte v, uint f)
            : this(boxType, v, UInt32ToFlags(f))
        {
        }

        public FullBox(string boxType, byte v, BitArray f)
            : this(StringToUnsignedInt32(boxType), v, f)
        {
        }

        public FullBox(string boxType, byte v, uint f)
            : this(StringToUnsignedInt32(boxType), v, UInt32ToFlags(f))
        {
        }

        public FullBox(Stream input)
            : base(input)
        {
            int value = input.ReadByte();
            if (value != -1)
                Version = (byte)value;
            else
                throw new IOException();

            byte[] buffer = new byte[3];
            if (!Read(input, buffer))
                throw new IOException();

            Flags = new BitArray(buffer);
        }

        public byte Version
        {
            get;
            set;
        }

        public BitArray Flags
        {
            get;
            set;
        }

        protected override long GetHeaderSize()
        {
            return base.GetHeaderSize() + 4;
        }

        private static BitArray UInt32ToFlags(uint f)
        {
            if (f > 16777216)
                throw new ArgumentOutOfRangeException();

            byte[] bytes =  Conversions.OrderBytesInBigEndian(BitConverter.GetBytes(f));
            return new BitArray(bytes.Take(3).ToArray());
        }

        protected int GetFlagsValue()
        {
            return 0;
        }
    }
}
