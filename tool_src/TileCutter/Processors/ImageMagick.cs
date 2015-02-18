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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TileCutter.Processors
{
    internal class ImageMagick : IProcessor
    {
        private readonly string _directory;

        public ImageMagick(string directory)
        {
            _directory = directory;
        }

        #region Imagemagick

        private void RunMagick(string args)
        {
            string exePath = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();

            Process magickProcess = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(exePath, _directory, "convert.exe"),
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.Combine(exePath, _directory)
            });

            magickProcess.WaitForExit();
        }

        #endregion

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

                        RunMagick(string.Format(
                            "\"{6}\" -resize {0}x{0} -crop {1}x{1} -set filename:tile \"%[fx:page.x/{1}].%[fx:page.y/{1}]\" " +
                            "+repage +adjoin \"{2}/{3}.{4}.%[filename:tile]{5}\"", outputSize*tiles, outputSize,
                            outputDirectory, baseName, zoom, outputExtension, inputPath));

                        if (ProgressChanged != null)
                            ProgressChanged(this,
                                new ProgressChangedEventArgs(processed += tiles, "Processing zoom " + zoom));
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
    }
}