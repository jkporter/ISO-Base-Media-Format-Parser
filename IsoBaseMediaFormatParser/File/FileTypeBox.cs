using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IsoBaseMediaFileFormat.File
{
    public class FileTypeBox:Box
    {
        protected uint majorBrand;
        protected uint minorVersion;
        protected uint[] compatibleBrands;

        public FileTypeBox()
            : base("ftyp")
        {
        }

        public FileTypeBox(Stream input)
            : base(input)
        {
            if (!ReadUnsignedInt32(input, out majorBrand))
                throw new IOException();

            if (!ReadUnsignedInt32(input, out minorVersion))
                throw new IOException();

            long dataSize = (long)Size.Value - (long)this.GetHeaderSize();

            compatibleBrands = new uint[(int)Math.Ceiling((double)dataSize / 4)];
            for (long c = 0; c < GetContentSize(); c += 4)
            {
                uint compatibleBrand;
                if (ReadUnsignedInt32(input, out compatibleBrand))
                    compatibleBrands[c / 4] = compatibleBrand;
                else
                    throw new IOException();
            }

        }

        public string MajorBrand
        {
            get
            {
                return Box.GetString(majorBrand);
            }
            set
            {
            }
        }

        public string MinorVersion
        {
            get
            {
                return Box.GetString(minorVersion);
            }
            set
            {
            }
        }

        public string[] CompatibleBrands
        {
            get
            {
                return compatibleBrands.Select(cb => Box.GetString(cb)).ToArray();
            }
            set
            {
            }
        }
    }
}
