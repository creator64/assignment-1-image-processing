using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace INFOIBV
{
    partial class INFOIBV
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
            this.LoadImageButton = new System.Windows.Forms.Button();
            this.openImageDialog = new System.Windows.Forms.OpenFileDialog();
            this.imageFileName = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.saveImageDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveButton = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.gaussianSize = new System.Windows.Forms.NumericUpDown();
            this.gaussianLabel = new Label();
            this.sigmaInput = new System.Windows.Forms.NumericUpDown();
            this.sigmaLabel = new Label();
            this.task2KernelSize = new System.Windows.Forms.NumericUpDown();
            this.task2KernelLabel = new Label();
            this.task3KernelSize = new System.Windows.Forms.NumericUpDown();
            this.task3KernelLabel = new Label();
            this.extraInformation = new Label();
            this.regionFinderComboBox = new System.Windows.Forms.ComboBox();

            #region Hough Transform initialisations

            //Inputs
            this.t_magInput = new System.Windows.Forms.NumericUpDown();
            this.thetaDetailInput = new System.Windows.Forms.NumericUpDown();
            this.rDetailInput = new System.Windows.Forms.NumericUpDown();
            this.t_peakInput = new System.Windows.Forms.NumericUpDown();
            this.minIntensityInput = new System.Windows.Forms.NumericUpDown();
            this.minSegLengthInput = new System.Windows.Forms.NumericUpDown();
            this.maxGapInput = new System.Windows.Forms.NumericUpDown();

            //labels
            this.t_magLabel = new System.Windows.Forms.Label();
            this.thetaDetailLabel = new System.Windows.Forms.Label();
            this.rDetailLabel = new System.Windows.Forms.Label();
            this.t_peakLabel = new System.Windows.Forms.Label();
            this.minIntensityLabel = new System.Windows.Forms.Label();
            this.minSegLengthLabel = new System.Windows.Forms.Label();
            this.maxGapLabel = new System.Windows.Forms.Label();

            #endregion
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // LoadImageButton
            // 
            this.LoadImageButton.Location = new System.Drawing.Point(12, 11);
            this.LoadImageButton.Name = "LoadImageButton";
            this.LoadImageButton.Size = new System.Drawing.Size(98, 23);
            this.LoadImageButton.TabIndex = 0;
            this.LoadImageButton.Text = "Load image...";
            this.LoadImageButton.UseVisualStyleBackColor = true;
            this.LoadImageButton.Click += new System.EventHandler(this.loadImageButton_Click);
            // 
            // openImageDialog
            // 
            this.openImageDialog.Filter = "Bitmap files (*.bmp;*.gif;*.jpg;*.png;*.tiff;*.jpeg)|*.bmp;*.gif;*.jpg;*.png;*.ti" +
    "ff;*.jpeg";
            this.openImageDialog.InitialDirectory = "..\\..\\images";
            // 
            // imageFileName
            // 
            this.imageFileName.Location = new System.Drawing.Point(116, 12);
            this.imageFileName.Name = "imageFileName";
            this.imageFileName.ReadOnly = true;
            this.imageFileName.Size = new System.Drawing.Size(329, 20);
            this.imageFileName.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(13, 100);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(512, 512);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(658, 11);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(103, 23);
            this.applyButton.TabIndex = 3;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // saveImageDialog
            // 
            this.saveImageDialog.Filter = "Bitmap file (*.bmp)|*.bmp";
            this.saveImageDialog.InitialDirectory = "..\\..\\images";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(948, 11);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(95, 23);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "Save as BMP...";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(531, 100);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(512, 512);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(767, 13);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(175, 20);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 6;
            this.progressBar.Visible = false;
            // 
            // comboBox
            // 
            this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox.FormattingEnabled = true;
            this.comboBox.Location = new System.Drawing.Point(451, 12);
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(201, 21);
            this.comboBox.TabIndex = 7;
            this.comboBox.SelectedIndexChanged += (sender, args) =>
            {
                // sorry for the hardcode im lazy
                bool task1Selected = this.comboBox.Text == "Task 1";
                bool task2Selected = this.comboBox.Text == "Task 2";
                bool task3Selected = this.comboBox.Text == "Task 3";
                bool binaryPipelineSelected = this.comboBox.Text == "Binary Pipeline";
                bool grayscalePipelineSelected = this.comboBox.Text == "Grayscale Pipeline";
                bool houghLineSegmentsSelected = this.comboBox.Text == "Hough Line Segments";
                bool houghPeaksDetected = this.comboBox.Text == "Hough Peak Finding";
                bool getPeaksDetected = binaryPipelineSelected || grayscalePipelineSelected || houghLineSegmentsSelected || houghPeaksDetected;
                bool houghTransformSelected = binaryPipelineSelected || grayscalePipelineSelected || houghLineSegmentsSelected || this.comboBox.Text == "Hough Transformation" || houghPeaksDetected;
                bool regionDetectionSelected = this.comboBox.Text == "Highlight Regions" || this.comboBox.Text == "Largest Region";

                sigmaInput.Visible = sigmaLabel.Visible = gaussianLabel.Visible = gaussianSize.Visible = task1Selected;
                task2KernelSize.Visible = task2KernelLabel.Visible = task2Selected;
                task3KernelSize.Visible = task3KernelLabel.Visible = task3Selected;
                regionFinderComboBox.Visible = regionDetectionSelected;

                #region HoughTransform Visibility
                t_magInput.Visible = t_magLabel.Visible = binaryPipelineSelected;
                t_peakInput.Visible = t_peakLabel.Visible = minIntensityInput.Visible = minIntensityLabel.Visible = minSegLengthInput.Visible = minSegLengthLabel.Visible = maxGapInput.Visible = maxGapLabel.Visible = getPeaksDetected;
                thetaDetailInput.Visible = thetaDetailLabel.Visible = rDetailInput.Visible = rDetailLabel.Visible = houghTransformSelected;

                if (this.comboBox.Text == "Hough Transformation") //some good values
                {
                    thetaDetailInput.Value = 1;
                    rDetailInput.Value = 1;
                }
                else
                {
                    thetaDetailInput.Value = 2;
                    rDetailInput.Value = 2;
                }
                #endregion
            };

            //
            // Gaussian Size number input
            // 
            this.gaussianLabel.Text = "Size of Gaussian matrix";
            this.gaussianLabel.Location = new System.Drawing.Point(12, 41);
            this.gaussianLabel.Size = new Size(125, 20);
            this.gaussianSize.Location = new System.Drawing.Point(15, 60);
            this.gaussianSize.Name = "GaussianSizeInput";
            this.gaussianSize.Size = new System.Drawing.Size(98, 50);
            this.gaussianSize.ValueChanged += new System.EventHandler((object sender, System.EventArgs e) =>
            {
                if (gaussianSize.Value % 2 == 0) gaussianSize.Value += 1;
            });
            this.gaussianSize.Increment = 2;
            this.gaussianSize.Value = 3;
            this.gaussianSize.Maximum = 19;
            this.gaussianSize.Minimum = 1;

            //
            // Task 2 Kernel Size
            //
            this.task2KernelLabel.Text = "Size of Task 2's filter matrix";
            this.task2KernelLabel.Location = new System.Drawing.Point(12, 41);
            this.task2KernelLabel.Size = new Size(125, 20);
            this.task2KernelSize.Location = new System.Drawing.Point(15, 60);
            this.task2KernelSize.Name = "Task2SizeInput";
            this.task2KernelSize.Size = new System.Drawing.Size(98, 50);
            this.task2KernelSize.ValueChanged += new System.EventHandler((object sender, System.EventArgs e) =>
            {
                if (gaussianSize.Value % 2 == 0) gaussianSize.Value += 1;
            });
            this.task2KernelSize.Increment = 2;
            this.task2KernelSize.Value = 3;
            this.task2KernelSize.Maximum = 19;
            this.task2KernelSize.Minimum = 1;


            //
            // Task 3 Kernel Size
            //
            this.task3KernelLabel.Text = "Size of Task 3's filter matrix";
            this.task3KernelLabel.Location = new System.Drawing.Point(12, 41);
            this.task3KernelLabel.Size = new Size(125, 20);
            this.task3KernelSize.Location = new System.Drawing.Point(15, 60);
            this.task3KernelSize.Name = "Task3SizeInput";
            this.task3KernelSize.Size = new System.Drawing.Size(98, 50);
            this.task3KernelSize.ValueChanged += new System.EventHandler((object sender, System.EventArgs e) =>
            {
                if (gaussianSize.Value % 2 == 0) gaussianSize.Value += 1;
            });
            this.task3KernelSize.Increment = 10;
            this.task3KernelSize.Value = 3;
            this.task3KernelSize.Maximum = 33;
            this.task3KernelSize.Minimum = 1;
            //
            // Gaussian Size number input
            // 
            this.sigmaLabel.Text = "sigma";
            this.sigmaLabel.Location = new System.Drawing.Point(142, 41);
            this.sigmaLabel.Size = new Size(50, 20);
            this.sigmaInput.Location = new System.Drawing.Point(145, 60);
            this.sigmaInput.Name = "GaussianSizeInput";
            this.sigmaInput.Size = new System.Drawing.Size(98, 50);
            this.sigmaInput.Increment = 0.25m;
            this.sigmaInput.DecimalPlaces = 2;
            this.sigmaInput.Value = 2.25m;
            this.sigmaInput.Minimum = 0;
            //
            // Extra information
            //
            this.extraInformation.Location = new System.Drawing.Point(531, 50);
            this.extraInformation.Size = new Size(8000, 100);
            //
            // regionFinderComboBox
            //
            this.regionFinderComboBox.Location = new System.Drawing.Point(15, 60);
            this.regionFinderComboBox.Items.AddRange(new []{"Flood Fill", "Sequential Labeling"});
            this.regionFinderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;;
            this.regionFinderComboBox.SelectedIndex = 0;

            ///
            // thetaDetail
            //
            this.thetaDetailLabel.Text = "Theta Detail";
            this.thetaDetailLabel.Location = new System.Drawing.Point(142, 41);
            this.thetaDetailLabel.Size = new Size(100, 20);
            this.thetaDetailInput.Location = new System.Drawing.Point(145, 60);
            this.thetaDetailInput.Name = "thetaDetailInput";
            this.thetaDetailInput.Size = new System.Drawing.Size(98, 50);
            this.thetaDetailInput.Increment = 1;
            this.thetaDetailInput.DecimalPlaces = 0;
            this.thetaDetailInput.Value = 2;
            this.thetaDetailInput.Maximum = 3;
            this.thetaDetailInput.Minimum = 1;

            ///
            // rDetail
            //
            this.rDetailLabel.Text = "R Detail";
            this.rDetailLabel.Location = new System.Drawing.Point(250, 41);
            this.rDetailLabel.Size = new Size(100, 20);
            this.rDetailInput.Location = new System.Drawing.Point(250, 60);
            this.rDetailInput.Name = "rDetailInput";
            this.rDetailInput.Size = new System.Drawing.Size(98, 50);
            this.rDetailInput.Increment = 1;
            this.rDetailInput.DecimalPlaces = 0;
            this.rDetailInput.Value = 2;
            this.rDetailInput.Maximum = 3;
            this.rDetailInput.Minimum = 1;


            ///
            // t_peak
            //
            this.t_peakLabel.Text = "t_peak";
            this.t_peakLabel.Location = new System.Drawing.Point(350, 41);
            this.t_peakLabel.Size = new Size(100, 20);
            this.t_peakInput.Location = new System.Drawing.Point(350, 60);
            this.t_peakInput.Name = "rDetailInput";
            this.t_peakInput.Size = new System.Drawing.Size(98, 50);
            this.t_peakInput.Increment = 10;
            this.t_peakInput.DecimalPlaces = 0;
            this.t_peakInput.Value = 80;
            this.t_peakInput.Maximum = 255;
            this.t_peakInput.Minimum = 0;

            ///
            // minIntensity
            //
            this.minIntensityLabel.Text = "Min. Intensity";
            this.minIntensityLabel.Location = new System.Drawing.Point(450, 41);
            this.minIntensityLabel.Size = new Size(100, 20);
            this.minIntensityInput.Location = new System.Drawing.Point(450, 60);
            this.minIntensityInput.Name = "minIntensityInput";
            this.minIntensityInput.Size = new System.Drawing.Size(98, 50);
            this.minIntensityInput.Increment = 10;
            this.minIntensityInput.DecimalPlaces = 0;
            this.minIntensityInput.Value = 50;
            this.minIntensityInput.Maximum = 255;
            this.minIntensityInput.Minimum = 0;

            ///
            // minSegLength
            //
            this.minSegLengthLabel.Text = "Min. Segment Length";
            this.minSegLengthLabel.Location = new System.Drawing.Point(550, 41);
            this.minSegLengthLabel.Size = new Size(100, 20);
            this.minSegLengthInput.Location = new System.Drawing.Point(550, 60);
            this.minSegLengthInput.Name = "minSegLengthInput";
            this.minSegLengthInput.Size = new System.Drawing.Size(98, 50);
            this.minSegLengthInput.Increment = 5;
            this.minSegLengthInput.DecimalPlaces = 0;
            this.minSegLengthInput.Maximum = ushort.MaxValue;
            this.minSegLengthInput.Value = 20;
            this.minSegLengthInput.Minimum = 0;

            ///
            // maxGap
            //
            this.maxGapLabel.Text = "Max Gap";
            this.maxGapLabel.Location = new System.Drawing.Point(650, 41);
            this.maxGapLabel.Size = new Size(100, 20);
            this.maxGapInput.Location = new System.Drawing.Point(650, 60);
            this.maxGapInput.Name = "maxGapInput";
            this.maxGapInput.Size = new System.Drawing.Size(98, 50);
            this.maxGapInput.Increment = 5;
            this.maxGapInput.DecimalPlaces = 0;
            this.maxGapInput.Maximum = ushort.MaxValue;
            this.maxGapInput.Value = 7;
            this.maxGapInput.Minimum = 0;

            ///
            // t_mag
            //
            this.t_magLabel.Text = "t_mag";
            this.t_magLabel.Location = new System.Drawing.Point(750, 41);
            this.t_magLabel.Size = new Size(100, 20);
            this.t_magInput.Location = new System.Drawing.Point(750, 60);
            this.t_magInput.Name = "t_magInput";
            this.t_magInput.Size = new System.Drawing.Size(98, 50);
            this.t_magInput.Increment = 10;
            this.t_magInput.DecimalPlaces = 0;
            this.t_magInput.Value = 80;
            this.t_magInput.Maximum = 255;
            this.t_magInput.Minimum = 0;

            //
            // INFOIBV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1052, 576);
            this.Controls.Add(this.comboBox);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.imageFileName);
            this.Controls.Add(this.LoadImageButton);
            this.Controls.AddRange(new Control[] {this.gaussianSize, gaussianLabel});
            this.Controls.AddRange(new Control[] {this.sigmaInput, sigmaLabel});
            this.Controls.AddRange(new Control[] { this.t_magInput, t_magLabel });
            this.Controls.AddRange(new Control[] { this.t_peakInput, t_peakLabel });
            this.Controls.AddRange(new Control[] { this.thetaDetailInput, thetaDetailLabel });
            this.Controls.AddRange(new Control[] { this.rDetailInput, rDetailLabel });
            this.Controls.AddRange(new Control[] { this.minIntensityInput, minIntensityLabel });
            this.Controls.AddRange(new Control[] { this.minSegLengthInput, minSegLengthLabel });
            this.Controls.AddRange(new Control[] { this.maxGapInput, maxGapLabel });

            this.Controls.AddRange(new Control[] { this.task2KernelSize, task2KernelLabel });
            this.Controls.AddRange(new Control[] { this.task3KernelSize, task3KernelLabel });
            this.Controls.Add(this.extraInformation);
            this.Controls.Add(this.regionFinderComboBox);
            this.Location = new System.Drawing.Point(10, 10);
            this.Name = "INFOIBV";
            this.ShowIcon = false;
            this.Text = "INFOIBV";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadImageButton;
        private System.Windows.Forms.OpenFileDialog openImageDialog;
        private System.Windows.Forms.TextBox imageFileName;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.SaveFileDialog saveImageDialog;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ComboBox comboBox;
        private System.Windows.Forms.NumericUpDown gaussianSize;
        private System.Windows.Forms.NumericUpDown sigmaInput;
        private System.Windows.Forms.Label gaussianLabel;
        private System.Windows.Forms.Label sigmaLabel;
        private System.Windows.Forms.NumericUpDown task2KernelSize;
        private System.Windows.Forms.NumericUpDown task3KernelSize;
        private System.Windows.Forms.Label task2KernelLabel;
        private System.Windows.Forms.Label task3KernelLabel;
        private System.Windows.Forms.Label extraInformation;
        private System.Windows.Forms.ComboBox regionFinderComboBox;

        #region Hough Transform Shenanigans

        //Inputs
        private System.Windows.Forms.NumericUpDown t_magInput;
        private System.Windows.Forms.NumericUpDown thetaDetailInput;
        private System.Windows.Forms.NumericUpDown rDetailInput;
        private System.Windows.Forms.NumericUpDown t_peakInput;
        private System.Windows.Forms.NumericUpDown minIntensityInput;
        private System.Windows.Forms.NumericUpDown minSegLengthInput;
        private System.Windows.Forms.NumericUpDown maxGapInput;

        //labels
        private System.Windows.Forms.Label t_magLabel;
        private System.Windows.Forms.Label thetaDetailLabel;
        private System.Windows.Forms.Label rDetailLabel;
        private System.Windows.Forms.Label t_peakLabel;
        private System.Windows.Forms.Label minIntensityLabel;
        private System.Windows.Forms.Label minSegLengthLabel;
        private System.Windows.Forms.Label maxGapLabel;

        #endregion

    }
}

