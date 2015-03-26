using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoBaseMediaFileFormatParser.QuickTime
{
    public class Atom
    {
        public uint Size
        {
            get;
            set;
        }

        public uint Type
        {
            get;
            set;
        }

        public ulong? ExtendedSize
        {
            get;
            set;
        }
    }
}
