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
using System.Threading.Tasks;
using System.Windows.Forms;
using TileCutter.Processors;

namespace TileCutter
{
    public partial class MainForm : Form
    {
        private readonly IProcessor[] _processors =
        {
            new GDI(),
            new DLLImageMagick(), 
            new OpenStreetMaps()
        };

        public MainForm()
        {
            InitializeComponent();

            // Add image processors
            processorComboBox.Items.AddRange(_processors);

            //Set default options
            processorComboBox.SelectedIndex = 0;
            outputTypeComboBox.SelectedIndex = 0;

            //Set default directories
            folderBrowserDialog.SelectedPath = Directory.GetCurrentDirectory();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
        }

        private async void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("{0}/{1}", e.NewProgress, toolStripProgressBar.Maximum);
                if (statusStrip1.InvokeRequired)
                    statusStrip1.Invoke((MethodInvoker) delegate
                    {
                        if (toolStripStatusLabel.Text == "Done!") return;
                        toolStripProgressBar.Value = Math.Min(e.NewProgress, toolStripProgressBar.Maximum);
                        toolStripStatusLabel.Text = e.ProgressStatus;
                    });
                else
                {
                    if (toolStripStatusLabel.Text == "Done!") return;

                    toolStripProgressBar.Value = Math.Min(e.NewProgress, toolStripProgressBar.Maximum);
                    toolStripStatusLabel.Text = e.ProgressStatus;
                }
            });
        }

        #region Calculators

        private void CalculateMaxZoom()
        {
            Size? size = ImageHelper.GetDimensions(inputPathTextBox.Text);
            if (size == null)
            {
                minZoomNumericUpDown.Maximum = 100;
                maxZoomNumericUpDown.Maximum = 100;

                return;
            }

            int max = CalculateMaxZoom(size.Value.Width, (int) targetSizeNumericUpDown.Value);
            minZoomNumericUpDown.Maximum = max;
            maxZoomNumericUpDown.Maximum = max;
        }

        private int CalculateMaxZoom(int size, int tileSize)
        {
            int zoom = 0;
            while ((size /= 2) > tileSize) zoom++;
            return zoom;
        }

        private IProcessor GetProcessor()
        {
            return processorComboBox.SelectedItem as IProcessor;
        }

        private ImageFormat GetImageFormat()
        {
            switch (outputTypeComboBox.SelectedIndex)
            {
                default:
                    return ImageFormat.Png;
                case 1:
                    return ImageFormat.Bmp;
                case 2:
                    return ImageFormat.Jpeg;
                case 3:
                    return ImageFormat.Gif;
            }
        }

        #endregion

        #region Component handlers

        private async void startButton_Click(object sender, EventArgs e)
        {
            var instr = new InstructionSet
            {
                InputPath = inputPathTextBox.Text,
                OutputDirectory = outputPathTextBox.Text,
                OutputSize = (int) targetSizeNumericUpDown.Value,
                OutputFormat = GetImageFormat(),
                MaximumZoom = (int) maxZoomNumericUpDown.Value,
                MinimumZoom = (int) minZoomNumericUpDown.Value,
                OutputName = Path.GetFileNameWithoutExtension(inputPathTextBox.Text),
                SkipExisting = skipExistingcheckBox.Checked,
                MapSize = Convert.ToDouble(mapSizeNumericUpDown.Value),
                PreprocessorResizeFactor = Convert.ToInt32(resizeFactorNumericUpDown.Value)
            };

            var processor = GetProcessor();
            string validationResult = processor.Validate(instr);
            if (validationResult != null)
            {
                MessageBox.Show(this, validationResult, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            optionsGroupBox.Enabled = false;
            toolStripProgressBar.Value = 0;
            toolStripStatusLabel.Text = "Starting...";
            processor.ProgressChanged += OnProgressChanged;

            toolStripProgressBar.Maximum = ImageDefaults.TotalTiles(instr.MinimumZoom, instr.MaximumZoom);

            string result = await processor.StartProcessing(instr);
            if (result != null)
            {
                MessageBox.Show(this, result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            processor.ProgressChanged -= OnProgressChanged;

            optionsGroupBox.Enabled = true;
            toolStripProgressBar.Value = 0;
            toolStripStatusLabel.Text = "Done!";
        }

        private void inputPathBrowseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;

            inputPathTextBox.Text = openFileDialog.FileName;

            Size? size = ImageHelper.GetDimensions(inputPathTextBox.Text);

            if (size == null)
            {
                CalculateMaxZoom();
                return;
            }

            if (size.Value.Width != size.Value.Height)
            {
                inputPathTextBox.Text = null;
                MessageBox.Show(this, "Image must be square!", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            CalculateMaxZoom();
        }

        private void outputPathBrowseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                outputPathTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void minZoomNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            maxZoomNumericUpDown.Minimum = minZoomNumericUpDown.Value;
        }

        private void targetSizeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            CalculateMaxZoom();
        }

        private void processorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateMaxZoom();
        }

        #endregion
    }
}