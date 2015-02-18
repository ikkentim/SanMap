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
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Projections;
using OsmSharp.Osm.Data.Memory;
using OsmSharp.Osm.Streams;
using OsmSharp.UI.Map;
using OsmSharp.UI.Map.Layers;
using OsmSharp.UI.Map.Styles;
using OsmSharp.UI.Renderer;
using OsmSharp.WinForms.UI.Renderer;

namespace TileCutter.Processors
{
    internal class RenderingInstance
    {
        public RenderingInstance()
        {
            Renderer = new MapRenderer<Graphics>(new GraphicsRenderer2D());
            Map = new Map(new WebMercator());
        }

        private Map Map { get; set; }
        private MapRenderer<Graphics> Renderer { get; set; }

        public Bitmap Render(int x, int y, int zoom, double width, int tileSize)
        {
            const double magicMaster = 9105.7453358554949742220244056229;

            var zoomRate = (float) (magicMaster*width);
            int tiles = 1 << zoom;
            double tileGeoWidth = width/tiles;

            double movex = tiles == 1 ? 0 : tileGeoWidth*(tiles/2 - x) - tileGeoWidth/2;
            double movey = tiles == 1 ? 0 : tileGeoWidth*(tiles/2 - y) - tileGeoWidth/2;

            var center = new GeoCoordinate(movey, -movex);

            double baseZoomFactor = 100f*((double) tileSize/zoomRate);

            float zoomFactor = Convert.ToSingle(baseZoomFactor*tiles);

            var image = new Bitmap(tileSize, tileSize);
            Graphics target = Graphics.FromImage(image);
            target.SmoothingMode = SmoothingMode.HighQuality;
            target.PixelOffsetMode = PixelOffsetMode.HighQuality;
            target.CompositingQuality = CompositingQuality.HighQuality;
            target.InterpolationMode = InterpolationMode.HighQualityBicubic;

            target.FillRectangle(Brushes.Red, 0, 0, tileSize, tileSize);
            View2D visibleView = Renderer.Create(tileSize, tileSize, Map, zoomFactor, center, false, true);

            Map.ViewChanged(zoomFactor, center, visibleView, visibleView);
            Renderer.Render(target, Map, visibleView, visibleView, zoomFactor);

            target.Dispose();
            return image;
        }

        public static RenderingInstance Build(OsmStreamSource streamSource, StyleInterpreter interpreter)
        {
            var instance = new RenderingInstance();

            MemoryDataSource dataSource = MemoryDataSource.CreateFrom(streamSource);

            instance.Map.AddLayer(new LayerOsm(dataSource, interpreter, instance.Map.Projection));

            return instance;
        }
    }
}