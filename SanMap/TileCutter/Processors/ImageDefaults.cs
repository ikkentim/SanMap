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
        public static bool Validate(InstructionSet instructions)
        {
            Size? dim = ImageHelper.GetDimensions(instructions.InputPath);
            return File.Exists(instructions.InputPath) &&
                   dim != null &&
                   dim.Value.Width == dim.Value.Height &&
                   IsValidSize(instructions.OutputSize);
        }

        private static bool IsValidSize(int size)
        {
            int s = 2;
            while (s < size) s *= 2;
            return size == s;
        }
    }
}