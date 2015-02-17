using TileCutter.Processors;

namespace TileCutter
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.skipExistingcheckBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.outputTypeComboBox = new System.Windows.Forms.ComboBox();
            this.targetSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.maxZoomNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.minZoomNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.outputPathBrowseButton = new System.Windows.Forms.Button();
            this.outputPathTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.inputPathBrowseButton = new System.Windows.Forms.Button();
            this.inputPathTextBox = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.label7 = new System.Windows.Forms.Label();
            this.processorComboBox = new System.Windows.Forms.ComboBox();
            this.optionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targetSizeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxZoomNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minZoomNumericUpDown)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.label7);
            this.optionsGroupBox.Controls.Add(this.processorComboBox);
            this.optionsGroupBox.Controls.Add(this.skipExistingcheckBox);
            this.optionsGroupBox.Controls.Add(this.label6);
            this.optionsGroupBox.Controls.Add(this.outputTypeComboBox);
            this.optionsGroupBox.Controls.Add(this.targetSizeNumericUpDown);
            this.optionsGroupBox.Controls.Add(this.label5);
            this.optionsGroupBox.Controls.Add(this.startButton);
            this.optionsGroupBox.Controls.Add(this.maxZoomNumericUpDown);
            this.optionsGroupBox.Controls.Add(this.label4);
            this.optionsGroupBox.Controls.Add(this.label3);
            this.optionsGroupBox.Controls.Add(this.minZoomNumericUpDown);
            this.optionsGroupBox.Controls.Add(this.label2);
            this.optionsGroupBox.Controls.Add(this.outputPathBrowseButton);
            this.optionsGroupBox.Controls.Add(this.outputPathTextBox);
            this.optionsGroupBox.Controls.Add(this.label1);
            this.optionsGroupBox.Controls.Add(this.inputPathBrowseButton);
            this.optionsGroupBox.Controls.Add(this.inputPathTextBox);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(277, 258);
            this.optionsGroupBox.TabIndex = 0;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            // 
            // skipExistingcheckBox
            // 
            this.skipExistingcheckBox.AutoSize = true;
            this.skipExistingcheckBox.Location = new System.Drawing.Point(6, 224);
            this.skipExistingcheckBox.Name = "skipExistingcheckBox";
            this.skipExistingcheckBox.Size = new System.Drawing.Size(106, 17);
            this.skipExistingcheckBox.TabIndex = 11;
            this.skipExistingcheckBox.Text = "Skip existing files";
            this.skipExistingcheckBox.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Ouput type:";
            // 
            // outputTypeComboBox
            // 
            this.outputTypeComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.outputTypeComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.outputTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.outputTypeComboBox.FormattingEnabled = true;
            this.outputTypeComboBox.Items.AddRange(new object[] {
            ".PNG",
            ".BMP",
            ".JPEG",
            ".GIF"});
            this.outputTypeComboBox.Location = new System.Drawing.Point(113, 98);
            this.outputTypeComboBox.Name = "outputTypeComboBox";
            this.outputTypeComboBox.Size = new System.Drawing.Size(127, 21);
            this.outputTypeComboBox.TabIndex = 0;
            // 
            // targetSizeNumericUpDown
            // 
            this.targetSizeNumericUpDown.Location = new System.Drawing.Point(113, 202);
            this.targetSizeNumericUpDown.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.targetSizeNumericUpDown.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.targetSizeNumericUpDown.Name = "targetSizeNumericUpDown";
            this.targetSizeNumericUpDown.Size = new System.Drawing.Size(46, 20);
            this.targetSizeNumericUpDown.TabIndex = 9;
            this.targetSizeNumericUpDown.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.targetSizeNumericUpDown.ValueChanged += new System.EventHandler(this.targetSizeNumericUpDown_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 204);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Target size:";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(192, 222);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // maxZoomNumericUpDown
            // 
            this.maxZoomNumericUpDown.Location = new System.Drawing.Point(113, 178);
            this.maxZoomNumericUpDown.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.maxZoomNumericUpDown.Name = "maxZoomNumericUpDown";
            this.maxZoomNumericUpDown.Size = new System.Drawing.Size(46, 20);
            this.maxZoomNumericUpDown.TabIndex = 7;
            this.maxZoomNumericUpDown.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 180);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Max zoom level:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 154);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Min zoom level:";
            // 
            // minZoomNumericUpDown
            // 
            this.minZoomNumericUpDown.Location = new System.Drawing.Point(113, 152);
            this.minZoomNumericUpDown.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.minZoomNumericUpDown.Name = "minZoomNumericUpDown";
            this.minZoomNumericUpDown.Size = new System.Drawing.Size(46, 20);
            this.minZoomNumericUpDown.TabIndex = 0;
            this.minZoomNumericUpDown.ValueChanged += new System.EventHandler(this.minZoomNumericUpDown_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output folder:";
            // 
            // outputPathBrowseButton
            // 
            this.outputPathBrowseButton.Location = new System.Drawing.Point(165, 69);
            this.outputPathBrowseButton.Name = "outputPathBrowseButton";
            this.outputPathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.outputPathBrowseButton.TabIndex = 4;
            this.outputPathBrowseButton.Text = "Browse...";
            this.outputPathBrowseButton.UseVisualStyleBackColor = true;
            this.outputPathBrowseButton.Click += new System.EventHandler(this.outputPathBrowseButton_Click);
            // 
            // outputPathTextBox
            // 
            this.outputPathTextBox.Location = new System.Drawing.Point(6, 71);
            this.outputPathTextBox.Name = "outputPathTextBox";
            this.outputPathTextBox.ReadOnly = true;
            this.outputPathTextBox.Size = new System.Drawing.Size(153, 20);
            this.outputPathTextBox.TabIndex = 5;
            this.outputPathTextBox.Text = " ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input image:";
            // 
            // inputPathBrowseButton
            // 
            this.inputPathBrowseButton.Location = new System.Drawing.Point(165, 30);
            this.inputPathBrowseButton.Name = "inputPathBrowseButton";
            this.inputPathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.inputPathBrowseButton.TabIndex = 0;
            this.inputPathBrowseButton.Text = "Browse...";
            this.inputPathBrowseButton.UseVisualStyleBackColor = true;
            this.inputPathBrowseButton.Click += new System.EventHandler(this.inputPathBrowseButton_Click);
            // 
            // inputPathTextBox
            // 
            this.inputPathTextBox.Location = new System.Drawing.Point(6, 32);
            this.inputPathTextBox.Name = "inputPathTextBox";
            this.inputPathTextBox.ReadOnly = true;
            this.inputPathTextBox.Size = new System.Drawing.Size(153, 20);
            this.inputPathTextBox.TabIndex = 2;
            this.inputPathTextBox.Text = " ";
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            this.openFileDialog.Filter = "Images|*.bmp;*.png;*.jpg;*.gif";
            // 
            // statusStrip1
            // 
            this.statusStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 258);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(277, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(130, 16);
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(76, 17);
            this.toolStripStatusLabel.Text = "By Tim Potze";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.optionsGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(277, 258);
            this.panel1.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 128);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Processor:";
            // 
            // processorComboBox
            // 
            this.processorComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.processorComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.processorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.processorComboBox.FormattingEnabled = true;
            this.processorComboBox.Location = new System.Drawing.Point(113, 125);
            this.processorComboBox.Name = "processorComboBox";
            this.processorComboBox.Size = new System.Drawing.Size(127, 21);
            this.processorComboBox.TabIndex = 12;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 280);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SanMap tile cutter";
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targetSizeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxZoomNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minZoomNumericUpDown)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button outputPathBrowseButton;
        private System.Windows.Forms.TextBox outputPathTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button inputPathBrowseButton;
        private System.Windows.Forms.TextBox inputPathTextBox;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.NumericUpDown maxZoomNumericUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown minZoomNumericUpDown;
        private System.Windows.Forms.NumericUpDown targetSizeNumericUpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox outputTypeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox skipExistingcheckBox;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox processorComboBox;
    }
}

