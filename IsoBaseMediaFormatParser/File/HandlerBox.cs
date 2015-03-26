using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IsoBaseMediaFileFormat.File
{
    public class HandlerBox : FullBox
    {
        public readonly uint[] Reserved = new UInt32[] { 0, 0, 0};

        public HandlerBox()
            : base("hdlr", 0, 0)
        {
            Stream input = null;

            PreDefined = 0;
            uint handlerTypeValue;
            if (ReadUnsignedInt32(input, out handlerTypeValue))
                HandlerType = handlerTypeValue;
            else
                throw new IOException();

            List<byte> bytes = new List<byte>();
            for (long c = 0; c < GetContentSize(); c++)
            {
                
            }
        }

        public HandlerBox(uint handlerType)
            : base("hdlr", 0, 0)
        {
            HandlerType = handlerType;
        }

        public uint PreDefined
        {
            get;
            set;
        }

        public uint HandlerType
        {
            get;
            set;
        }

        public string HandlerTypeAsString
        {
            get
            {
                return GetString(HandlerType);
            }
            set
            {
            }
        }

        public String Name
        {
            get;
            set;
        }
    }
}
