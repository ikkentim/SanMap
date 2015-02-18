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

using System.Drawing;
using System.IO;

namespace TileCutter.Processors
{
    internal static class ImageDefaults
    {
        public static string Validate(InstructionSet instructions)
        {
            if (!File.Exists(instructions.InputPath))
                return "Input path does not exist.";

            if (!Directory.Exists(instructions.OutputDirectory))
                return "Output directory does not exist.";

            Size? dim = ImageHelper.GetDimensions(instructions.InputPath);
            if (dim == null)
                return "Invalid image format.";

            if (dim.Value.Width != dim.Value.Height)
                return "Input image must be square.";

            if (!IsValidSize(instructions.OutputSize))
                return "The output size must be 128, 256, 512, 1024, ...";

            return null;
        }

        public static bool IsValidSize(int size)
        {
            int s = 2;
            while (s < size) s *= 2;
            return size == s;
        }

        public static int TotalTiles(int minZoom, int maxZoom)
        {
            int result = 0;

            for (int z = minZoom; z <= maxZoom; z++)
                result += (1 << z);

            return result;
        }
    }
}