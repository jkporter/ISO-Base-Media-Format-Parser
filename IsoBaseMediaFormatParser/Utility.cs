using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace IsoBaseMediaFileFormatParser
{
    public static class Conversions
    {
        public static readonly byte[] Iso12Bytes = new byte[] { 0x00, 0x11, 0x00, 0x10, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71 };
        public static readonly DateTime Epoch = new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static readonly DateTime MaxDateTimeFromUnsignedInt32 = Epoch.AddSeconds(uint.MaxValue);
        public static readonly ulong MaxUnsignedInt64DateTime = DateTimeToUnsignedInt64(DateTime.MaxValue);

        public static readonly uint FlagsMaxValue = 16777216;

        public static Encoding ISO88591
        {
            get
            {
                return Encoding.GetEncoding("ISO-8859-1");
            }
        }

        public static Encoding UTF8
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        public static Encoding UTF16
        {
            get
            {
                return Encoding.BigEndianUnicode;
            }
        }

        public static uint GetType(string s)
        {
            if (s.Length != 4)
                throw new ArgumentOutOfRangeException();

            byte[] bytes = OrderBytesInBigEndian(ISO88591.GetBytes(s));

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static string GetTypeAsString(uint type)
        {
            return ISO88591.GetString(OrderBytesInBigEndian(BitConverter.GetBytes(type)));
        }

        public static string GetUserTypeAsString(byte[] userType)
        {
            if (userType.Length != 16)
                throw new ArgumentException();

            return new Guid(userType).ToString();
        }

        public static Guid FormUuid(uint type, byte[] b)
        {
            byte[] typeBytes = OrderBytesInBigEndian(BitConverter.GetBytes(type));
            return new Guid(typeBytes.Concat(b).ToArray());
        }

        public static byte[] OrderBytesInBigEndian(byte[] value)
        {
            return BitConverter.IsLittleEndian ? value.Reverse().ToArray() : value;
        }

        public static bool IsIsoUuid(Guid uuid)
        {
            return uuid.ToByteArray().Skip(4).SequenceEqual(Iso12Bytes);
        }

        public static DateTime GetDateTime(uint secondsFromEpoch)
        {
            return Epoch.AddSeconds(secondsFromEpoch);
        }

        public static DateTime GetDateTime(ulong secondsFromEpoch)
        {
            return Epoch.AddSeconds(secondsFromEpoch);
        }

        public static uint DateTimeToUnsignedInt32(DateTime time)
        {
            double seconds = Math.Floor((time.ToUniversalTime() - Epoch).TotalSeconds);
            if (seconds > uint.MaxValue || seconds < uint.MinValue)
                throw new ArgumentOutOfRangeException();
            return (uint)seconds;
        }

        public static ulong DateTimeToUnsignedInt64(DateTime time)
        {
            double seconds = Math.Floor((time.ToUniversalTime() - Epoch).TotalSeconds);
            if (seconds > ulong.MaxValue || seconds < ulong.MinValue)
                throw new ArgumentOutOfRangeException();
            return (ulong)seconds;
        }

        public static decimal GetFixedPointNumber(uint number, uint m, uint n)
        {
            //float d = number;
            //d

            return 0;
        }

        public static BitArray GetFlags(uint value)
        {
            if (value > FlagsMaxValue)
                throw new ArgumentOutOfRangeException();
            return new BitArray(OrderBytesInBigEndian(BitConverter.GetBytes(value)).Skip(1).ToArray());
        }
        
        public static uint GetFlagsValue(BitArray flags)
        {
            byte[] bytes = new byte[4];
            flags.CopyTo(bytes, 0);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();

            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}
