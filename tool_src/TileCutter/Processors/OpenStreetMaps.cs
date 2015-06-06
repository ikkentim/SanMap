// SanMap
// Copyright 2015 Tim Potze
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.UI.Map.Styles.MapCSS;

namespace TileCutter.Processors
{
    internal class OpenStreetMaps : IProcessor
    {
        #region Implementation of IProcessor

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public string Validate(InstructionSet instructions)
        {
            if (!File.Exists(instructions.InputPath))
                return "Input path does not exist.";

            if (Path.GetExtension(instructions.InputPath) != ".osm")
                return "Input file is not a *.osm file.";

            if (!File.Exists(instructions.InputPath + ".mapcss"))
                return "*.mapcss file not found.";

            if (!Directory.Exists(instructions.OutputDirectory))
                return "Output directory does not exist.";


            if (!ImageDefaults.IsValidSize(instructions.OutputSize))
                return "The output size must be 128, 256, 512, 1024, ...";

            return null;
        }

        public async Task<string> StartProcessing(InstructionSet instructions)
        {
            string validationResult = Validate(instructions);
            if (validationResult != null) return validationResult;

            string inputPath = instructions.InputPath;
            string tempPath =
                Path.Combine(Path.GetDirectoryName(inputPath), Path.GetFileNameWithoutExtension(inputPath)) + ".temp";
            int resizeFactor = instructions.PreprocessorResizeFactor;
            bool doResize = resizeFactor > 1;
            double origSize = instructions.MapSize;
            double targetSize = origSize*resizeFactor;
            int outputSize = instructions.OutputSize;
            string cssPath = instructions.InputPath + ".mapcss";
            string baseName = instructions.OutputName;
            ImageFormat outputFormat = instructions.OutputFormat;
            string outputExtension = outputFormat.GetFileExtension();

            int minZoom = instructions.MinimumZoom;
            int maxZoom = instructions.MaximumZoom;
            string outputDir = instructions.OutputDirectory;
            bool skipExisting = instructions.SkipExisting;

            return await Task<string>.Run(() =>
            {
                if (doResize)
                {
                    XDocument doc = XDocument.Load(inputPath);

                    IEnumerable<XElement> nodes = doc.Root.Elements("node");

                    int nodeCount = 0;
                    foreach (XElement node in nodes)
                    {
                        XAttribute lat = node.Attribute("lat");
                        XAttribute lon = node.Attribute("lon");

                        double newLat = double.Parse(lat.Value, CultureInfo.InvariantCulture) * resizeFactor;
                        double newLon = double.Parse(lon.Value, CultureInfo.InvariantCulture) * resizeFactor;

                        lat.Value = newLat.ToString("0.000000000000000000", CultureInfo.InvariantCulture);
                        lon.Value = newLon.ToString("0.000000000000000000", CultureInfo.InvariantCulture);

                        nodeCount++;
                    }

                    doc.Save(tempPath);
                    
                    inputPath = tempPath;
                }

                MapCSSInterpreter mapCss;
                RenderingInstance renderer;

                FileStream mapCssStream = new FileInfo(cssPath).OpenRead();
                try
                {
                    mapCss = new MapCSSInterpreter(mapCssStream, new MapCSSDictionaryImageSource());
                }
                catch (Exception e)
                {
                    return e.Message;
                }
                finally
                {
                    mapCssStream.Dispose();
                }

                FileStream inputStream = new FileInfo(inputPath).OpenRead();
                try
                {
                    renderer = RenderingInstance.Build(new XmlOsmStreamSource(inputStream), mapCss);
                }
                catch (Exception e)
                {
                    return e.Message;
                }
                finally
                {
                    inputStream.Dispose();
                }

                int processed = 0;
                for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                {
                    if (ProgressChanged != null)
                        ProgressChanged(this, new ProgressChangedEventArgs(processed, "Processing zoom " + zoom));

                    int tiles = 1 << zoom;
                    for (int x = 0; x < tiles; x++)
                        for (int y = 0; y < tiles; y++)
                        {
                            string outfile = string.Format(@"{3}\{5}.{2}.{0}.{1}{4}", x, y, zoom, outputDir,
                                outputExtension, baseName);

                            if (skipExisting && File.Exists(outfile)) continue;

                            Bitmap outstr = renderer.Render(x, y, zoom, targetSize, outputSize);
                            outstr.Save(outfile, outputFormat);
                            outstr.Dispose();

                            if (ProgressChanged != null)
                                ProgressChanged(this,
                                    new ProgressChangedEventArgs(++processed, "Processing zoom " + zoom));
                        }
                }

                inputStream.Dispose();

                return null;
            });
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return "Open Street Maps";
        }

        #endregion
    }
}