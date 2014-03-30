using System;
using System.IO;
using System.Linq;

namespace SanMap
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string inputPath =null;
            string outputPath = null;
            int minimumZoomLevel = 0;
            int maximumZoomLevel = 2;
            bool showHelp = false;

            #region Args checking
            var p = new OptionSet
            {
                {
                    "i|input=",
                    "the input file (Required)",
                    (value) => inputPath = value
                },
                {
                    "o|output=",
                    "the output path",
                    (value) => outputPath = value
                },
                {
                    "z|zoom|maximum-zoom=",
                    "the maximum zoom level",
                    (int value) => maximumZoomLevel = value
                },
                {
                    "minimum-zoom=",
                    "the minimum zoom level",
                    (int value) => minimumZoomLevel = value < 0 ? 0 : value
                },
                {
                    "h|help", "show this message and exit",
                    value => showHelp = value != null
                },
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("SanMap: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'SanMap --help' for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(p);
                return;
            }

            if (inputPath == null)
            {
                Console.Write("SanMap: ");
                Console.WriteLine("Missing required argument input file.");
                Console.WriteLine("Try 'SanMap --help' for more information.");
                return;
            }

            if (!File.Exists(inputPath))
            {
                Console.Write("SanMap: ");
                Console.WriteLine("Input file not found.");
                Console.WriteLine("Try 'SanMap --help' for more information.");
                return;
            }

            if(!new []{".bmp", ".png", ".jpg", ".gif"}.Contains(Path.GetExtension(inputPath)))
            {
                Console.Write("SanMap: ");
                Console.WriteLine("Input file is no known image format.");
                Console.WriteLine("Try 'SanMap --help' for more information.");
                return;
            }

            if (outputPath == null)
                outputPath = Path.GetDirectoryName(inputPath);

            Directory.CreateDirectory(outputPath);

            #endregion

            //Info
            Console.WriteLine("SanMap cutting tool...");
            Console.WriteLine("Input file: " + inputPath);
            Console.WriteLine("Output directory: " + outputPath);
            Console.WriteLine("Zoom levels: " + minimumZoomLevel + "-" + maximumZoomLevel);
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: SanMap [OPTIONS]");
            Console.WriteLine("Cut images up in multiple part for use with Google Maps.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
