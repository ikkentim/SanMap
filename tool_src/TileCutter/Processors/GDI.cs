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
using System.IO;
using System.Threading.Tasks;

namespace TileCutter.Processors
{
    internal class GDI : IProcessor
    {
        #region Implementation of IProcessor

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public string Validate(InstructionSet instructions)
        {
            return ImageDefaults.Validate(instructions);
        }

        public async Task<string> StartProcessing(InstructionSet instructions)
        {
            var validationResult = Validate(instructions);
            if (validationResult != null) return validationResult;

            var dims = ImageHelper.GetDimensions(instructions.InputPath);
            if (dims == null) return null;

            string inputPath = instructions.InputPath;
            int minZoom = instructions.MinimumZoom;
            int maxZoom = instructions.MaximumZoom;
            var inputSize = (Size) dims;
            string outputDirectory = instructions.OutputDirectory;
            bool skipExisting = instructions.SkipExisting;
            string baseName = instructions.OutputName;
            ImageFormat outputFormat = instructions.OutputFormat;
            string outputExtension = outputFormat.GetFileExtension();
            int outputSize = instructions.OutputSize;

            return await Task<string>.Run(() =>
            {
                try
                {
                    var baseImage = Image.FromFile(inputPath) as Bitmap;

                    int processed = 0;
                    for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                    {
                        if (ProgressChanged != null)
                            ProgressChanged(this, new ProgressChangedEventArgs(processed, "Processing zoom " + zoom));

                        int tiles = 1 << zoom;

                        int tileSize = inputSize.Width/tiles;

                        for (int tileX = 0; tileX < tiles; tileX++)
                            for (int tileY = 0; tileY < tiles; tileY++)
                            {
                                string outputFile = Path.Combine(outputDirectory,
                                    string.Format("{0}.{1}.{2}.{3}{4}", baseName, zoom, tileX, tileY, outputExtension));

                                if (skipExisting && File.Exists(outputFile))
                                    continue;

                                var baseTile = new Bitmap(tileSize, tileSize);
                                using (Graphics g = Graphics.FromImage(baseTile))
                                {
                                    g.DrawImage(baseImage, new RectangleF(0, 0, tileSize, tileSize),
                                        new RectangleF(tileSize*tileX, tileSize*tileY, tileSize, tileSize),
                                        GraphicsUnit.Pixel);
                                }

                                Bitmap tile = ResizeImage(baseTile, new Size(outputSize, outputSize));

                                tile.Save(outputFile, outputFormat);

                                baseTile.Dispose();
                                tile.Dispose();

                                if (ProgressChanged != null)
                                    ProgressChanged(this,
                                        new ProgressChangedEventArgs(++processed, "Processing zoom " + zoom));
                            }
                    }

                    return null;
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            });
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return "GDI";
        }

        #endregion

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

            return newImage;
        }
    }
}