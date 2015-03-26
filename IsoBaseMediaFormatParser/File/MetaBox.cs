using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IsoBaseMediaFileFormat.File
{
    public class MetaBox : FullBox
    {
        public MetaBox(uint handlerType)
            : base("meta", 0, 0)
        {
            TheHandler = new HandlerBox(handlerType);
        }

        public HandlerBox TheHandler
        {
            get;
            private set;
        }

        public Box[] OtherBoxes
        {
            get;
            private set;
        }
    }
}
