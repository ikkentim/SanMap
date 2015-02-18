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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Projections;
using OsmSharp.Osm.Data.Memory;
using OsmSharp.Osm.Streams;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.UI.Map;
using OsmSharp.UI.Map.Layers;
using OsmSharp.UI.Map.Styles;
using OsmSharp.UI.Map.Styles.MapCSS;
using OsmSharp.UI.Renderer;
using OsmSharp.WinForms.UI.Renderer;

namespace TileCutter.Processors
{
    internal class OpenStreetMaps : IProcessor
    {
        #region Implementation of IProcessor

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public bool Validate(InstructionSet instructions)
        {
            return File.Exists(instructions.InputPath) &&
                   File.Exists(instructions.InputPath + ".mapcss") &&
                   Path.GetExtension(instructions.InputPath) == ".osm" &&
                   Directory.Exists(instructions.OutputDirectory) &&
                   ImageDefaults.IsValidSize(instructions.OutputSize);
        }

        public async Task<bool> StartProcessing(InstructionSet instructions)
        {
            if (!Validate(instructions)) return false;

            var inputPath = instructions.InputPath;
            var tempPath = Path.Combine(Path.GetDirectoryName(inputPath), Path.GetFileNameWithoutExtension(inputPath)) + ".temp";
            var resizeFactor = instructions.PreprocessorResizeFactor;
            var doResize = resizeFactor > 1;
            var origSize = instructions.MapSize;
            var targetSize = origSize*resizeFactor;
            var outputSize = instructions.OutputSize;
            var cssPath = instructions.InputPath + ".mapcss";
            string baseName = instructions.OutputName;
            ImageFormat outputFormat = instructions.OutputFormat;
            string outputExtension = outputFormat.GetFileExtension();

            var minZoom = instructions.MinimumZoom;
            var maxZoom = instructions.MaximumZoom;
            var outputDir = instructions.OutputDirectory;
            var skipExisting = instructions.SkipExisting;

            return await Task<bool>.Run(() =>
            {
                if (doResize)
                {
                    var doc = XDocument.Load(inputPath);

                    var nodes = doc.Root.Elements("node");

                    double minLat = double.PositiveInfinity;
                    double maxLat = double.NegativeInfinity;
                    double minLon = double.PositiveInfinity;
                    double maxLon = double.NegativeInfinity;

                    foreach (var node in nodes)
                    {
                        var lat = node.Attribute("lat");
                        var lon = node.Attribute("lon");

                        var newLat = double.Parse(lat.Value.Replace(".", ","))*resizeFactor;
                        var newLon = double.Parse(lon.Value.Replace(".", ","))*resizeFactor;

                        minLat = Math.Min(minLat, newLat);
                        maxLat = Math.Max(maxLat, newLat);
                        minLon = Math.Min(minLon, newLon);
                        maxLon = Math.Max(maxLon, newLon);

                        lat.Value = newLat.ToString("0.000000000000000000", CultureInfo.InvariantCulture);
                        lon.Value = newLon.ToString("0.000000000000000000", CultureInfo.InvariantCulture);
                    }

                    doc.Save(tempPath);

                    inputPath = tempPath;
                }

                var mapCssStream = new FileInfo(cssPath).OpenRead();
                var inputStream = new FileInfo(inputPath).OpenRead();

                MapCSSInterpreter mapcss;

                try
                {
                    mapcss = new MapCSSInterpreter(mapCssStream, new MapCSSDictionaryImageSource());
                }
                catch (Exception)
                {
                    return false;
                }

                var renderer = RenderingInstance.Build(new XmlOsmStreamSource(inputStream), mapcss);

                int processed = 0;
                for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                {
                    if (ProgressChanged != null)
                        ProgressChanged(this, new ProgressChangedEventArgs(processed, "Processing zoom " + zoom));

                    int tiles = 1 << zoom;
                    for (int x = 0; x < tiles; x++)
                        for (int y = 0; y < tiles; y++)
                        {

                            var outfile = string.Format(@"{3}\{5}.{2}.{0}.{1}{4}", x, y, zoom, outputDir, outputExtension, baseName);

                            if (skipExisting && File.Exists(outfile)) continue;

                            var outstr = renderer.Render(x, y, zoom, targetSize, outputSize);
                            outstr.Save(outfile, outputFormat);
                            outstr.Dispose();


                            if (ProgressChanged != null)
                                ProgressChanged(this,
                                    new ProgressChangedEventArgs(++processed, "Processing zoom " + zoom));
                        }
                }

                mapCssStream.Dispose();
                inputStream.Dispose();
                
                return true;
            });
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return "Open Street Maps";
        }

