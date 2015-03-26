namespace IsoBaseMediaFileFormat.File
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    public abstract class Box
    {
        internal uint size;
        internal uint type;
        internal ulong? largeSize = null;
        internal byte[] userType = null;
        internal bool isContainer = false;

        protected static byte[] iso12Bytes = new byte[] { 0x00, 0x11, 0x00, 0x10, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71 };
        protected static DateTime epoch = new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Box current = null;

        #region "Constructors"

        public Box(string boxType, byte[] extendedType = null):this(StringToUnsignedInt32(boxType), extendedType)
        {
        }

        public Box(uint boxType, byte[] extendedType = null)
        {
            Stream input = null;

            type = boxType;
            if (size == 1)
            {
                ulong largeSizeValue;
                if (ReadUnsignedInt64(input, out largeSizeValue))
                    largeSize = largeSizeValue;
                else
                    throw new IOException();
            }
            else if (size == 0)
            {
                // box extends to end of file
            }
            if (GetTypeAsString() == "uuid")
            {
                Guid userTypeValue;
                if (ReadUuid(input, out userTypeValue))
                    userType = userTypeValue.ToByteArray();
                else
                    throw new IOException();
            }
        }

        internal Box(Stream input)
        {
            if (!ReadUnsignedInt32(input, out size))
                throw new IOException();

            if (!ReadUnsignedInt32(input, out type))
                throw new IOException();

            if (size == 1)
            {
                ulong largeSizeValue;
                if (ReadUnsignedInt64(input, out largeSizeValue))
                    largeSize = largeSizeValue;
                else
                    throw new IOException();
            }

            if (GetTypeAsString() == "uuid")
            {
                Guid userTypeValue;
                if (ReadUuid(input, out userTypeValue))
                    userType = userTypeValue.ToByteArray();
                else
                    throw new IOException();
            }
        }

        #endregion

        #region "Properties"

        public ulong? Size
        {
            get
            {
                switch (size)
                {
                    case 0:
                        return null;
                    case 1:
                        return this.largeSize.Value;
                    default:
                        return this.size;
                }
            }
            set
            {
                if (value > uint.MaxValue)
                    this.SetLargeSize(value.Value);
                else
                    this.SetSize(value.HasValue ? new Nullable<uint>((uint)value.Value) : null);
            }
        }

        public uint Type
        {
            get
            {
                return this.type;
            }
            private set
            {
                this.type = value;
            }
        }

        public byte[] UserType
        {
            get
            {
                return this.userType;
            }
        }

        public Guid Uuid
        {
            get
            {
                return userType == null ? FormUuid(Type, iso12Bytes) : new Guid(UserType);
            }
            private set
            {
                if (IsIsoUuid(value))
                {
                    Type = BitConverter.ToUInt32(value.ToByteArray(), 0);
                    userType = null;
                }
                else
                {
                    Type = StringToUnsignedInt32("uuid");
                    userType = value.ToByteArray();
                }
            }
        }

        #endregion

        public static Encoding Iso88591Encoding
        {
            get
            {
                Encoding e = Encoding.GetEncoding("ISO-8859-1");
                e.EncoderFallback = new EncoderExceptionFallback();
                return e;
            }
        }

        #region "Public Instance Methods"


        public string GetTypeAsString()
        {
            return Box.GetString(this.type);
        }

        public void SetSize(uint? size)
        {
            this.size = size.GetValueOrDefault();
            this.largeSize = null;
        }

        public void SetLargeSize(ulong size)
        {
            this.size = 1;
            this.largeSize = size;
        }

        public void SetType(string type)
        {
            this.SetType(StringToUnsignedInt32(type));
        }

        public void SetType(uint type)
        {
            this.type = type;
        }

        #endregion

        protected virtual long GetHeaderSize()
        {
            return 8 + (size == 1 ? 8 : 0) + (userType == null ? 0 : 16);
        }

        protected long GetContentSize()
        {
            return (long)Size.Value - GetHeaderSize();
        }

        protected static bool Read(Stream input, byte[] buffer, bool readBigEndian = true)
        {
            int bytesRead = input.Read(buffer, 0, buffer.Length);
            if (readBigEndian && BitConverter.IsLittleEndian)
                buffer = buffer.Reverse().ToArray();

            if (bytesRead < buffer.Length)
                return false;

            return true;
        }

        protected static bool ReadUnsignedInt8(Stream input, out byte value)
        {

            int @return = input.ReadByte();
            if(@return == -1)
            {
                value = 0;
                return false;
            }

            value = (byte)@return;

            return true;
        }

        protected static bool ReadUnsignedInt32(Stream input, out uint value)
        {
            byte[] buffer = new byte[4];
            if (!Read(input, buffer))
            {
                value = 0;
                return false;
            }

            value = BitConverter.ToUInt32(buffer, 0);

            return true;
        }

        protected static bool ReadUnsignedInt64(Stream input, out ulong value)
        {
            byte[] buffer = new byte[8];
            if (!Read(input, buffer))
            {
                value = 0;
                return false;
            }

            value = BitConverter.ToUInt64(buffer, 0);

            return true;
        }

        protected static bool ReadUuid(Stream input, out Guid value)
        {
            byte[] buffer = new byte[16];
            if (!Read(input, buffer, false))
            {
                value = new Guid();
                return false;
            }

            value = new Guid(buffer);

            return true;
        }

        public static string GetString(uint value)
        {
            byte[] typeBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                typeBytes = typeBytes.Reverse().ToArray();

            return Iso88591Encoding.GetString(typeBytes);
        }

        public static DateTime GetDateTime(uint value)
        {
            return epoch.AddSeconds(value);
        }

        public static DateTime GetDateTime(ulong value)
        {
            return epoch.AddSeconds(value);
        }

        public static uint DateTimeToUnsignedInt32(DateTime time)
        {
            return (uint)(time.ToUniversalTime() - epoch).TotalSeconds;
        }

        public static ulong DateTimeToUnsignedInt64(DateTime time)
        {
            return (ulong)(time.ToUniversalTime() - epoch).TotalSeconds;
        }

        public static uint StringToUnsignedInt32(string s)
        {
            if (s.Length > 4)
                throw new ArgumentOutOfRangeException();
            s.PadRight(4, '\x00');

            byte[] bytes = Iso88591Encoding.GetBytes(s);
            
            return ((uint)bytes[3] << 0) | ((uint)bytes[2] << 8) | ((uint)bytes[1] << 16) | ((uint)bytes[0] << 24);
        }

        public static Guid FormUuid(uint type, byte[] b)
        {
            byte[] bytes = BitConverter.GetBytes(type);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            return new Guid(bytes.Concat(b).ToArray());
        }

        public static bool IsIsoUuid(Guid uuid)
        {
            return uuid.ToByteArray().Skip(4).SequenceEqual(iso12Bytes);
        }
    }
}
