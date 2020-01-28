using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ShortcutProjectFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("ShortcutProjectFixer.exe \"<mlt-file>\"");
                    Console.WriteLine("Press <Enter> to exit.");
                    Console.ReadKey();
                    return;
                }

                var fileName = args[0];

                int fixedCount = 0;

                var doc = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);

                if (doc?.Root?.Name != "mlt")
                {
                    Console.WriteLine("Unknown file format.");
                    return;
                }

                var strFrameRate = doc.Root.Element("profile")?.Attribute("frame_rate_num")?.Value;

                if (!int.TryParse(strFrameRate, out var frameRate))
                {
                    Console.WriteLine("frame_rate_num attribute is not found.");
                    return;
                }

                var producers = doc.Root.Elements("producer");
                foreach (var producer in producers)
                {
                    var producerId = producer.Attribute("id")?.Value;

                    var prop = producer.Elements("property")
                        .FirstOrDefault(x => x.Attribute("name")?.Value == "length");

                    if (prop == null)
                    {
                        Console.WriteLine($"Producer '{producerId}': property <length> is not found.");
                        continue;
                    }

                    var strLength = prop?.Value ?? "";
                    if (strLength.Contains(":"))
                    {
                        Console.WriteLine($"Producer '{producerId}': <length> = {strLength} ");
                        continue;
                    }

                    if (!double.TryParse(strLength, out var length))
                    {
                        Console.WriteLine($"Producer '{producerId}': <length> = {strLength} can't be parced.");
                        continue;
                    }

                    var time = TimeSpan.FromMilliseconds(1000.0 * length / frameRate);
                    var strTime = time.ToString(@"hh\:mm\:ss\.fff");
                    prop.Value = strTime;
                    Console.WriteLine($"Producer '{producerId}': set <length> to {strTime}.");
                    fixedCount++;
                }

                if (fixedCount > 0)
                {
                    var ext = Path.GetExtension(fileName);

                    var newName = fileName.Replace(ext, " [fixed]" + ext);
                    Console.WriteLine($"Saving file '{newName}'");
                    doc.Save(newName);
                    Console.WriteLine($"Success");
                }
                else
                {
                    Console.WriteLine($"No changes were done.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
