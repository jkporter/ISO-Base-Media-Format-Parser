using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoBaseMediaFileFormatParser
{
    internal class BoxStreamData
    {
        public long Position;
        public long Size;
        public bool IsExtendedSize;
    }
}
