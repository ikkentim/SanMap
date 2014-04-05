// SanMap
// Copyright (C) 2014 Tim Potze
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            string baseName;
            string inputPath = null;
            string outputPath = null;
            string extension = "png";
            const int maxArgsLength = 6000;
            ImageFormat format;
            int minZoom = 0;
            int maxZoom = 4;
            int targetSize = 512;
            bool useMagick = false;
            bool showHelp = false;
            bool debug = false;
            bool skipExisting = false;

            #region Args checking

            //Declare options.
            var optionSet = new OptionSet
            {
                {
                    "i|input=",
                    "the input file (Required)",
                    value => inputPath = value
                },
                {
                    "o|output=",
                    "the output path (Default: same path as input image)",
                    value => outputPath = value
                },
                {
                    "z|zoom|maximum-zoom=",
                    "the maximum zoom level (Default: 4)",
                    (int value) => maxZoom = value
                },
                {
                    "n|minimum-zoom=",
                    "the minimum zoom level (Default: 0)",
                    (int value) => minZoom = value < 0 ? 0 : value
                },
                {
                    "s|size|target-size=",
                    "the resulting with/height (Default: 512)",
                    (int value) => targetSize = value < 128 ? 128 : value
                },
                {
                    "e|extension|output-extension=",
                    "the output extension (Default: png)",
                    value => extension = value
                },
                {
                    "m|magick", "use ImageMagick to process the images",
                    value => useMagick = value != null
                },
                {
                    "d|debug", "show debug information",
                    value => debug = value != null
                },
                {
                    "k|skip", "skip existing tiles",
                    value => skipExisting = value != null
                },
                {
                    "h|help", "show this message and exit",
                    value => showHelp = value != null
                }
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
            if (!new[] {"bmp", "png", "jpg", "gif"}.Contains(extension))
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
            switch (extension)
            {
                case "bmp":
                    format = ImageFormat.Bmp;
                    break;
                case "jpg":
                    format = ImageFormat.Jpeg;
                    break;
                case "gif":
                    format = ImageFormat.Gif;
                    break;
                default:
                    format = ImageFormat.Png;
                    break;
            }

            Size size = ImageHelper.GetDimensions(inputPath);

            #endregion

            #region Show information

            //Show entered information
            Console.WriteLine("=======================");
            Console.WriteLine("SanMap Cutting Tool");
            Console.WriteLine("http://github.com/ikkentim/SanMap");
            Console.WriteLine("=======================");
            Console.WriteLine();
            Console.WriteLine("Input file: {0}", inputPath);
            Console.WriteLine("Input size: {0}x{1}", size.Width, size.Height);
            Console.WriteLine("Output directory: {0}", outputPath);
            Console.WriteLine("Target size: {0}x{0}", targetSize);
            Console.WriteLine("Zoom levels: {0}-{1}", minZoom, maxZoom);
            Console.WriteLine();

            #endregion

            #region Cutting

            if (useMagick)
            {
                #region Magick

                //Queue of magick commands
                List<string> magickQueue = new List<string>();
                var startArgs = "\"" + inputPath + "\" ";

                //Loop trough every zoom level.
                for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                {
                    //Show our progress
                    Console.Write("Processing zoom level " + zoom + (debug ? "\n" : ""));

                    //Caclucate the number of tiles we're processing in this zoom level.
                    int tiles = 1 << zoom;

                    //Calculate the source-tilesize
                    double tileWidth = size.Width/tiles;
                    double tileHeight = size.Height/tiles;

                    //Keep track of our progress
                    int progress = 0;

                    //Loop trough every tile X/Y
                    for (int tileX = 0; tileX < tiles; tileX++)
                        for (int tileY = 0; tileY < tiles; tileY++)
                        {
                            //Generate the output filename
                            string outputFile = Path.Combine(outputPath,
                                string.Format("{0}.{1}.{2}.{3}.{4}", baseName, zoom, tileX, tileY, extension));

                            //Skip existing
                            if (skipExisting && File.Exists(outputFile))
                                continue;

                            //Don't mind cutting if on zoom level 0
                            string magickArgs;
                            if (zoom == 0)
                                magickArgs = string.Format("( +clone -resize {0}x{0}! -write \"{1}\" +delete )",
                                    targetSize, outputFile);
                            else
                                magickArgs =
                                    string.Format(
                                        "( +clone -crop {0}x{1}+{2}+{3} +repage -resize {4}x{4}! -write \"{5}\" +delete )",
                                        Math.Floor(tileWidth), Math.Floor(tileHeight),
                                        Math.Floor(tileWidth*tileX), Math.Floor(tileHeight*tileY),
                                        targetSize,
                                        outputFile);

                            //Check if the queue isn't full, if it is, process it
                            if (magickQueue.Sum(s => s.Length + 1) + magickArgs.Length + startArgs.Length >
                                maxArgsLength)
                            {
                                RunMagick(debug, startArgs, magickQueue);
                                magickQueue.Clear();
                            }

                            //Add command to queue
                            magickQueue.Add(magickArgs);

                            //Update current progress
                            int currentProgress = ((tileX*tiles + tileY + 1)*(maxZoom*2))/(tiles*tiles);
                            while (progress < currentProgress && !debug)
                            {
                                progress++;
                                Console.Write(".");
                            }
                        }

                    //End line of 'Processing zoom level X.....'
                    Console.WriteLine();

                    //Process last commands of the zoom level
                    if (magickQueue.Count <= 0) continue;
                    RunMagick(debug, startArgs, magickQueue);
                    magickQueue.Clear();
                }

                #endregion

            }
            else
            {
                #region GDI

                try
                {
                    //Read the input image to memory.
                    Console.WriteLine("Opening input file...");
                    var baseImage = Image.FromFile(inputPath) as Bitmap;

                    //Loop trough every zoom level.
                    for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                    {
                        //Show our progress
                        Console.Write("Processing zoom level " + zoom);

                        //Caclucate the number of tiles we're processing in this zoom level.
                        int tiles = 1 << zoom;

                        //Calculate the source-tilesize
                        float tileWidth = size.Width/tiles;
                        float tileHeight = size.Height/tiles;

                        //Keep track of our progress
                        int progress = 0;

                        //Loop trough every tile X/Y
                        for (int tileX = 0; tileX < tiles; tileX++)
                            for (int tileY = 0; tileY < tiles; tileY++)
                            {
                                //Generate the output filename
                                string outputFile = Path.Combine(outputPath,
                                    string.Format("{0}.{1}.{2}.{3}.{4}", baseName, zoom, tileX, tileY, extension));

                                //Skip existing
                                if (skipExisting && File.Exists(outputFile))
                                    continue;

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
                                Bitmap tile = ResizeImage(baseTile, new Size(targetSize, targetSize));

                                //Save the tile
                                tile.Save(outputFile, format);

                                //Dispose tile
                                baseTile.Dispose();
                                tile.Dispose();

                                //Update current progress
                                int currentProgress = ((tileX*tiles + tileY + 1)*(maxZoom*2))/(tiles*tiles);
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
                    Console.WriteLine("Conversion FAILED!");
                    Console.WriteLine(e.Message);
                    throw;
                }

                #endregion
            }

            Console.WriteLine("Processing images completed.");

            #endregion
        }

        private static void RunMagick(bool debug, string start, IEnumerable<string> batch)
        {
            RunMagick(debug, start + string.Join(" ", batch));
        }

        private static void RunMagick(bool debug, string args)
        {
            //Start the magick process
            if (debug)
                Console.WriteLine("convert {0}", args);

            Process magickProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "magick/convert.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            magickProcess.WaitForExit();    
        }

        private static Bitmap ResizeImage(Image image, Size size)
        {
            var newImage = new Bitmap(size.Width, size.Height);

            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    gr.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height), 0, 0, image.Width, image.Height,
                        GraphicsUnit.Pixel, wrapMode);
                }
            }

            //return the resulting bitmap
            return newImage;
        }

        private static void ShowHelp(OptionSet p)
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