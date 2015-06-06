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

using System.Drawing.Imaging;

namespace TileCutter.Processors
{
    internal struct InstructionSet
    {
        public string InputPath { get; set; }
        public string OutputDirectory { get; set; }
        public string OutputName { get; set; }
        public int OutputSize { get; set; }
        public ImageFormat OutputFormat { get; set; }
        public int MinimumZoom { get; set; }
        public int MaximumZoom { get; set; }
        public bool SkipExisting { get; set; }
        public int PreprocessorResizeFactor { get; set; }
        public double MapSize { get; set; }
    }
}