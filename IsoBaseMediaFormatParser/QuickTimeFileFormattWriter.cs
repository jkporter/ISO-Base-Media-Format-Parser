using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using IsoBaseMediaFileFormatParser.IO;

namespace IsoBaseMediaFileFormatParser
{
    public class QuickTimeFileFormatWriter : IDisposable
    {
        private BigEndianBinaryWriter writer;

        private Stack<AtomData> atomStack = new Stack<AtomData>();

        private bool lastWasWideAtom = false;

        public QuickTimeFileFormatWriter(Stream output)
        {
            writer = new BigEndianBinaryWriter(output, Encoding.GetEncoding("ISO-8859-1"));
        }

        private void WriteAtomHeader(uint type, ulong extendedSize)
        {
            WriteAtomHeader(type, 1);
            writer.Write(extendedSize);
        }

        private void WriteAtomHeader(uint size, uint type)
        {
            writer.Write(size);
            writer.Write(type);
        }

        public virtual void WriteAtomHeader(uint type, bool extendedSize)
        {
            atomStack.Push(new AtomData()
            {
                type = type,
                PrecededByWideAtom = lastWasWideAtom,
                Position = writer.BaseStream.Position,
                Size = 0,
                IsExtendedSize = extendedSize
            });

            if (extendedSize)
            {
                WriteAtomHeader(type, (ulong)8);
                AdjustBoxSizes(16);
            }
            else
            {
                WriteAtomHeader(8u, type);
                AdjustBoxSizes(8);
            }
        }

        public virtual void WriteVersionAndFlagsFields(byte version, BitArray flags)
        {
            writer.Write(version);

            byte[] flagBuffer = new byte[3];
            flags.CopyTo(flagBuffer, 0);
            writer.Write(flagBuffer);
        }

        public virtual void WriteVersionAndFlagsFields()
        {
            WriteVersionAndFlagsFields(0, new BitArray(new byte[] { 0, 0, 0 }));
        }

        public virtual void WriteRawAtomData(byte[] buffer, int index, int count)
        {
            writer.Write(buffer, index, count);
            AdjustBoxSizes(count);
        }

        public virtual void WriteEndAtom()
        {
            var currentAtomData = atomStack.Peek();

            long currentPostion = writer.BaseStream.Position;

            writer.Seek((int)currentAtomData.Position, SeekOrigin.Begin);
            if(currentAtomData.IsExtendedSize)
            {
                writer.Seek(8, SeekOrigin.Current);
                writer.Write((ulong)currentAtomData.Size);
            }
            else if(currentAtomData.Size > uint.MaxValue)
            {
                if (currentAtomData.PrecededByWideAtom)
                {
                    writer.Seek(-8, SeekOrigin.Current);
                    WriteAtomHeader(currentAtomData.type, (ulong)currentAtomData.Size);
                }
                else
                {
                    throw new Exception();
                }
            }
            else
            {
                writer.Write((uint)currentAtomData.Size);
            }

            writer.Seek((int)currentPostion, SeekOrigin.Begin);
            lastWasWideAtom = currentAtomData.type == 1234;
            atomStack.Pop();
        }

        private void AdjustBoxSizes(int increaseBy)
        {
            if (increaseBy != 0)
            {
                foreach (var boxInfo in atomStack)
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
