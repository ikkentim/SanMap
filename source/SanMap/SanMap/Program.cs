using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SanMap
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Options
            string baseName = null;
            string inputPath = null;
            string outputPath = null;
            string outputExtension = "png";
            ImageFormat outputFormat;
            int minimumZoomLevel = 0;
            int maximumZoomLevel = 4;
            int targetSize = 512;
            bool showHelp = false;

            #region Args checking

            //Declare options.
            var optionSet = new OptionSet
            {
                {
                    "i|input=",
                    "the input file (Required)",
                    (value) => inputPath = value
                },
                {
                    "o|output=",
                    "the output path (Default: same path as input image)",
                    (value) => outputPath = value
                },
                {
                    "z|zoom|maximum-zoom=",
                    "the maximum zoom level (Default: 4)",
                    (int value) => maximumZoomLevel = value
                },
                {
                    "minimum-zoom=",
                    "the minimum zoom level (Default: 0)",
                    (int value) => minimumZoomLevel = value < 0 ? 0 : value
                },
                {
                    "s|size|target-size=",
                    "the resulting with/height (Default: 512)",
                    (int value) => targetSize = value < 8 ? 8 : value
                },
                {
                    "e|extension|output-extension=",
                    "the output extension (Default: png)",
                    (value) => outputExtension = value
                },
                {
                    "h|help", "show this message and exit",
                    value => showHelp = value != null
                },
            };

            //Try to parse the optionset, or print an error.
            try
            {
                optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("SanMap: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'SanMap --help' for more information.");
                return;
            }

            //Show help if asked for.
            if (showHelp)
            {
                ShowHelp(optionSet);
                return;
            }

            //Check if inputPath is given.
            if (inputPath == null)
            {
                Console.Write("SanMap: ");
                Console.WriteLine("Missing required argument input file.");
                Console.WriteLine("Try 'SanMap --help' for more information.");
                return;
            }

            //Check if inputPath exists.
            if (!File.Exists(inputPath))
            {
                Console.Write("SanMap: ");
                Console.WriteLine("Input file not found.");
                Console.WriteLine("Try 'SanMap --help' for more information.");
                return;
            }

            //Check if inputPath is an supported ImageFormat.
            if (!new[] {".bmp", ".png", ".jpg", ".gif"}.Contains(Path.GetExtension(inputPath)))
            {
                Console.Write("SanMap: ");
                Console.WriteLine("Input file is no known image format.");
                Console.WriteLine("Try 'SanMap --help' for more information.");
                return;
            }

            //Check if outputExtension is an supported ImageFormat.
            if (!new[] {"bmp", "png", "jpg", "gif"}.Contains(outputExtension))
            {
                Console.Write("SanMap: ");
                Console.WriteLine("Output extension is not known .");
                Console.WriteLine("Try 'SanMap --help' for more information.");
                return;
            }

            //If no outputpath is given, generate one.
            if (outputPath == null)
                outputPath = Path.GetDirectoryName(inputPath);

            //Calculate basename of output images.
            baseName = Path.GetFileNameWithoutExtension(inputPath);

            //Create directory of outputPath.
            Directory.CreateDirectory(outputPath);

            //Set outputFormat according to outputExtension.
            switch (outputExtension)
            {
                case "bmp":
                    outputFormat = ImageFormat.Bmp;
                    break;
                case "jpg":
                    outputFormat = ImageFormat.Jpeg;
                    break;
                case "gif":
                    outputFormat = ImageFormat.Gif;
                    break;
                default:
                    outputFormat = ImageFormat.Png;
                    break;
            }

            #endregion

            #region Show information

            //Show entered information
            Console.WriteLine("=======================");
            Console.WriteLine("SanMap Cutting Tool");
            Console.WriteLine("http://github.com/ikkentim/SanMap");
            Console.WriteLine("=======================");
            Console.WriteLine();
            Console.WriteLine("Input file: " + inputPath);
            Console.WriteLine("Output directory: " + outputPath);
            Console.WriteLine("Target size: " + targetSize + "x" + targetSize);
            Console.WriteLine("Zoom levels: " + minimumZoomLevel + "-" + maximumZoomLevel);
            Console.WriteLine();
            #endregion

            #region Cutting

            try
            {
                //Read the input image to memory.
                Console.WriteLine("Opening input file...");
                var baseImage = Image.FromFile(inputPath) as Bitmap;

                //Loop trough every zoom level.
                for (var zoom = minimumZoomLevel; zoom <= maximumZoomLevel; zoom++)
                {
                    //Show our progress
                    Console.Write("Processing zoom level " + zoom);

                    //Caclucate the number of tiles we're processing in this zoom level.
                    var tiles = 1 << zoom;

                    //Calculate the source-tilesize
                    var tileWidth = (float) baseImage.Width/tiles;
                    var tileHeight = (float) baseImage.Height/tiles;

                    //Keep track of our progress
                    var progress = 0;

                    //Loop trough every tile X/Y
                    for (var tileX = 0; tileX < tiles; tileX++)
                        for (var tileY = 0; tileY < tiles; tileY++)
                        {
                            //Create a new bitmap of the source-tilesize
                            var baseTile = new Bitmap((int) Math.Floor(tileWidth), (int) Math.Floor(tileHeight));
                            using (Graphics g = Graphics.FromImage(baseTile))
                            {
                                //Copy the image from the source
                                g.DrawImage(baseImage, new RectangleF(0, 0, tileWidth, tileHeight),
                                    new RectangleF(tileWidth*tileX, tileHeight*tileY, tileWidth, tileHeight),
                                    GraphicsUnit.Pixel);
                            }

                            //Resize the tile
                            var tile = ResizeImage(baseTile, new Size(targetSize, targetSize));

                            //Generate the output filename
                            var outputFile = string.Format("{0}.{1}.{2}.{3}.{4}", baseName, zoom, tileX, tileY,
                                outputExtension);

                            //Save the tile
                            tile.Save(Path.Combine(outputPath, outputFile), outputFormat);

                            //Dispose tile
                            baseTile.Dispose();
                            tile.Dispose();

                            //Update current progress
                            var currentProgress = ((tileX*tiles + tileY + 1)*5)/(tiles*tiles);
                            while (progress < currentProgress)
                            {
                                progress++;
                                Console.Write(".");
                            }
                        }

                    //End line of 'Processing zoom level X.....'
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                //Whoops!
                Console.WriteLine();
                Console.WriteLine("ERROR THROWN: ");
                Console.Write("\t");
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine("Done Processing!");
            }
            #endregion
        }

        static Bitmap ResizeImage(Image image, Size size)
        {
            Bitmap newImage = new Bitmap(size.Width, size.Height);
         
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    gr.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            //return the resulting bitmap
            return newImage;
        }

        static void ShowHelp(OptionSet p)
        {
            //Show usage, options and supported image formats
            Console.WriteLine("Usage: SanMap [OPTIONS]");
            Console.WriteLine("Cut images up in multiple part for use with Google Maps.");

            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);

            Console.WriteLine();
            Console.WriteLine("Supported image types: bmp, png, jpg, gif");
        }
    }
}
