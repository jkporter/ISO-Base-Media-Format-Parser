using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IsoBaseMediaFileFormatParser;

namespace IsoBaseMediaFileFormatReaderDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read))
            {
                using (IsoBaseMediaFileFormatReader reader = new IsoBaseMediaFileFormatReader(fs))
                {
                    f(reader);
                }
            }
        }

        static void f(IsoBaseMediaFileFormatReader reader)
        {
            const int indent = 4;

            long totalSize = 0;
            int totalBoxes = 0;
            long mediaData = 0;
            long totalFreeBoxSpace = 0;

            while (reader.Read())
            {
                Console.WriteLine("{0}Box {1} @ {2} of size: {3}, ends @ {4} {5}",
                    string.Empty.PadRight(reader.Depth * indent),
                    reader.GetTypeAsString(),
                    reader.BoxPosition,
                    reader.CalculatedSize + (reader.Size == 0 ? " (0*)" : string.Empty),
                    reader.BoxPosition + reader.CalculatedSize,
                    (reader.IsRecognizedType && reader.IsRecognizedVersion.GetValueOrDefault(true)) ? string.Empty : "~");
                if (reader.Depth == 0)
                    totalSize += reader.CalculatedSize;
                if (reader.TypeString == "mdat")
                    mediaData = reader.CalculatedSize;
                if (reader.TypeString == "free" || reader.TypeString == "skip")
                    totalFreeBoxSpace += reader.CalculatedSize;
                totalBoxes++;
            }
            Console.WriteLine("                        (*)denotes length of atom goes to End-of-File");
            Console.WriteLine();
            Console.WriteLine(" ~ denotes an unknown box");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("Total size: {0} bytes; {1} atoms total.", totalSize, totalBoxes);
            Console.WriteLine("Media data: {0} bytes; {1} bytes all other boxes ({2} box overhead).", mediaData, totalSize - mediaData, ((totalSize - mediaData) / (double)totalSize).ToString("0.000%"));
            Console.WriteLine("Total free box space: {0} bytes; {1} waste. Padding avaliable: {2} bytes.", totalFreeBoxSpace, (totalFreeBoxSpace / (double)totalSize).ToString("0.000%"), "?");
            Console.WriteLine("------------------------------------------------------");
            Console.ReadLine();
        }
    }
}
