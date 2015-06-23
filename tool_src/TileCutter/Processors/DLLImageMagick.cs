using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;

namespace TileCutter.Processors
{
    internal class DLLImageMagick : IProcessor
    {
        #region Implementation of IProcessor

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public string Validate(InstructionSet instructions)
        {
            return ImageDefaults.Validate(instructions);
        }

        public async Task<string> StartProcessing(InstructionSet instructions)
        {
            string validationResult = Validate(instructions);
            if (validationResult != null) return validationResult;

            Size? dims = ImageHelper.GetDimensions(instructions.InputPath);
            if (dims == null) return null;

            string inputPath = instructions.InputPath;
            int minZoom = instructions.MinimumZoom;
            int maxZoom = instructions.MaximumZoom;
            string outputDirectory = instructions.OutputDirectory;
            string baseName = instructions.OutputName;
            ImageFormat outputFormat = instructions.OutputFormat;
            string outputExtension = outputFormat.GetFileExtension();
            int outputSize = instructions.OutputSize;

            return await Task<string>.Run(() =>
            {
                try
                {

                    int processed = 0;
                    for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                    {
                        if (ProgressChanged != null)
                            ProgressChanged(this, new ProgressChangedEventArgs(processed, "Processing zoom " + zoom));

                        int tiles = 1 << zoom;


                        var input = new MagickImage(inputPath);

                        input.Resize(outputSize * tiles, outputSize * tiles);

                        var x = -1;
                        var y = 0;
                        foreach (var tile in input.CropToTiles(outputSize, outputSize))
                        {
                            x++;
                            y += x/tiles;
                            x %= tiles;

                            var outPath = Path.Combine(outputDirectory,
                                string.Format("{0}.{1}.{2}.{3}{4}", baseName, zoom, x, y, outputExtension));

                            if(!instructions.SkipExisting || !File.Exists(outPath))
                            tile.Write(outPath);

                            if (ProgressChanged != null)
                                ProgressChanged(this,
                                    new ProgressChangedEventArgs(++processed, "Processing zoom " + zoom));
                        }


//                        RunMagick(string.Format(
//                            "\"{6}\" -resize {0}x{0} -crop {1}x{1} -set filename:tile \"%[fx:page.x/{1}].%[fx:page.y/{1}]\" " +
//                            "+repage +adjoin \"{2}/{3}.{4}.%[filename:tile]{5}\"", outputSize * tiles, outputSize,
//                            outputDirectory, baseName, zoom, outputExtension, inputPath));

                    }
                }
                catch (Exception e)
                {
                    return e.Message;
                }

                return null;
            });
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return "ImageMagick";
        }

        #endregion
    }
}
