using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using IsoBaseMediaFileFormatParser.IO;

namespace IsoBaseMediaFileFormatParser
{
    public class IsoBaseMediaFileFormatReader : IDisposable
    {
        private static string[] recognizedTypes;
        private static string[] containerBoxTypes;
        private static string[] specialContainerBoxTypes;
        private static string[][] fullBoxes = new string[2][];

        protected BigEndianBinaryReader reader;

        private bool ignoreUnknownBoxes;

        protected long nextBoxPosition;
        private long inputEndPosition;

        protected long calculatedSize;
        protected int depth;
        protected Stack<long> depths = new Stack<long>();
        private Stack<uint> containers = new Stack<uint>();
        private bool isRecognizedType = false;
        private bool? isRecognizedVersion = null;

        protected long boxPosition;

        private bool? isContainer;
        private bool? isFullBox;

        private uint size;
        private uint type;
        private ulong? largeSize;

        private byte[] userType;

        private byte? version;
        private BitArray flags;

        static IsoBaseMediaFileFormatReader()
        {
            /* recognizedTypes = new string[] {
                "ftyp", "pdin", "moov", "mvhd", "trak", "tkhd", "tref", "edts", "elst", "mdia",
                "mdhd", "hdlr", "minf", "vmhd", "smhd", "hmhd", "nmhd", "dinf", "dref", "stbl",
                "stsd", "stts", "ctts", "stsc", "stsz", "stz2", "stco", "co64", "stss", "stsh",
                "padb", "stdp", "sdtp", "sbgp", "sgpd", "subs", "mvex", "mehd", "trex", "ipmc",
                "moof", "mfhd", "traf", "tfhd", "trun", "mfra", "tfra", "mfro", "mdat", "free",
                "skip", "udta", "cprt", "meta", "iloc", "ipro", "sinf", "frma", "imif", "schm",
                "schi", "iinf", "xml ", "bxml", "pitm", "fiin", "paen", "fpar", "fecr", "segr",
                "gitn", "tsel", "meco", "mere", "stsl", "btrt", "metx", "mett", "pasp", "clap",
                "url ", "urn " }; */

            recognizedTypes = new string[] {
                "btrt", "bxml", "cdsc", "clap", "co64", "cprt", "ctts", "dinf", "dref", "edts", "elst", "fecr", "fiin",
                "fpar", "free", "frma", "ftyp", "gitn", "hdlr", "hind", "hint", "hmhd", "iinf", "iloc", "imif",
                "ipmc", "ipro", "mdat", "mdhd", "mdia", "meco", "mehd", "mere", "meta", "mett", "metx", "mfhd",
                "mfra", "mfro", "minf", "moof", "moov", "mvex", "mvhd", "nmhd", "padb", "paen", "pasp",
                "pdin", "pitm", "rtp ", "sbgp", "schi", "schm", "sdtp", "segr", "sgpd", "sinf", "skip",
                "smhd", "snro", "srpp", "srtp", "stbl", "stco", "stdp", "stsc", "stsd", "stsh", "stsl", "stss", "stsz", "stts",
                "stz2", "subs", "tfhd", "tfra", "tims", "tkhd", "traf", "trak", "tref", "trex", "trun",
                "tsel", "tsro", "udta", "url ", "urn ", "vmhd", "xml "
            };

            containerBoxTypes = new string[] {
                "edts", "dinf", "fiin", "ipro", "mdia", "meco", "meta", "mfra", "minf", "moof",
                "moov", "mvex", "paen", "sinf", "stbl", "traf", "trak", "tref", "udta" };

            specialContainerBoxTypes = new string[] {
                "dref", "rtp ", "stsd", "srpp", "srtp"
            };

            fullBoxes[0] = new string[] {
                "pdin", "mvhd", "tkhd", "elst", "mdhd", "hdlr", "vmhd", "smhd", "hmhd", "nmhd",
                "url ", "urn ", "dref", "stsd", "stts", "ctts", "stsc", "stsz", "stz2", "stco", "co64", "stss",
                "stsh", "padb", "stdp", "stsl", "sdtp", "sbgp", "sgpd", "subs", "mehd", "trex", "ipmc",
                "mfhd", "tfhd", "trun", "tfra", "mfro", "cprt", "meta", "dref", "iloc",
                "ipro", "imif", "schm", "iinf", "xml ", "bxml", "pitm", "fiin", "fpar", "fecr",
                "gitn", "tsel", "mere", "stsl", "rtp ", "srpp" };

            fullBoxes[1] = new string[] {
                "elst", "dref", "hdlr", "ipmc", "mdhd", "mehd", "mfro", "mvhd", "tkhd", "sbgp", "subs", "sdtp", "sgpd" };
        }

