using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;

namespace IsoBaseMediaFileFormatParser
{
    public class IsoBaseMediaFileFormatReaderOld:IDisposable
    {
        private static InternalBoxDefinition[] knownBoxes;
        InternalBoxDefinition currentBoxDefinition;

        private Stream input;

        private bool ignoreUnknownBoxes;

        private long nextPosition;
        private long inputEndPosition;

        private long calculatedSize;
        private int depth;
        private Stack<long> depths = new Stack<long>();
        private Stack<uint> containers = new Stack<uint>();

        private long boxStartPosition;

        private bool? isContainer;

        private uint size;
        private uint type;
        private ulong? largeSize;

        private byte[] userType;

        private byte? version;
        private BitArray flags;
        
        static IsoBaseMediaFileFormatReaderOld()
        {
            knownBoxes = new InternalBoxDefinition[] { 
                new InternalBoxDefinition() { Types = new string[] { "ftyp" }, Containers = new string[] { null } },
                new InternalBoxDefinition() { Types = new string[] { "mdat" }, Containers = new string[] { null } },
                new InternalBoxDefinition() { Types = new string[] { "free", "skip" }, Containers = new string[] { null, string.Empty } },
                new InternalBoxDefinition() { Types = new string[] { "pdin" }, Versions = new byte[] { 0 }, Containers = new string[] { null } },
                new InternalBoxDefinition() { Types = new string[] { "moov" }, Containers = new string[] { null }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "mvhd" }, Versions = new byte[] { 0, 1 }, Containers = new string[] { "moov" } },
                new InternalBoxDefinition() { Types = new string[] { "trak" }, Containers = new string[] { "moov" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "tkhd" }, Versions = new byte[] { 0, 1 }, Containers = new string[] { "trak" } },
                new InternalBoxDefinition() { Types = new string[] { "tref" }, Containers = new string[] { "trak" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "hint", "cdsc", "hind" }, Containers = new string[] { "tref" } },
                new InternalBoxDefinition() { Types = new string[] { "mdia" }, Containers = new string[] { "trak" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "mdhd" }, Versions = new byte[] { 0, 1 }, Containers = new string[] { "mdia" } },
                new InternalBoxDefinition() { Types = new string[] { "hdlr" }, Versions = new byte[] { 0 }, Containers = new string[] { "mdia", "meta" } },
                new InternalBoxDefinition() { Types = new string[] { "minf" }, Containers = new string[] { "mdia" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "vmhd" }, Versions = new byte[] { 0 }, Containers = new string[] { "minf" } },
                new InternalBoxDefinition() { Types = new string[] { "smhd" }, Versions = new byte[] { 0 }, Containers = new string[] { "minf" } },
                new InternalBoxDefinition() { Types = new string[] { "hmhd" }, Versions = new byte[] { 0 }, Containers = new string[] { "minf" } },
                new InternalBoxDefinition() { Types = new string[] { "nmhd" }, Versions = new byte[] { 0 }, Containers = new string[] { "minf" } },
                new InternalBoxDefinition() { Types = new string[] { "stbl" }, Containers = new string[] { "minf" }, IsContainer = true }, // More boxes under stbl
                new InternalBoxDefinition() { Types = new string[] { "stsd" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "stdp" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                // stsl
                new InternalBoxDefinition() { Types = new string[] { "stts" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "ctts" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "stss" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "stsh" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "sdtp" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl", "traf" } },
                new InternalBoxDefinition() { Types = new string[] { "edts" }, Containers = new string[] { "trak" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "elst" }, Versions = new byte[] { 0, 1 }, Containers = new string[] { "edts" } },
                new InternalBoxDefinition() { Types = new string[] { "dinf" }, Containers = new string[] { "minf", "meta:" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "url " }, Versions = new byte[] { 0 }, Containers = new string[] { "dinf" } },
                new InternalBoxDefinition() { Types = new string[] { "urn " }, Versions = new byte[] { 0 }, Containers = new string[] { "dinf" } },
                new InternalBoxDefinition() { Types = new string[] { "dref" }, Versions = new byte[] { 0 }, Containers = new string[] { "dinf" } },
                new InternalBoxDefinition() { Types = new string[] { "stsz" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "stz2" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "stsc" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "stco" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "co64" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "padb" }, Versions = new byte[] { 0 }, Containers = new string[] { "stbl" } },
                new InternalBoxDefinition() { Types = new string[] { "subs" }, Versions = new byte[] { 0, 1 }, Containers = new string[] { "stbl", "traf" } },
                new InternalBoxDefinition() { Types = new string[] { "mvex" }, Containers = new string[] { "moov" }, IsContainer= true },
                new InternalBoxDefinition() { Types = new string[] { "mehd" }, Versions = new byte[] { 0, 1 }, Containers = new string[] { "mvex" } },
                new InternalBoxDefinition() { Types = new string[] { "trex" }, Versions = new byte[] { 0 }, Containers = new string[] { "mvex" } },
                new InternalBoxDefinition() { Types = new string[] { "moof" }, Containers = new string[] { null }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "mfhd" }, Versions = new byte[] { 0 }, Containers = new string[] { "moof" } },
                new InternalBoxDefinition() { Types = new string[] { "traf" }, Containers = new string[] { "moof" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "tfhd" }, Versions = new byte[] { 0 }, Containers = new string[] { "traf" } },
                new InternalBoxDefinition() { Types = new string[] { "trun" }, Versions = new byte[] { 0 }, Containers = new string[] { "traf" } },
                new InternalBoxDefinition() { Types = new string[] { "mfra" }, Containers = new string[] { "null" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "tfra" }, Containers = new string[] { "mfra" }},
                
                new InternalBoxDefinition() { Types = new string[] { "udta" }, Containers = new string[] { "moov", "trak" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "cprt" }, Versions = new byte[] { 0 }, Containers = new string[] { "udta" } },
                new InternalBoxDefinition() { Types = new string[] { "tsel" }, Versions = new byte[] { 0 }, Containers = new string[] { "udta" } },
                new InternalBoxDefinition() { Types = new string[] { "meta" }, Versions = new byte[] { 0 }, Containers = new string[] { null, "moov", "trak", "meco" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "xml" }, Versions = new byte[] { 0 }, Containers = new string[] { "meta" } },
                new InternalBoxDefinition() { Types = new string[] { "bxml" }, Versions = new byte[] { 0 }, Containers = new string[] { "meta" } },
                new InternalBoxDefinition() { Types = new string[] { "iloc" }, Versions = new byte[] { 0 }, Containers = new string[] { "meta" } },
                new InternalBoxDefinition() { Types = new string[] { "pitm" }, Versions = new byte[] { 0 }, Containers = new string[] { "meta" } },
                new InternalBoxDefinition() { Types = new string[] { "ipro" }, Versions = new byte[] { 0 }, Containers = new string[] { "meta" } },
                new InternalBoxDefinition() { Types = new string[] { "iinf" }, Versions = new byte[] { 0 }, Containers = new string[] { "meta" } },
                // speical boxes infe fdel
                new InternalBoxDefinition() { Types = new string[] { "meco" }, Containers = new string[] { "moov", "trak" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "mere" }, Versions = new byte[] { 0 }, Containers = new string[] { "meco" } },
                // sinf
                new InternalBoxDefinition() { Types = new string[] { "frma" }, Containers = new string[] { "sinf" } },
                new InternalBoxDefinition() { Types = new string[] { "imif" }, Versions = new byte[] { 0 }, Containers = new string[] { "sinf" } },
                new InternalBoxDefinition() { Types = new string[] { "ipmc" }, Versions = new byte[] { 0 }, Containers = new string[] { "moov", "meta" } },
                new InternalBoxDefinition() { Types = new string[] { "schm" }, Versions = new byte[] { 0 }, Containers = new string[] { "sinf", "srpp" } },
                new InternalBoxDefinition() { Types = new string[] { "schi" }, Containers = new string[] { "sinf", "srpp" }, IsContainer = true },
                new InternalBoxDefinition() { Types = new string[] { "fiin" }, Versions = new byte[] { 0 }, Containers = new string[] { "meta" }, IsContainer = false },

                

                new InternalBoxDefinition() { Types = new string[] { "fdsa" }, Containers = new string[] { "fdsa", }, IsContainer = false },
                
                new InternalBoxDefinition() { Types = new string[] { "fdpa" }, Containers = new string[] { "fdsa", }, IsContainer = false }
            };
        }

        public IsoBaseMediaFileFormatReaderOld(Stream input, bool ignoreUnknownBoxes = false)
        {
            this.input = input;
            this.ignoreUnknownBoxes = ignoreUnknownBoxes;
            nextPosition = input.Position;
            depths.Push(inputEndPosition = input.Length);
        }

        public uint? Container
        {
            get
            {
                if (containers.Count == 0)
                    return null;
                return containers.Peek();
            }
        }

        public long Size
        {
            get
            {
                return calculatedSize;
            }
        }

        public uint Type
        {
            get
            {
                return type;
            }
        }

        public byte[] UserType
        {
            get
            {
                return userType;
            }
        }

        public Guid Uuid
        {
            get
            {
                return userType == null ? Conversions.FormUuid(type, Conversions.Iso12Bytes) : new Guid(userType);
            }
        }

        public byte? Version
        {
            get
            {
                return version;
            }
        }

        public BitArray Flags
        {
            get
            {
                return flags;
            }
        }

        public bool? IsContainer
        {
            get
            {
                return isContainer;
            }
        }

        public long BoxStartPosition
        {
            get
            {
                return boxStartPosition;
            }
        }

        public int Depth
        {
            get
            {
                return depth;
            }
        }

        public bool EOF
        {
            get
            {
                return input.Position == input.Length;
            }
        }

        public string GetTypeAsString()
        {
            return Conversions.GetTypeAsString(Type);
        }

        public virtual bool Read()
        {
            if (nextPosition >= inputEndPosition)
                return false;

            if (isContainer.GetValueOrDefault())
                containers.Push(Type);

            if (input.Position != nextPosition)
                input.Position = nextPosition;

            boxStartPosition = input.Position;

            int headerSize = 8;

            size = ReadUnsignedInt32();
            type = ReadUnsignedInt32();
            if (size == 1)
            {
                headerSize += 8;
                largeSize = ReadUnsignedInt64();
            }
            else
            {
                largeSize = null;
            }

            calculatedSize = (long)largeSize.GetValueOrDefault(size);
            if (calculatedSize == 0)
                calculatedSize = input.Length - boxStartPosition;

            if (GetTypeAsString() == "uuid")
            {
                headerSize += 16;
                userType = ReadUuid().ToByteArray();
            }
            else
            {
                userType = null;
            }

            while (depths.Peek() == boxStartPosition)
                depths.Pop();

            depth = depths.Count - 1;

            while (containers.Count > depth)
                containers.Pop();

            if (IsRecognizedBoxType())
            {
                if (IsFullBox())
                {
                    headerSize += 4;
                    version = (byte)input.ReadByte();
                    flags = new BitArray(Read(3));
                }
                else
                {
                    version = null;
                    flags = null;
                }

                if (IsKnownBox())
                {
                    isContainer = IsContainerBox();
                    if (isContainer.Value)
                    {
                        nextPosition = input.Position;
                        depths.Push(boxStartPosition + calculatedSize);
                        isContainer = true;
                    }
                }
                else
                {
                    isContainer = null;
                }
            }
            else
            {
                isContainer = null;
                version = null;
                flags = null;
            }

            if (ignoreUnknownBoxes && (!IsRecognizedBoxType() || !IsKnownBox()))
                return Read();

            if (!IsRecognizedBoxType() || !IsKnownBox() || !isContainer.GetValueOrDefault())
                nextPosition = boxStartPosition + calculatedSize;

            return true;
        }

        public int ReadContent(byte[] buffer, int index, int count)
        {
            long maxCountLong = Math.Max(boxStartPosition + calculatedSize - input.Position, 0);
            int maxCount = (int)Math.Min(maxCountLong, int.MaxValue);
            count = isContainer.GetValueOrDefault() ? 0 : Math.Min(maxCount, count);
            return input.Read(buffer, index, count);
        }

        public uint ReadContentAsUnsignedInt32()
        {
            return BitConverter.ToUInt32(Conversions.OrderBytesInBigEndian(ReadContentAs(4)), 0);
        }

        public ulong ReadContentAsUnsignedInt64()
        {
            return BitConverter.ToUInt64(Conversions.OrderBytesInBigEndian(ReadContentAs(8)), 0);
        }

        private InternalBoxDefinition GetBox()
        {
            try
            {
                return currentBoxDefinition = knownBoxes
                    .Where(d => d.Types.Contains(GetTypeAsString()))
                    .Where(d => (!Container.HasValue && d.Containers.Contains(null)) || (Container.HasValue && (d.Containers.Contains(string.Empty) || d.Containers.Contains(Conversions.GetTypeAsString(Container.Value)))))
                    .SingleOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public virtual bool IsRecognizedBoxType()
        {
            return GetBox() != null;
        }

        public virtual bool IsFullBox()
        {
            return currentBoxDefinition.Versions != null || currentBoxDefinition.Types.Contains("tfra");
        }

        public virtual bool IsKnownBox()
        {
            return Version.HasValue ? currentBoxDefinition.Versions.Contains(Version.Value) : currentBoxDefinition.Versions == null || currentBoxDefinition.Types.Contains("tfra");
        }

        public virtual bool IsContainerBox()
        {
            return currentBoxDefinition.IsContainer;
        }

        protected byte[] Read(int count, bool readBigEndian = false)
        {
            byte[] buffer = new byte[count];
            int bytesRead = input.Read(buffer, 0, count);
            if (bytesRead < count)
                throw new IOException();

            if (readBigEndian && BitConverter.IsLittleEndian)
                buffer = buffer.Reverse().ToArray();

            return buffer;
        }

        protected uint ReadUnsignedInt32()
        {
            return BitConverter.ToUInt32(Read(4, true), 0);
        }

        protected ulong ReadUnsignedInt64()
        {
            return BitConverter.ToUInt64(Read(8, true), 0);
        }

        protected Guid ReadUuid()
        {
            return new Guid(Read(16));
        }

        private byte[] ReadContentAs(int count)
        {
            byte[] buffer = new byte[count];
            int readBytes = ReadContent(buffer, 0, buffer.Length);
            if (readBytes < count)
                throw new Exception();
            return buffer;
        }

        public void Dispose()
        {
        }
    }

    internal class InternalBoxDefinition
    {
        public string[] Types;
        public string[] Containers;
        public byte[] Versions;
        public bool IsContainer;
    }
}
