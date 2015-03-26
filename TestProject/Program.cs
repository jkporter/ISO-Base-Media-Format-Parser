using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IsoBaseMediaFileFormatParser;
using System.Text.RegularExpressions;
using System.Collections;

namespace Mpeg4Tagging
{
    class Program
    {
        static void Main(string[] args)
        {

            using (StreamReader reader = new StreamReader(@"D:\Users\Jonathan\Documents\Visual Studio 2010\Projects\IsoBaseMediaFormatParser\types.txt"))
            {
                string line = null;
                StringBuilder sb = new StringBuilder();
                List<string> types = new List<string>();
                while ((line = reader.ReadLine()) != null)
                {
                    string type = line.Substring(0, 4);
                    types.Add(string.Format("\"{0}\"", type));
                }

                types = types.Distinct().ToList();
                types.Sort();

                Console.Write(string.Join(", ", types.ToArray()));
                Console.Read();
            }
        }

        

        static void PopulateDB(string[] args)
        {
            using (DataClasses1DataContext context = new DataClasses1DataContext())
            {
                using (StreamReader r = new StreamReader(@"D:\Users\Jonathan\Desktop\spec.txt"))
                {
                    string line;
                    Regex typeRegex = new Regex(@"[`‘'](.{4})[’']", RegexOptions.IgnoreCase);
                    Regex containersRegex = new Regex(@"(File)|([`‘'](.{4})[’'])", RegexOptions.IgnoreCase);
                    Regex mandtoryRegex = new Regex(@"^Mandatory: (Yes|No)\.?$", RegexOptions.IgnoreCase);
                    Regex quantityRegex = new Regex(@"^Quantity: ((Exactly one( variant must be present)?)|(Any number)|(One or more)|(Zero or one\.?)|(Zero or more\.?))$", RegexOptions.IgnoreCase);
                    while ((line = r.ReadLine()) != null)
                    {
                        if (line.StartsWith("Box Type: "))
                        {
                            string containerLine = r.ReadLine();
                            string mandatoryLine = r.ReadLine();
                            string quanityLine = r.ReadLine();

                            MatchCollection typeMatches = typeRegex.Matches(line);
                            MatchCollection containerMatches = containersRegex.Matches(containerLine);
                            Match mandatoryMatch = mandtoryRegex.Match(mandatoryLine);
                            Match quantityMatch = quantityRegex.Match(quanityLine);
                            if (typeMatches.Count == 0 || containerMatches.Count == 0 || !mandatoryMatch.Success || !quantityMatch.Success)
                            {
                                Console.WriteLine(line);
                                Console.WriteLine(containerLine);
                                Console.WriteLine(mandatoryLine);
                                Console.WriteLine(quanityLine);
                                Console.WriteLine();
                            }

                            Box box = context.CreateBox().Single();

                            foreach (Match typeMatch in typeMatches)
                            {
                                string type = typeMatch.Groups[1].Value;
                                BoxUuid uuid = new BoxUuid();
                                uuid.Uuid = Conversions.FormUuid(Conversions.GetType(type), Conversions.Iso12Bytes);
                                box.BoxUuids.Add(uuid);
                            }

                            foreach (Match m in containerMatches)
                            {
                                BoxDefinition definition = new BoxDefinition();
                                string container = null;
                                if (m.Value != "File")
                                    container = m.Groups[3].Value;

                                definition.Conatiner = container == null ? new Nullable<Guid>() : Conversions.FormUuid(Conversions.GetType(container), Conversions.Iso12Bytes);

                                definition.Mandatory = mandatoryMatch.Groups[1].Value.StartsWith("Yes", StringComparison.InvariantCultureIgnoreCase);
                                if (box.BoxUuids.Any(u => u.Uuid == Conversions.FormUuid(Conversions.GetType("dinf"), Conversions.Iso12Bytes)))
                                {
                                    if (container == "minf")
                                        definition.Mandatory = true;

                                    if (container == "meta")
                                        definition.Mandatory = false;
                                }

                                if (quantityMatch.Groups[1].Value.StartsWith("Exactly one", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    definition.QuantityMinimum = 1;
                                    definition.QuantityMaximum = 1;
                                }

                                if (quantityMatch.Groups[1].Value.StartsWith("Any number", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    definition.QuantityMinimum = null;
                                    definition.QuantityMaximum = null;
                                }

                                if (quantityMatch.Groups[1].Value.StartsWith("One or more", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    definition.QuantityMinimum = 1;
                                    definition.QuantityMaximum = null;
                                }

                                if (quantityMatch.Groups[1].Value.StartsWith("Zero or one", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    definition.QuantityMinimum = 0;
                                    definition.QuantityMaximum = 1;
                                }

                                if (quantityMatch.Groups[1].Value.StartsWith("Zero or more", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    definition.QuantityMinimum = 0;
                                    definition.QuantityMaximum = null;
                                }

                                if (box.BoxUuids.Any(u => u.Uuid == Conversions.FormUuid(Conversions.GetType("meta"), Conversions.Iso12Bytes)))
                                {
                                    if (container == "File" || container == "moov" || container == "trak")
                                    {
                                        definition.QuantityMinimum = 0;
                                        definition.QuantityMaximum = 1;
                                    }
                                    else if (container == "meco")
                                    {
                                        definition.QuantityMinimum = 1;
                                        definition.QuantityMaximum = null;
                                    }
                                }

                                box.BoxDefinitions.Add(definition);
                            }

                            context.SubmitChanges();
                        }
                    }
                }
            }

            Console.Read();
        }
    }
}
