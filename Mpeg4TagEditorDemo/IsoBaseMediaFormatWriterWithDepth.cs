using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IsoBaseMediaFileFormatParser;
using System.IO;

namespace Mpeg4TagEditorDemo
{
    class IsoBaseMediaFormatWriterWithDepth: IsoBaseMediaFormatWriter
    {
        public IsoBaseMediaFormatWriterWithDepth(Stream input)
            : base(input)
        {
        }

        public override void WriteStartBox(string type, byte[] extendedType = null)
        {
            base.WriteStartBox(type, extendedType);
            Depth++;
        }

        public override void WriteStartBox(uint type, byte[] extendedType = null)
        {
            base.WriteStartBox(type, extendedType);
            Depth++;
        }

        public override void WriteStartFullBox(string type, byte version, System.Collections.BitArray flags)
        {
            base.WriteStartFullBox(type, version, flags);
            Depth++;
        }

        public override void WriteStartFullBox(uint type, byte version, System.Collections.BitArray flags)
        {
            base.WriteStartFullBox(type, version, flags);
            Depth++;
        }

        public override void WriteEndBox()
        {
            base.WriteEndBox();
            Depth--;
        }

        public int Depth
        {
            get;
            private set;
        }
    }
}
