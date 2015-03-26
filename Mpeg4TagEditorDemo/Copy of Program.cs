using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mpeg4Tagging;
using System.Collections;
using Mpeg4Tagging.WindowsMedia;
using Mpeg4Tagging.ITunes;
using System.IO;
using IsoBaseMediaFileFormatParser;

namespace Mpeg4TagEditorDemo
{
    class Program2
    {
        static string[] iTunesMetaPath = new string[] { "moov", "udta", "meta" };
        static string[] iTunesMetaHandlerPath = new string[] { "moov", "udta", "meta", "hdlr" };
        static string[] iTunesMetaListPath = new string[] { "moov", "udta", "meta", "ilst" };


        static PathTracker iTunesMeta = new PathTracker(iTunesMetaPath);
        static PathTracker iTunesMetaHandler = new PathTracker(iTunesMetaHandlerPath);
        static PathTracker iTunesMetaList = new PathTracker(iTunesMetaListPath);
        

        static void Main(string[] args)
        {
            /* iTunesMetaHandler.EqualsChanged += new PathTracker.OffPathEventHandler(iTunesMetaHandler_EqualsChanged);
            List<PathTracker> trackers = new List<PathTracker>();
            trackers.Add(iTunesMeta);
            trackers.Add(iTunesMetaHandler);
            trackers.Add(iTunesMetaList);

            using (FileStream fs = new FileStream(@"D:\Users\Jonathan\Documents\Visual Studio 2010\Projects\IsoBaseMediaFormatParser\trailer.m4v", FileMode.Open, FileAccess.Read))
            {
                using (FileStream output = new FileStream(@"test.m4v", FileMode.Create, FileAccess.Write))
                {
                    IsoBaseMediaFileFormatReader reader = new IsoBaseMediaFileFormatReader(fs);
                    Stack<uint> boxes = new Stack<uint>();

                    IsoBaseMediaFormatWriterWithDepth writer = new IsoBaseMediaFormatWriterWithDepth(output);

                    while (reader.Read())
                    {    
                        while (writer.Depth > reader.Depth)
                        {
                            writer.WriteEndBox();
                            boxes.Pop();
                            foreach (PathTracker t in trackers)
                                t.CurrentPath = boxes;
                        }

                        boxes.Push(reader.Type);

                        foreach (PathTracker t in trackers)
                            t.CurrentPath = boxes;

                        if (iTunesMetaList.OnPath)
                        {
                            if (boxes.Count == 6 && reader.GetTypeAsString() == "data")
                            {
                            }
                        }
                        else
                        {
                            if (reader.IsRecognizedType && reader.IsFullBox)
                                writer.WriteStartFullBox(reader.Type, reader.Version.Value, reader.Flags);
                            else
                                writer.WriteStartBox(reader.Type, reader.UserType);

                            if (!reader.IsContainer)
                            {
                                int bytesRead;
                                byte[] buffer = new byte[4096];
                                while ((bytesRead = reader.ReadContent(buffer, 0, buffer.Length)) > 0)
                                    writer.WriteContent(buffer, 0, bytesRead);
                            }
                        }
                    }

                    while (writer.Depth > reader.Depth)
                    {
                        writer.WriteEndBox();
                        boxes.Pop();
                    }
                }
            } */
        }

       /* static void iTunesMetaHandler_EqualsChanged(object sender, EventArgs e)
        {
            reader.ReadContentAsUnsignedInt32();
            handlerType = reader.ReadContentAsUnsignedInt32();
            reader.ReadContent(new byte[12], 0, 12);

            byte[] buffer = new byte[reader.Size];
            int bytesRead = reader.ReadContent(buffer, 0, buffer.Length);

            Console.WriteLine("Handler");
            Console.WriteLine("    Type: " + Utility.GetTypeAsString(handlerType.Value));
            Console.WriteLine("    Name: " + Encoding.UTF8.GetString(buffer, 0, bytesRead));
        }

        static void CopyBox(IsoBaseMediaFileFormatReader reader, IsoBaseMediaFormatWriter writer)
        {
            if (reader.IsRecognizedBoxType() && reader.IsFullBox())
                writer.WriteStartFullBox(reader.Type, reader.Version.Value, reader.Flags);
            else
                writer.WriteStartBox(reader.Type, reader.UserType);

            if (!reader.IsContainer.GetValueOrDefault())
            {
                int bytesRead;
                byte[] buffer = new byte[4096];
                while ((bytesRead = reader.ReadContent(buffer, 0, buffer.Length)) > 0)
                    writer.WriteContent(buffer, 0, bytesRead);
            }
        }


        static object ReadData(IsoBaseMediaFileFormatReader reader)
        {
            byte[] padding = new byte[4];
            reader.ReadContent(padding, 0, 4);

            byte[] flags = new byte[4];
            reader.Flags.CopyTo(flags, 1);
            if (BitConverter.IsLittleEndian)
                flags = flags.Reverse().ToArray();

            byte[] buffer = new byte[(int)reader.Size];
            int returedBytes = reader.ReadContent(buffer, 0, buffer.Length);

            buffer = buffer.Take(returedBytes).ToArray();

            if (buffer.Length > 0)
            {
                uint flagsValue = BitConverter.ToUInt32(flags, 0);
                switch (flagsValue)
                {
                    case 1:
                        return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    case 21:
                        return BitConverter.ToUInt32(BitConverter.IsLittleEndian ? buffer.Reverse().ToArray() : buffer, 0);
                }
            }

            return buffer;
        }

        static void WriteMeta(IsoBaseMediaFormatWriter writer, Tags tags)
        {
            writer.WriteStartFullBox(Utility.GetType("meta"), 0, new BitArray(new byte[] { 0, 0, 0 }));

            writer.WriteStartBox(Utility.GetType("hdlr"));
            writer.WriteContent(new byte[] { 0, 0, 0, 0 }, 0, 4);
            writer.WriteContent(BitConverter.GetBytes((uint)1835297138).Reverse().ToArray(), 0, 4);
            writer.WriteContent(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 12);
            writer.WriteContent(new byte[] { 0 }, 0, 1);
            writer.WriteEndBox();

            writer.WriteStartBox(Utility.GetType("ilst"));

            foreach (string type in tags.tags.Keys)
            {
                BitArray flags = null;
                byte[] data = null;


                object d = tags.tags[type];
                if (d is string)
                {
                    flags = new BitArray(new byte[] { 0x00, 0x00, 0x01 });
                    data = Encoding.UTF8.GetBytes((string)d);
                }

                if (d is byte || d is uint)
                {
                    flags = new BitArray(new byte[] { 0x00, 0x00, 0x15 });
                    if (d is byte)
                        data = BitConverter.GetBytes((byte)d);
                    if (d is uint)
                        data = BitConverter.GetBytes((uint)d);

                    if (BitConverter.IsLittleEndian)
                        data = data.Reverse().ToArray();
                }

                if (data != null)
                {
                    writer.WriteStartBox(Utility.GetType(type));
                    writer.WriteStartFullBox(Utility.GetType(type), 0, flags);
                    writer.WriteContent(data, 0, data.Length);
                    writer.WriteEndBox();
                    writer.WriteEndBox();
                }
            }
            writer.WriteEndBox();
            writer.WriteEndBox();
        }*/
    } 
}
