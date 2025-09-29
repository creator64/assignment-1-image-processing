using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using INFOIBV.Core;
using INFOIBV.Helper_Code;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        /*
         * this enum defines the processing functions that will be shown in the dropdown (a.k.a. combobox)
         * you can expand it by adding new entries to applyProcessingFunction()
         */
        private enum ProcessingFunctions
        {
            Invert,
            AdjustContrast,
            ConvolveImage,
            DetectEdges,
            Threshold,
            BinaryErosion,
            BinaryDilation,
            BinaryOpening,
            BinaryClosing,
            GrayscaleErosion,
            GrayscaleDilation,
            Task1,
            Task2,
            Task3,
            HistogramEqualization,
            MedianFilter,
            LargestRegion
        }
        /*
         * these are the parameters for your processing functions, you should add more as you see fit
         * it is useful to set them based on controls such as sliders, which you can add to the form
         */

        private byte filterSize = 5;
        private float filterSigma = 1f;
        private byte threshold = 127;

        public INFOIBV()
        {
            InitializeComponent();
            populateCombobox();
        }

        /*
         * populateCombobox: populates the combobox with items as defined by the ProcessingFunctions enum
         */
        private void populateCombobox()
        {
            foreach (string itemName in Enum.GetNames(typeof(ProcessingFunctions)))
            {
                string ItemNameSpaces = Regex.Replace(Regex.Replace(itemName, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
                comboBox.Items.Add(ItemNameSpaces);
            }
            comboBox.SelectedIndex = 0;
        }
        /*
         * loadButton_Click: process when user clicks "Load" button
         */
        private void loadImageButton_Click(object sender, EventArgs e)
        {
           if (openImageDialog.ShowDialog() == DialogResult.OK)             // open file dialog
           {
                string file = openImageDialog.FileName;                     // get the file name
                imageFileName.Text = file;                                  // show file name
                if (InputImage != null) InputImage.Dispose();               // reset image
                InputImage = new Bitmap(file);                              // create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // dimension check (may be removed or altered)
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image) InputImage;                 // display input image
           }
        }


        /*
         * applyButton_Click: process when user clicks "Apply" button
         */
        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // create new output image
            Color[,] Image = ConverterMethods.convertBitmapToColor(InputImage);

            // execute image processing steps
            byte[,] workingImage = convertToGrayscale(Image);               // convert image to grayscale

            ImageData data = new ImageData(workingImage);
            if (((ProcessingFunctions)comboBox.SelectedIndex == ProcessingFunctions.BinaryClosing
                || (ProcessingFunctions)comboBox.SelectedIndex == ProcessingFunctions.BinaryOpening
                || (ProcessingFunctions)comboBox.SelectedIndex == ProcessingFunctions.BinaryErosion
                || (ProcessingFunctions)comboBox.SelectedIndex == ProcessingFunctions.BinaryDilation
                || (ProcessingFunctions)comboBox.SelectedIndex == ProcessingFunctions.LargestRegion)
                && data.amountDistinctValues != 2)
                MessageBox.Show("The current image you've selected isn't a binary image, hence you can't perform binary morphological operations over it. Threshold it first to turn it into a binary image.");
            else
            { 
                workingImage = applyProcessingFunction(workingImage);           // processing functions

                OutputImage = ConverterMethods.convertToImage(workingImage);
                pictureBox2.Image = (Image)OutputImage;                         // display output image
            }
        }

        /*
         * applyProcessingFunction: defines behavior of function calls when "Apply" is pressed
         */
        private byte[,] applyProcessingFunction(byte[,] workingImage)
        {
            float[,] horizontalKernel = {
                { -1, -2, -1},
                { 0, 0, 0},
                { 1, 2, 1}
            };

            float[,] verticalKernel = {
                { -1, 0, 1},
                { -2, 0, 2},
                { -1, 0, 1}
            };
            switch ((ProcessingFunctions)comboBox.SelectedIndex)
            {
                case ProcessingFunctions.Invert:
                    return Core.ProcessingFunctions.invertImage(workingImage);
                case ProcessingFunctions.AdjustContrast:
                    return Core.ProcessingFunctions.adjustContrast(workingImage);
                case ProcessingFunctions.ConvolveImage:
                    float[,] filter = Core.ProcessingFunctions.createGaussianFilter(filterSize, filterSigma);
                    return Core.ProcessingFunctions.convolveImage(workingImage, filter);
                case ProcessingFunctions.DetectEdges:
                    return Core.ProcessingFunctions.edgeMagnitude(workingImage, horizontalKernel, verticalKernel);
                case ProcessingFunctions.Threshold:
                    return Core.ProcessingFunctions.thresholdImage(workingImage, threshold);

                case ProcessingFunctions.BinaryErosion:
                    bool[,] structElem = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    }; // Define this structuring element yourself
                    return Core.ProcessingFunctions.binaryErodeImage(workingImage, structElem);

                case ProcessingFunctions.BinaryDilation:
                    bool[,] structElem2 = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    };
                    return Core.ProcessingFunctions.binaryDilateImage(workingImage, structElem2);

                case ProcessingFunctions.BinaryOpening:
                    bool[,] structElem3 = {
                        { false, false, true, false, false},
                        { false, true, true, true, false},
                        { true, true, true, true, true},
                        { false, true, true, true, false },
                        { false, false, true, false, false }
                    };
                    return Core.ProcessingFunctions.binaryOpenImage(workingImage, structElem3);

                case ProcessingFunctions.BinaryClosing:
                    bool[,] structElem4 = {
                        { false, false, true, false, false},
                        { false, true, true, true, false},
                        { true, true, true, true, true},
                        { false, true, true, true, false },
                        { false, false, true, false, false }
                    };
                    return Core.ProcessingFunctions.binaryCloseImage(workingImage, structElem4);

                case ProcessingFunctions.GrayscaleErosion:
                    int[,] grayStructElem = {
                        { 1, 1, 1},
                        { 1, 2, 1},
                        { 1, 2, 1}
                    };
                    return Core.ProcessingFunctions.grayscaleErodeImage(workingImage, grayStructElem);

                case ProcessingFunctions.GrayscaleDilation:
                    int[,] grayStructElem2 = {
                        { 1, 1, 1},
                        { 1, 2, 1},
                        { 1, 2, 1}
                    };
                    return Core.ProcessingFunctions.grayscaleDilateImage(workingImage, grayStructElem2);
                
                case ProcessingFunctions.Task1:
                    decimal sigma = sigmaInput.Value, gaussianMatrixSize = gaussianSize.Value;
                    return Pipelines.GaussianFilterAndEdgeDetection(sigma, gaussianMatrixSize, horizontalKernel,
                        verticalKernel, workingImage);

                case ProcessingFunctions.Task2:
                    byte[,] adjustedImg = Core.ProcessingFunctions.adjustContrast(workingImage);
                    int[,] task2StructElem = FilterGenerators.createSquareFilter<int>((int)task2KernelSize.Value, FilterValueGenerators.createUniformSquareFilter);
                    return Core.ProcessingFunctions.grayscaleDilateImage(adjustedImg, task2StructElem);

                case ProcessingFunctions.Task3:
                    byte[,] imageF = Core.ProcessingFunctions.thresholdImage(workingImage, 127);
                    bool[,] task3StructElem = FilterGenerators.createSquareFilter<bool>((int)task3KernelSize.Value, FilterValueGenerators.createUniformBinaryStructElem);
                    return Core.ProcessingFunctions.binaryCloseImage(imageF, task3StructElem);
                
                case ProcessingFunctions.HistogramEqualization:
                    return Core.ProcessingFunctions.histogramEqualization(workingImage);
                
                case ProcessingFunctions.MedianFilter:
                    return Core.ProcessingFunctions.medianFilter(workingImage, 5);
                
                case ProcessingFunctions.LargestRegion:
                    return Core.ProcessingFunctions.findLargestRegion(workingImage);

                default:
                    return null;
            }
        }


        /*
         * saveButton_Click: process when user clicks "Save" button
         */
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // save the output image
        }
        
        /*
         * convertToGrayScale: convert a three-channel color image to a single channel grayscale image
         * input:   inputImage          three-channel (Color) image
         * output:                      single-channel (byte) image
         */
        private byte[,] convertToGrayscale(Color[,] inputImage)
        {
            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            byte[,] tempImage = ConverterMethods.convertToGrayscale(inputImage, progressBar);
            
            progressBar.Visible = false;                                    // hide progress bar

            return tempImage;
        }


        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 2 GO HERE ==============
        // ====================================================================


        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 3 GO HERE ==============
        // ====================================================================

    }
}