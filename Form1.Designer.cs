using System;
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

                sigmaInput.Visible = sigmaLabel.Visible = gaussianLabel.Visible = gaussianSize.Visible = task1Selected;
                task2KernelSize.Visible = task2KernelLabel.Visible = task2Selected;
                task3KernelSize.Visible = task3KernelLabel.Visible = task3Selected;
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
            this.Controls.AddRange(new Control[] { this.task2KernelSize, task2KernelLabel });
            this.Controls.AddRange(new Control[] { this.task3KernelSize, task3KernelLabel });
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

    }
}

