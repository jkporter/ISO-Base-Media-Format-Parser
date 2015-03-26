using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoBaseMediaFileFormatParser.NewFolder1
{
    public class Box
    {
        public Box(uint boxtype, byte[] extendedType)
        {
            uint size;
            uint type = boxtype;
            if(size == 1)
            {
                ulong largesize;
            }
            else if(size == 0)
            {
            }
        }

unsigned int(32) size;
unsigned int(32) type = boxtype;
if (size==1) {
unsigned int(64) largesize;
} else if (size==0) {
// box extends to end of file
}
if (boxtype==‘uuid’) {
unsigned int(8)[16] usertype = extended_type;
}
}
}
