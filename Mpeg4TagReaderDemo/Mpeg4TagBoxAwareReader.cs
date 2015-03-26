using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IsoBaseMediaFileFormatParser;

namespace Mpeg4Tagging
{
    public class Mpeg4TagBoxAwareReader : IsoBaseMediaFileFormatReader, IDisposable
    {
        public Mpeg4TagBoxAwareReader(Stream input)
            : base(input)
        {
        }

        public override bool Read()
        {
            bool success;
            bool ignoreAndSkip;

            do
            {
                success = Read(out ignoreAndSkip);
            } while (success && ignoreAndSkip);

            return success;
        }

        protected override bool Read(out bool ignoreAndSkip)
        {
            bool success = base.Read(out ignoreAndSkip);
            if (success && ignoreAndSkip)
            {
                switch (TypeString)
                {
                    case "ilst":
                    case "meta":
                    case "©alb":
                    case "©ART":
                    case "aART":
                    case "©cmt":
                    case "©day":
                    case "©nam":
                    case "©gen":
                    case "gnre":
                    case "trkn":
                    case "disk":
                    case "©wrt":
                    case "©too":
                    case "tmpo":
                    case "cprt":
                    case "cpil":
                    case "covr":
                    case "rtng":
                    case "grp":
                    case "stik":
                    case "pcst":
                    case "catg":
                    case "keyw":
                    case "purl":
                    case "egid":
                    case "desc":
                    case "©lyr":
                    case "tvnn":
                    case "tvsh":
                    case "tven":
                    case "tvsn":
                    case "tves":
                    case "purd":
                    case "pgap":
                    case "data":
                    case "Xtra":
                    case "ID32":
                        IsRecognizedType = true;
                        break;
                }

                if (IsRecognizedType)
                {
                    if (TypeString == "data" | TypeString == "ID32")
                    {
                        ReadFullBox();
                        IsRecognizedVersion = Version == 0;
                    }

                    if (IsRecognizedVersion.GetValueOrDefault(true))
                    {
                        switch (TypeString)
                        {
                            case "ilst":
                            case "meta":
                            case "©alb":
                            case "©ART":
                            case "aART":
                            case "©cmt":
                            case "©day":
                            case "©nam":
                            case "©gen":
                            case "gnre":
                            case "trkn":
                            case "disk":
                            case "©wrt":
                            case "©too":
                            case "tmpo":
                            case "cprt":
                            case "cpil":
                            case "covr":
                            case "rtng":
                            case "grp":
                            case "stik":
                            case "pcst":
                            case "catg":
                            case "keyw":
                            case "purl":
                            case "egid":
                            case "desc":
                            case "©lyr":
                            case "tvnn":
                            case "tvsh":
                            case "tven":
                            case "tvsn":
                            case "tves":
                            case "purd":
                            case "pgap":
                                IsContainer = true;
                                nextBoxPosition = reader.BaseStream.Position;
                                depths.Push(boxPosition + calculatedSize);
                                break;
                        }
                    }
                }

                ignoreAndSkip = !(IsRecognizedType && IsRecognizedVersion.GetValueOrDefault(true));
            }

            return success;
        }

        /* private bool InternalIsRecognizedBoxType()
        {
            if (Container.HasValue)
            {
                string containerAsString = Utility.GetTypeAsString(Container.GetValueOrDefault());

                if (GetTypeAsString() == "meta" && containerAsString == "udta")
                    return true;

                if (GetTypeAsString() == "ilst" && containerAsString == "meta")
                    return true;

                if (GetTypeAsString() == "data" || containerAsString == "ilst")
                    switch (containerAsString == "ilst" ? GetTypeAsString() : containerAsString)
                    {
                        case "©alb":
                        case "©ART":
                        case "aART":
                        case "©cmt":
                        case "©day":
                        case "©nam":
                        case "©gen":
                        case "gnre":
                        case "trkn":
                        case "disk":
                        case "©wrt":
                        case "©too":
                        case "tmpo":
                        case "cprt":
                        case "cpil":
                        case "covr":
                        case "rtng":
                        case "grp":
                        case "stik":
                        case "pcst":
                        case "catg":
                        case "keyw":
                        case "purl":
                        case "egid":
                        case "desc":
                        case "©lyr":
                        case "tvnn":
                        case "tvsh":
                        case "tven":
                        case "tvsn":
                        case "tves":
                        case "purd":
                        case "pgap":
                            return true;
                    }

                if (GetTypeAsString() == "Xtra" && containerAsString == "udta")
                    return true;
            }

            return false;
        }

        public override bool IsRecognizedBoxType()
        {
            return InternalIsRecognizedBoxType()
                || (GetTypeAsString() == "ID32" && Container.HasValue && Utility.GetTypeAsString(Container.GetValueOrDefault()) == "meta")
                ||  base.IsRecognizedBoxType();
        }

        public override bool IsFullBox()
        {
            if (InternalIsRecognizedBoxType())
                switch (GetTypeAsString())
                {
                    case "meta":
                    case "data":
                    case "ID32":
                        return true;
                    default:
                        return false;
                }

            return base.IsFullBox();
        }

        private bool InternalIsKnownBox()
        {
            if (InternalIsRecognizedBoxType())
                switch (GetTypeAsString())
                {
                    case "meta":
                    case "data":
                    case "ID32":
                        return Version == 0;
                    default:
                        return Version == null;
                }

            return false;
        }

        public override bool IsKnownBox()
        {
            return InternalIsKnownBox() || base.IsKnownBox();
        }

        public override bool IsContainerBox()
        {
            if (InternalIsKnownBox())
                switch (GetTypeAsString())
                {
                    case "data":
                    case "Xtra":
                        return false;
                    default:
                        return true;
                }

            return base.IsContainerBox();
        } */
    }
}
