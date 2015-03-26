using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace IsoBaseMediaFileFormatParser.QuickTime
{
    interface IFullAtom
    {
        byte Version
        {
            get;
            set;
        }

        BitArray Flags
        {
            get;
            set;
        }
    }
}
