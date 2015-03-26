using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using IsoBaseMediaFileFormatParser.IO;

namespace IsoBaseMediaFileFormatParser
{
    public class IsoBaseMediaFormatWriter : IDisposable
    {
        private BigEndianBinaryWriter writer;

        private Stack<BoxStreamData> boxStack = new Stack<BoxStreamData>();

        public IsoBaseMediaFormatWriter(Stream output)
        {
            writer = new BigEndianBinaryWriter(output, Encoding.GetEncoding("ISO-8859-1"));
        }

        public virtual void WriteStartBox(uint type, byte[] extendedType = null)
        {
            boxStack.Push(new BoxStreamData() {
                Position = writer.BaseStream.Position,
                Size = 0,
                IsExtendedSize = false
            });

            writer.Write(0U);
            writer.Write(type);
            AdjustBoxSizes(8);
            if (type == Conversions.GetType("uuid"))
            {
                writer.Write(extendedType);
                AdjustBoxSizes(16);
            }
        }

        public virtual void WriteStartBox(string type, byte[] extendedType = null)
        {
            WriteStartBox(Conversions.GetType(type), extendedType);
        }

        public virtual void WriteStartFullBox(uint type, byte version, BitArray flags)
        {
            if (flags.Length != 24)
                throw new ArgumentOutOfRangeException("flags", flags.Length, "flags BitArray length must be equal to 24.");

            WriteStartBox(type, null);
            writer.Write(version);

            byte[] flagBuffer = new byte[3];
            flags.CopyTo(flagBuffer, 0);
            writer.Write(flagBuffer);

            AdjustBoxSizes(4);
        }

        public virtual void WriteStartFullBox(string type, byte version, BitArray flags)
        {
            WriteStartFullBox(Conversions.GetType(type), version, flags);
        }

        public virtual void WriteContent(byte[] buffer, int index, int count)
        {
            writer.Write(buffer, index, count);
            AdjustBoxSizes(count);
        }

        public virtual void WriteEndBox()
        {
            long currentPostion = writer.BaseStream.Position;

            writer.Seek((int)boxStack.Peek().Position, SeekOrigin.Begin);
            writer.Write((uint)boxStack.Peek().Size);

            writer.Seek((int)currentPostion, SeekOrigin.Begin);
            
            boxStack.Pop();
        }

        private void AdjustBoxSizes(int increaseBy)
        {
            if (increaseBy != 0)
            {
                foreach (var boxInfo in boxStack)
                    boxInfo.Size += increaseBy;
            }
        }

        public virtual void Close()
        {
            writer.Close();
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }
    }
}