        #endregion

        private class RenderingInstance
        {
            private readonly Map _map;
            private readonly MapRenderer<Graphics> _renderer;

            public RenderingInstance()
            {
                _renderer = new MapRenderer<Graphics>(new GraphicsRenderer2D());
                _map = new Map(new WebMercator());
            }

            public Map Map
            {
                get { return _map; }
            }

            public Bitmap Render(int x, int y, int zoom, double width, int tileSize)
            {
                const double magicMaster = 9105.7453358554949742220244056229;

                float zoomRate = (float)(magicMaster * width);
                int tiles = 1 << zoom;
                double tileGeoWidth = width / tiles;

                double movex = tiles == 1 ? 0 : tileGeoWidth * (tiles / 2 - x) - tileGeoWidth / 2;
                double movey = tiles == 1 ? 0 : tileGeoWidth * (tiles / 2 - y) - tileGeoWidth / 2;

                var center = new GeoCoordinate(movey, -movex);

                double baseZoomFactor = 100f * ((double)tileSize / zoomRate);

                float zoomFactor = Convert.ToSingle(baseZoomFactor * tiles);

                Bitmap image = new Bitmap(tileSize, tileSize);
                Graphics target = Graphics.FromImage(image);
                target.SmoothingMode = SmoothingMode.HighQuality;
                target.PixelOffsetMode = PixelOffsetMode.HighQuality;
                target.CompositingQuality = CompositingQuality.HighQuality;
                target.InterpolationMode = InterpolationMode.HighQualityBicubic;

                target.FillRectangle(Brushes.Red, 0, 0, tileSize, tileSize);
                View2D visibleView = _renderer.Create(tileSize, tileSize, _map, zoomFactor, center, false, true);

                _map.ViewChanged(zoomFactor, center, visibleView, visibleView);
                _renderer.Render(target, _map, visibleView, visibleView, zoomFactor);

                target.Dispose();
                return image;
            }

            #region Static Instance Builders

            public static RenderingInstance Build(OsmStreamSource streamSource, StyleInterpreter interpreter)
            {
                var instance = new RenderingInstance();

                MemoryDataSource dataSource = MemoryDataSource.CreateFrom(streamSource);

                instance.Map.AddLayer(new LayerOsm(dataSource, interpreter, instance.Map.Projection));

                return instance;
            }

            public static RenderingInstance BuildForMapCSS(OsmStreamSource streamSource, string mapCSS)
            {
                var instance = new RenderingInstance();

                MemoryDataSource dataSource = MemoryDataSource.CreateFrom(streamSource);

                var interpreter = new MapCSSInterpreter(mapCSS);

                instance.Map.AddLayer(new LayerOsm(dataSource, interpreter, instance.Map.Projection));

                return instance;
            }

            public static RenderingInstance BuildForMapCSS(OsmStreamSource streamSource, Stream mapCSSFile)
            {
                var instance = new RenderingInstance();

                MemoryDataSource dataSource = MemoryDataSource.CreateFrom(streamSource);

                var interpreter = new MapCSSInterpreter(mapCSSFile, new MapCSSDictionaryImageSource());

                instance.Map.AddLayer(new LayerOsm(dataSource, interpreter, instance.Map.Projection));

                return instance;
            }

            #endregion
        }
    }
}