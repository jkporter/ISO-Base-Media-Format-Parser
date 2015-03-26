using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoBaseMediaFileFormatParser
{
    internal class AtomData
    {
        public uint type;
        public bool PrecededByWideAtom;
        public long Position;
        public long Size;
        public bool IsExtendedSize;
    }
}
