using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoBaseMediaFileFormatParser.QuickTime
{
    public class ContainerAtom:Atom
    {
        public Atom[] Children
        {
            get;
            set;
        }
    }
}
