using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoBaseMediaFileFormatParser.QuickTime
{
    public class QTAtom :Atom
    {
        public int AtomID
        {
            get;
            set;
        }

        public short ChildCount
        {
            get;
            set;
        }
    }
}
