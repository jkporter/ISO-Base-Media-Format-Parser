using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IsoBaseMediaFileFormatParser;

namespace CopyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FileStream fs = new FileStream(@"D:\Users\Jonathan\Documents\Visual Studio 2010\Projects\IsoBaseMediaFormatParser\trailer.m4v", FileMode.Open, FileAccess.Read))
            {
                using (FileStream output = new FileStream(@"test.m4v", FileMode.Create, FileAccess.Write))
                {
                    Stack<KeyValuePair<uint, bool>> boxes = new Stack<KeyValuePair<uint, bool>>();

                    IsoBaseMediaFileFormatReader reader = new IsoBaseMediaFileFormatReader(fs);
                    IsoBaseMediaFormatWriter writer = new IsoBaseMediaFormatWriter(output);
                    int writerDepth = 0;

                    while (reader.Read())
                    {
                        /* while (boxes.Count > reader.Depth)
                        {
                            if (boxes.Peek().Value)
                                writer.WriteBoxEnd();
                            boxes.Pop();
                        }

                        if (reader.Depth == boxes.Count)
                            boxes.Push(new KeyValuePair<uint, bool>(reader.Type, false));

                        //var boxTypes = boxes.Reverse().Select(b => Utility.GetTypeAsString(b.Key)).ToArray();

                        //Console.WriteLine(string.Join("->", boxTypes)); */

                        while (writerDepth > reader.Depth)
                        {
                            writer.WriteEndBox();
                            writerDepth--;
                        }

                        if (reader.IsFullBox)
                            writer.WriteStartFullBox(reader.Type, reader.Version.Value, reader.Flags);
                        else
                            writer.WriteStartBox(reader.Type, reader.UserType);
                        writerDepth++;
                        
                        if (!reader.IsContainer)
                        {
                            int bytesRead;
                            byte[] buffer = new byte[4096];
                            while ((bytesRead = reader.ReadContent(buffer, 0, buffer.Length)) > 0)
                                writer.WriteContent(buffer, 0, bytesRead);
                        }

                        //boxes.Push(new KeyValuePair<uint, bool>(boxes.Pop().Key, true));
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
