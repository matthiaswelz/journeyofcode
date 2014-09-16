using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using journeyofcode.Images.OnenoteOCR;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var imagePath = args.FirstOrDefault();
            if (String.IsNullOrWhiteSpace(imagePath) || !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: {0} [Path to image file]", Path.GetFileName(Assembly.GetAssembly(typeof(Program)).CodeBase));
                return;
            }

            Console.WriteLine("Running OCR for file " + imagePath);
            try
            {
                using (var ocrEngine = new OnenoteOcrEngine())
                using (var image = Image.FromFile(imagePath))
                {
                    var text = ocrEngine.Recognize(image);
                    if (text == null)
                        Console.WriteLine("nothing recognized");
                    else
                        Console.WriteLine("Recognized: " + text);
                }
            }
            catch (OcrException ex)
            {
                Console.WriteLine("OcrException:\n" + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Exception:\n" + ex);
            }

            Console.ReadLine();
        }
    }
}
