using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TileCutter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            outputTypeComboBox.SelectedIndex = 0;

            minZoomNumericUpDown.ValueChanged +=
                (sender, args) => maxZoomNumericUpDown.Minimum = minZoomNumericUpDown.Value;

            inputPathBrowseButton.Click += (sender, args) =>
            {
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                    inputPathTextBox.Text = openFileDialog.FileName;
            };

            outputPathBrowseButton.Click += (sender, args) =>
            {
                if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                    outputPathTextBox.Text = folderBrowserDialog.SelectedPath;
            };

            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.RestoreDirectory = true;

            folderBrowserDialog.SelectedPath = Directory.GetCurrentDirectory();
        }

        public bool ValidityChecks()
        {
            //Input file
            if (!File.Exists(inputPathTextBox.Text))
            {
                MessageBox.Show("Input file does not exist.", "SanMap", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!new[] { ".bmp", ".png", ".jpg", ".gif" }.Contains(Path.GetExtension(inputPathTextBox.Text)))
            {
                MessageBox.Show("Unknown filetype.", "SanMap", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;                
            }

            //Output path
            if (!Directory.Exists(outputPathTextBox.Text))
            {
                MessageBox.Show("Output path does not exist.", "SanMap", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;             
            }

            //Success
            return true;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            //Check input
            if (!ValidityChecks())
                return;

            optionsGroupBox.Enabled = false;

            toolStripStatusLabel.Text = "Setting up";


            var inputPath = inputPathTextBox.Text;
            var outputPath = outputPathTextBox.Text;

            bool useMagick = magickCheckBox.Checked;
            bool skipExisting = skipExistingcheckBox.Checked;

            var baseName = Path.GetFileNameWithoutExtension(inputPath);

            var outputSize = (int) targetSizeNumericUpDown.Value;
            var minZoom = (int) minZoomNumericUpDown.Value;
            var maxZoom = (int) maxZoomNumericUpDown.Value;

            string outputExtension;
            ImageFormat outputFormat;
            switch (outputTypeComboBox.SelectedIndex)
            {
                default:
                    outputExtension = ".png";
                    outputFormat = ImageFormat.Png;
                    break;
                case 1:
                    outputExtension = ".bmp";
                    outputFormat = ImageFormat.Bmp;
                    break;
                case 2:
                    outputExtension = ".jpg";
                    outputFormat = ImageFormat.Jpeg;
                    break;
                case 3:
                    outputExtension = ".gif";
                    outputFormat = ImageFormat.Gif;
                    break;
            }

            //Process image
            Process(inputPath, outputPath, outputSize, minZoom, maxZoom, useMagick, skipExisting, baseName, outputExtension, outputFormat);
        }

        public void UpdateStatus(int perc, string stat)
        {
            Task.Run(() =>
            {
                if (statusStrip1.InvokeRequired)
                    statusStrip1.Invoke((MethodInvoker) delegate
                    {
                        toolStripProgressBar.Value = perc > toolStripProgressBar.Maximum ? toolStripProgressBar.Maximum : perc;
                        toolStripStatusLabel.Text = stat;
                    });
                else
                {
                    toolStripProgressBar.Value = perc > toolStripProgressBar.Maximum ? toolStripProgressBar.Maximum : perc;
                    toolStripStatusLabel.Text = stat;
                }
            });
        }

        public void Process(string inputPath, string outputPath, int outputSize, int minZoom, int maxZoom, bool useMagick, bool skipExisting, string baseName, string outputExtension, ImageFormat outputFormat)
        {
            Task.Run(() =>
            {
                Size inputSize = ImageHelper.GetDimensions(inputPath);

                UpdateStatus(0, "Working");

                bool success = useMagick
                    ? ProcessMagick(inputPath, outputPath, outputSize, minZoom, maxZoom, skipExisting, inputSize,
                        baseName, outputExtension, outputFormat)
                    : ProcessGDI(inputPath, outputPath, outputSize, minZoom, maxZoom, skipExisting, inputSize, baseName,
                        outputExtension, outputFormat);

                //Success
                UpdateStatus(0, "Ready");
                optionsGroupBox.Invoke((MethodInvoker) delegate
                {
                    optionsGroupBox.Enabled = true;
                    if (success)
                        MessageBox.Show("Successfully processed images.", "SanMap", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    else
                    {
                        MessageBox.Show("Failed to process images.", "SanMap", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                });

            });

        }

        #region Imagemagick
        public bool ProcessMagick(string inputPath, string outputPath, int outputSize, int minZoom, int maxZoom,
            bool skipExisting, Size inputSize, string baseName, string outputExtension, ImageFormat outputFormat)
        {
                const int maxArgsLength = 6000;

                //Queue of magick commands
                List<string> magickQueue = new List<string>();
                var startArgs = "\"" + inputPath + "\" ";

                //Loop trough every zoom level.
            for (int zoom = minZoom; zoom <= maxZoom; zoom++)
            {
                //Show our progress
                UpdateStatus(0, "Processing zoom " + zoom);

                //Caclucate the number of tiles we're processing in this zoom level.
                int tiles = 1 << zoom;

                //Calculate the source-tilesize
                double tileWidth = inputSize.Width/tiles;
                double tileHeight = inputSize.Height / tiles;

                //Keep track of our progress
                int progress = 0;

                //Loop trough every tile X/Y
                for (int tileX = 0; tileX < tiles; tileX++)
                    for (int tileY = 0; tileY < tiles; tileY++)
                    {
                        //Generate the output filename
                        string outputFile = Path.Combine(outputPath,
                            string.Format("{0}.{1}.{2}.{3}{4}", baseName, zoom, tileX, tileY, outputExtension));

                        //Skip existing
                        if (skipExisting && File.Exists(outputFile))
                            continue;

                        //Don't mind cutting if on zoom level 0
                        string magickArgs;
                        if (zoom == 0)
                            magickArgs = string.Format("( +clone -resize {0}x{0}! -write \"{1}\" +delete )",
                                outputSize, outputFile);
                        else
                            magickArgs =
                                string.Format(
                                    "( +clone -crop {0}x{1}+{2}+{3} +repage -resize {4}x{4}! -write \"{5}\" +delete )",
                                    Math.Floor(tileWidth), Math.Floor(tileHeight),
                                    Math.Floor(tileWidth*tileX), Math.Floor(tileHeight*tileY),
                                    outputSize,
                                    outputFile);

                        //Check if the queue isn't full, if it is, process it
                        if (magickQueue.Sum(s => s.Length + 1) + magickArgs.Length + startArgs.Length >
                            maxArgsLength)
                        {
                            RunMagick(startArgs, magickQueue);
                            magickQueue.Clear();
                        }

                        //Add command to queue
                        magickQueue.Add(magickArgs);

                        //Update current progress
                        int currentProgress = ((tileX*tiles + tileY + 1)*100)/(tiles*tiles);
                        
                        UpdateStatus(currentProgress, "Processing zoom " + zoom);
                 
                    }

                //Process last commands of the zoom level
                if (magickQueue.Count <= 0) continue;
                RunMagick(startArgs, magickQueue);
                magickQueue.Clear();
            }

            return true;
        }

        private static void RunMagick(string start, IEnumerable<string> batch)
        {
            RunMagick(start + string.Join(" ", batch));
        }

        private static void RunMagick( string args)
        {
            //Start the magick process
            Process magickProcess = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory + "/", "magick/convert.exe"),
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            magickProcess.OutputDataReceived += (sender, eventArgs) => Debug.Write((eventArgs.Data));

            magickProcess.WaitForExit();    
        }

        #endregion

        #region GDI

        public bool ProcessGDI(string inputPath, string outputPath, int outputSize, int minZoom, int maxZoom,
            bool skipExisting, Size inputSize, string baseName, string outputExtension, ImageFormat outputFormat)
        {
            try
            {
                //Read the input image to memory.
                var baseImage = Image.FromFile(inputPath) as Bitmap;

                //Loop trough every zoom level.
                for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                {
                    UpdateStatus(0, "Processing zoom " + zoom);
      
                    //Caclucate the number of tiles we're processing in this zoom level.
                    int tiles = 1 << zoom;

                    //Calculate the source-tilesize
                    float tileWidth = inputSize.Width/tiles;
                    float tileHeight = inputSize.Height/tiles;

                    //Keep track of our progress
                    int progress = 0;

                    //Loop trough every tile X/Y
                    for (int tileX = 0; tileX < tiles; tileX++)
                        for (int tileY = 0; tileY < tiles; tileY++)
                        {
                            //Generate the output filename
                            string outputFile = Path.Combine(outputPath,
                                string.Format("{0}.{1}.{2}.{3}{4}", baseName, zoom, tileX, tileY, outputExtension));

                            //Skip existing
                            if (skipExisting && File.Exists(outputFile))
                                continue;

                            //Create a new bitmap of the source-tilesize
                            var baseTile = new Bitmap((int) Math.Floor(tileWidth), (int) Math.Floor(tileHeight));
                            using (Graphics g = Graphics.FromImage(baseTile))
                            {
                                //Copy the image from the source
                                g.DrawImage(baseImage, new RectangleF(0, 0, tileWidth, tileHeight),
                                    new RectangleF(tileWidth*tileX, tileHeight*tileY, tileWidth, tileHeight),
                                    GraphicsUnit.Pixel);
                            }

                            //Resize the tile
                            Bitmap tile = ResizeImage(baseTile, new Size(outputSize, outputSize));

                            //Save the tile
                            tile.Save(outputFile, outputFormat);

                            //Dispose tile
                            baseTile.Dispose();
                            tile.Dispose();

                            //Update current progress
                            int currentProgress = ((tileX * tiles + tileY + 1) * 100) / (tiles * tiles);
                            UpdateStatus(currentProgress, "Processing zoom " + zoom);
                        }
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

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

            //return the resulting bitmap
            return newImage;
        }

        #endregion

    }
}
