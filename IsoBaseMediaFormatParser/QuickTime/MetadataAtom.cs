using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoBaseMediaFileFormatParser.QuickTime
{
    class MetadataAtom : ContainerAtom
    {
        public MetadataHandlerAtom HandlerAtom
        {
            get;
            set;
        }
    }
}