        public IsoBaseMediaFileFormatReader(Stream input, bool ignoreUnknownBoxes = false)
        {
            reader = new BigEndianBinaryReader(input, Encoding.GetEncoding("ISO-8859-1"));
            this.ignoreUnknownBoxes = ignoreUnknownBoxes;
            nextBoxPosition = input.Position;
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

        public uint Size
        {
            get
            {
                return size;
            }
        }

        public ulong? LargeSize
        {
            get
            {
                return largeSize;
            }
        }

        public long CalculatedSize
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

        public string TypeString
        {
            get
            {
                return GetTypeAsString();
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

        public bool IsFullBox
        {
            get
            {
                return isFullBox.Value;
            }
            protected set
            {
                isFullBox = value;
            }
        }

        public bool IsContainer
        {
            get
            {
                return isContainer.Value;
            }
            protected set
            {
                isContainer = value;
            }
        }

        public long BoxPosition
        {
            get
            {
                return boxPosition;
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
                return reader.BaseStream.Position == reader.BaseStream.Length;
            }
        }

        public bool IsRecognizedType
        {
            get
            {
                return isRecognizedType;
            }
            protected set
            {
                isRecognizedType = value;
            }
        }

        public bool? IsRecognizedVersion
        {
            get
            {
                return isRecognizedVersion;
            }
            protected set
            {
                isRecognizedVersion = value;
            }
        }

        public string GetTypeAsString()
        {
            return Conversions.GetTypeAsString(Type);
        }

        public virtual bool Read()
        {
            bool success;
            bool ignoreAndSkip;
 
            do
            {
                success = Read(out ignoreAndSkip);
            } while (success && ignoreAndSkip);

            return success;
        }

        protected virtual bool Read(out bool ignoreAndSkip)
        {
            ignoreAndSkip = true;
            
            if (nextBoxPosition >= inputEndPosition)
                return false;

            if (isContainer.GetValueOrDefault())
                containers.Push(Type);

            if (reader.BaseStream.Position != nextBoxPosition)
                reader.BaseStream.Position = nextBoxPosition;

            isRecognizedType = false;
            isRecognizedVersion = null;

            largeSize = null;
            userType = null;
            version = null;
            flags = null;
            isContainer = null;
            isFullBox = null;

            boxPosition = reader.BaseStream.Position;

            size = reader.ReadUInt32();
            type = reader.ReadUInt32();
            if (size == 1)
                largeSize = reader.ReadUInt64();

            calculatedSize = (long)largeSize.GetValueOrDefault(size);
            if (calculatedSize == 0)
                calculatedSize = reader.BaseStream.Length - boxPosition;

            if (GetTypeAsString() == "uuid")
                userType = reader.ReadBytes(16);

            while (depths.Peek() == boxPosition)
                depths.Pop();

            depth = depths.Count - 1;

            while (containers.Count > depth)
                containers.Pop();

            if (isRecognizedType = IsARecognizedType(type))
            {
                if ((isFullBox = IsAFullBoxType(type)).Value)
                {
                    version = reader.ReadByte();
                    flags = new BitArray(reader.ReadBytes(3));

                    isRecognizedVersion = (version.Value == 0 | version.Value == 1) && fullBoxes[version.Value].Contains(GetTypeAsString());
                }

                ignoreAndSkip = !isRecognizedType || !isRecognizedVersion.GetValueOrDefault(true);
                if (!ignoreAndSkip && (isContainer = containerBoxTypes.Contains(GetTypeAsString())).Value)
                {
                    nextBoxPosition = reader.BaseStream.Position;
                    depths.Push(boxPosition + calculatedSize);
                }
            }

            if (!isContainer.GetValueOrDefault())
                nextBoxPosition = boxPosition + calculatedSize;

            return true;
        }

        protected void ReadFullBox()
        {
            version = reader.ReadByte();
            flags = new BitArray(reader.ReadBytes(3));
        }

        public int ReadContent(byte[] buffer, int index, int count)
        {
            long maxCountLong = Math.Max(boxPosition + calculatedSize - reader.BaseStream.Position, 0);
            int maxCount = (int)Math.Min(maxCountLong, int.MaxValue);
            count = isContainer.GetValueOrDefault() ? 0 : Math.Min(maxCount, count);
            return reader.Read(buffer, index, count);
        }

        private byte[] ReadBytesInMachineOrder(int count)
        {
            byte[] buffer = new byte[count];
            int bytesRead = ReadContent(buffer, 0, count);

            return BitConverter.IsLittleEndian ? buffer.Reverse().ToArray() : buffer;
        }

        public uint ReadContentAsUnsignedInt8()
        {
            return BitConverter.ToUInt32(ReadBytesInMachineOrder(1), 0);
        }

        public short ReadContentAsUnsignedInt16()
        {
            return BitConverter.ToInt16(ReadBytesInMachineOrder(2), 0);
        }

        public uint ReadContentAsUnsignedInt32()
        {
            return BitConverter.ToUInt32(ReadBytesInMachineOrder(4), 0);
        }

        public ulong ReadContentAsUnsignedInt64()
        {
            return BitConverter.ToUInt64(ReadBytesInMachineOrder(8), 0);
        }

        public short ReadContentAsInt16()
        {
            return BitConverter.ToInt16(ReadBytesInMachineOrder(2), 0);
        }

        public int ReadContentAsInt32()
        {
            return BitConverter.ToInt32(ReadBytesInMachineOrder(4), 0);
        }

        public long ReadContentAsInt64()
        {
            return BitConverter.ToInt64(ReadBytesInMachineOrder(8), 0);
        }

        public virtual void Close()
        {
            reader.Close();
        }

        private static bool IsARecognizedType(uint type)
        {
            string typeAsString = Conversions.GetTypeAsString(type);
            return recognizedTypes.Contains(typeAsString);
        }

        private static bool IsAFullBoxType(uint type)
        {
            string typeAsString = Conversions.GetTypeAsString(type);
            return fullBoxes[0].Contains(typeAsString) || fullBoxes[1].Contains(typeAsString);
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
