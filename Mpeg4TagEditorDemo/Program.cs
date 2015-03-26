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
    class Program
    {
        static byte[] ReadBytes(IsoBaseMediaFileFormatReader r, int count)
        {
            byte[] buffer = new byte[count];
            r.ReadContent(buffer, 0, count);
            return buffer;
        }

        static void Main(string[] args)
        {
            string[] moovMeta = new string[] { "moov", "udta", "meta" };
            string[] appleTagPath = new string[] { "moov", "udta", "meta", "ilst" };
            bool foundUserDataBox = false;
            uint? handlerType = null;
            Tags tags = new Tags();
            Tags currentTags = new Tags();

            const string s1 = @"D:\Users\Jonathan\Videos\test.m4v";
            const string s3 = @"D:\Users\Jonathan\Documents\Expression\Expression Encoder\Output\JONATHAN-PC 10-12-2010 4.31.43 AM\d.mp4";
            const string s2 = @"D:\Users\Jonathan\Documents\Visual Studio 2010\Projects\IsoBaseMediaFormatParser\trailer.m4v";

            using (FileStream fs = new FileStream(s3, FileMode.Open, FileAccess.Read))
            {
                using (FileStream output = new FileStream(@"test.m4v", FileMode.Create, FileAccess.Write))
                {
                    Stack<KeyValuePair<uint, bool>> boxes = new Stack<KeyValuePair<uint, bool>>();

                    IsoBaseMediaFileFormatReader reader = new Mpeg4TagBoxAwareReader(fs);
                    IsoBaseMediaFormatWriter writer = new IsoBaseMediaFormatWriter(output);

                    bool foundMetaPath = false;
                    bool endOnMetaPath = false;
                    int previousMetaPathLength = 0;

                    while (reader.Read())
                    {
                        while (boxes.Count > reader.Depth)
                        {
                            if (boxes.Peek().Value)
                                writer.WriteEndBox();
                            boxes.Pop();
                        }

                        if (reader.Depth == boxes.Count)
                            boxes.Push(new KeyValuePair<uint, bool>(reader.Type, false));

                        var boxTypes = boxes.Reverse().Select(b => Conversions.GetTypeAsString(b.Key)).ToArray();

                        int onMetaPathLength = OnPath(moovMeta, boxTypes);

                        foundMetaPath = foundMetaPath || onMetaPathLength > 0;
                        endOnMetaPath = foundMetaPath && onMetaPathLength < previousMetaPathLength;
                        foundUserDataBox = foundUserDataBox || onMetaPathLength == 2;

                        if (endOnMetaPath)
                        {
                            if ((handlerType.HasValue && Conversions.GetTypeAsString(handlerType.Value) == "mdir"))
                            {
                                foreach (string tag in currentTags.tags.Keys)
                                {
                                    if (!tags.tags.ContainsKey(tag))
                                        tags.tags.Add(tag, currentTags.tags[tag]);
                                }
                            }

                            if (!foundUserDataBox)
                                writer.WriteStartBox("udta");

                            WriteMeta(writer, tags);

                            if (!foundUserDataBox)
                                writer.WriteEndBox();
                        }

                        if (reader.GetTypeAsString() == "Xtra")
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {

                                byte[] buffer = new byte[4096];
                                int byteCount;
                                while ((byteCount = reader.ReadContent(buffer, 0, buffer.Length)) != 0)
                                    ms.Write(buffer, 0, byteCount);

                                ms.Position = 0;
                                while (ms.Position < ms.Length)
                                {
                                    var obj = Mpeg4Tagging.WindowsMedia.ContentDescriptor.Parse(ms);
                                    Console.WriteLine("Name: " + obj.Name);
                                    Console.WriteLine("Data: " + obj.GetValueAsObject().ToString() + ":");
                                }
                            }
                        }

                        if (OnPath(moovMeta, boxTypes) == 3 && reader.GetTypeAsString() == "hdlr")
                        {
                            reader.ReadContentAsUnsignedInt32();
                            handlerType = reader.ReadContentAsUnsignedInt32();
                            reader.ReadContent(new byte[12], 0, 12);

                            byte[] buffer = new byte[reader.Size];
                            int bytesRead = reader.ReadContent(buffer, 0, buffer.Length);

                            Console.WriteLine("Handler");
                            Console.WriteLine("    Type: " + Conversions.GetTypeAsString(handlerType.Value));
                            Console.WriteLine("    Name: " + Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        }

                        if (OnPath(appleTagPath, boxTypes) >= appleTagPath.Length && reader.GetTypeAsString() == "data")
                        {
                            byte[] flags = new byte[4];
                            reader.Flags.CopyTo(flags, 1);

                            flags = BitConverter.IsLittleEndian ? flags.Reverse().ToArray() : flags;

                            uint flagsValue = BitConverter.ToUInt32(flags, 0);

                            object data = ReadData(reader);
                            /* if (data is byte[])
                                Console.WriteLine(BitConverter.ToString(data as byte[]));
                            else
                                Console.WriteLine(data); */

                            currentTags.SetData(Conversions.GetTypeAsString(reader.Container.Value), data);
                        }
                        else if (onMetaPathLength < moovMeta.Length)
                        {
                            Console.WriteLine(string.Join("->", boxTypes));

                            if (reader.IsRecognizedType && reader.IsFullBox)
                                writer.WriteStartFullBox(reader.Type, reader.Version.Value, reader.Flags);
                            else
                                writer.WriteStartBox(reader.Type, reader.UserType);
                            if (!reader.IsContainer)
                            {
                                int bytesRead;
                                byte[] buffer = new byte[4096];
                                while ((bytesRead = reader.ReadContent(buffer, 0, buffer.Length)) > 0)
                                {
                                    writer.WriteContent(buffer, 0, bytesRead);
                                }
                            }

                            boxes.Push(new KeyValuePair<uint, bool>(boxes.Pop().Key, true));
                        }

                        previousMetaPathLength = onMetaPathLength;
                    }
                }
            }

            Console.ReadLine();
        }

        static int OnPath(string[] path, string[] currentPath)
        {
            int matching = 0;
            for (int i = 0; i < Math.Min(path.Length, currentPath.Length); i++)
                if (path[i] == currentPath[i])
                    matching++;
                else
                    break;

            return matching;
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
            writer.WriteStartFullBox(Conversions.GetType("meta"), 0, new BitArray(new byte[] { 0, 0, 0 }));

            writer.WriteStartBox(Conversions.GetType("hdlr"));
            writer.WriteContent(new byte[] { 0, 0, 0, 0 }, 0, 4);
            writer.WriteContent(BitConverter.GetBytes((uint)1835297138).Reverse().ToArray(), 0, 4);
            writer.WriteContent(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 12);
            writer.WriteContent(new byte[] { 0 }, 0, 1);
            writer.WriteEndBox();

            writer.WriteStartBox(Conversions.GetType("ilst"));

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
                    writer.WriteStartBox(Conversions.GetType(type));
                    writer.WriteStartFullBox(Conversions.GetType(type), 0, flags);
                    writer.WriteContent(data, 0, data.Length);
                    writer.WriteEndBox();
                    writer.WriteEndBox();
                }
            }
            writer.WriteEndBox();
            writer.WriteEndBox();
        }
    }
}
