using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using INFOIBV.Core;
using INFOIBV.Helper_Code;
using System.Diagnostics;

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
            LargestRegion,
            HighlightRegions,
            HoughTransformation,
            HoughPeakFinding,
            HoughLineSegments,
            BinaryPipeline,
            GrayscalePipeline,
            DrawIntersectionPoints
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
                || (ProcessingFunctions)comboBox.SelectedIndex == ProcessingFunctions.LargestRegion
                || (ProcessingFunctions)comboBox.SelectedIndex == ProcessingFunctions.HighlightRegions)
                && !data.isBinary())
                MessageBox.Show("The current image you've selected isn't a binary image, hence you can't perform binary morphological operations over it. Threshold it first to turn it into a binary image.");
            else
            { 
                ProcessingImage image = applyProcessingFunction(workingImage);           // processing functions

                OutputImage = image.convertToImage();
                pictureBox2.Image = (Image)OutputImage;                         // display output image
            }
        }

        private ImageRegionFinder selectedRegionFinder()
        {
            switch (regionFinderComboBox.SelectedItem)
            {
                case "Flood Fill": return new FloodFill();
                case "Sequential Labeling": return new SequentialLabeling();
            }

            return new FloodFill();
        }

        /*
         * applyProcessingFunction: defines behavior of function calls when "Apply" is pressed
         */
        private ProcessingImage applyProcessingFunction(byte[,] workingImage)
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

            extraInformation.Text = "";
            ProcessingImage processingImage = new ProcessingImage(workingImage);
            switch ((ProcessingFunctions)comboBox.SelectedIndex)
            {
                case ProcessingFunctions.Invert:
                    return processingImage.invertImage();
                case ProcessingFunctions.AdjustContrast:
                    return processingImage.adjustContrast();
                case ProcessingFunctions.ConvolveImage:
                    float[,] filter = ProcessingImage.createGaussianFilter(filterSize, filterSigma);
                    return processingImage.convolveImage(filter);
                case ProcessingFunctions.DetectEdges:
                    return processingImage.edgeMagnitude(horizontalKernel, verticalKernel);
                case ProcessingFunctions.Threshold:
                    return processingImage.thresholdImage(threshold);

                case ProcessingFunctions.BinaryErosion:
                    bool[,] structElem = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    };
                    return processingImage.binaryErodeImage(structElem);

                case ProcessingFunctions.BinaryDilation:
                    bool[,] structElem2 = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    };
                    return processingImage.binaryDilateImage(structElem2);

                case ProcessingFunctions.BinaryOpening:
                    bool[,] structElem3 = {
                        { false, false, true, false, false},
                        { false, true, true, true, false},
                        { true, true, true, true, true},
                        { false, true, true, true, false },
                        { false, false, true, false, false }
                    };
                    return processingImage.binaryOpenImage(structElem3);

                case ProcessingFunctions.BinaryClosing:
                    bool[,] structElem4 = {
                        { false, false, true, false, false},
                        { false, true, true, true, false},
                        { true, true, true, true, true},
                        { false, true, true, true, false },
                        { false, false, true, false, false }
                    };
                    return processingImage.binaryCloseImage(structElem4);

                case ProcessingFunctions.GrayscaleErosion:
                    int[,] grayStructElem = {
                        { 1, 1, 1},
                        { 1, 2, 1},
                        { 1, 2, 1}
                    };
                    return processingImage.grayscaleErodeImage(grayStructElem);

                case ProcessingFunctions.GrayscaleDilation:
                    int[,] grayStructElem2 = {
                        { 1, 1, 1},
                        { 1, 2, 1},
                        { 1, 2, 1}
                    };
                    return processingImage.grayscaleDilateImage(grayStructElem2);
                
                case ProcessingFunctions.Task1:
                    decimal sigma = sigmaInput.Value, gaussianMatrixSize = gaussianSize.Value;
                    return Pipelines.GaussianFilterAndEdgeDetection(sigma, gaussianMatrixSize, horizontalKernel,
                        verticalKernel, processingImage);

                case ProcessingFunctions.Task2:
                    int[,] task2StructElem = FilterGenerators.createSquareFilter<int>((int)task2KernelSize.Value, FilterValueGenerators.createUniformSquareFilter);
                    return processingImage.adjustContrast().grayscaleDilateImage(task2StructElem);

                case ProcessingFunctions.Task3:
                    bool[,] task3StructElem = FilterGenerators.createSquareFilter<bool>((int)task3KernelSize.Value, FilterValueGenerators.createUniformBinaryStructElem);
                    return processingImage.thresholdImage(127).binaryCloseImage(task3StructElem);
                
                case ProcessingFunctions.HistogramEqualization:
                    return processingImage.histogramEqualization();
                
                case ProcessingFunctions.MedianFilter:
                    return processingImage.medianFilter(5);
                
                case ProcessingFunctions.LargestRegion:
                    return processingImage.toRegionalImage(selectedRegionFinder()).displayLargestRegion();

                case ProcessingFunctions.HighlightRegions:
                    RegionalProcessingImage regionalProcessingImage = processingImage.toRegionalImage(selectedRegionFinder());
                    extraInformation.Text = "amount of regions: " + regionalProcessingImage.amountOfRegions;
                    return regionalProcessingImage.highlightRegions();
                
                case ProcessingFunctions.HoughTransformation :
                    HoughTransform ht = new HoughTransform(processingImage.toArray(), processingImage.width * (int)thetaDetailInput.Value, processingImage.height * (int)rDetailInput.Value);
                    return ht.houghTransform();
                
                case ProcessingFunctions.HoughPeakFinding:
                    
                    bool[,] structElem5 = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    };
                    byte t_peak = (byte)t_peakInput.Value;

                    int thetaDetail = processingImage.width, rDetail = processingImage.height; //this is because the processingImage already is the accumulator Array
                    HoughTransform peakfindingTransform = new HoughTransform(processingImage.toArray(), thetaDetail, rDetail);
                    bool[,] struc = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    };
                    ProcessingImage accumulatorArray = peakfindingTransform.houghTransform();
                    ProcessingImage output = accumulatorArray.halfThresholdImage(t_peak).binaryCloseImage(struc);

                    List<Vector2> peaks = Pipelines.peakFinding(accumulatorArray, t_peak);

                    extraInformation.Text = "peaks <theta, r> (r is normalized): \n" + string.Join(",", peaks);
                    
                    return output; // output;

                case ProcessingFunctions.HoughLineSegments:

                    byte minIntensity = (byte)minIntensityInput.Value;
                    ushort minSegLength = (ushort)minSegLengthInput.Value;
                    ushort maxGap = (ushort)maxGapInput.Value;
                    t_peak = (byte)t_peakInput.Value;
                    thetaDetail = processingImage.width * (int)thetaDetailInput.Value;
                    rDetail = processingImage.height * (int)rDetailInput.Value;

                    HoughTransform htDrawLines = new HoughTransform(processingImage.toArray(), thetaDetail, rDetail);
                    accumulatorArray = htDrawLines.houghTransform();
                    peaks = Pipelines.peakFinding(accumulatorArray, t_peak);
                    Bitmap outputImage = htDrawLines.houghLineSegments(peaks, minIntensity, minSegLength, maxGap);
                    return new RGBProcessingImage(processingImage.toArray(), outputImage);
                case ProcessingFunctions.BinaryPipeline:

                    thetaDetail = processingImage.width * (int)thetaDetailInput.Value;
                    rDetail = processingImage.height * (int)rDetailInput.Value;

                    t_peak = (byte)t_peakInput.Value;
                    minIntensity = (byte)minIntensityInput.Value;
                    minSegLength = (ushort)minSegLengthInput.Value;
                    maxGap = (ushort)maxGapInput.Value;
                    byte t_mag = (byte)t_magInput.Value; 

                    return Pipelines.BinaryPipeline(processingImage, thetaDetail, rDetail, minIntensity, minSegLength, maxGap, t_mag, t_peak, selectedRegionFinder());
                case ProcessingFunctions.GrayscalePipeline:
                    thetaDetail = processingImage.width * (int)thetaDetailInput.Value;
                    rDetail = processingImage.height * (int)rDetailInput.Value;

                    t_peak = (byte)t_peakInput.Value;
                    minIntensity = (byte)minIntensityInput.Value;
                    minSegLength = (ushort)minSegLengthInput.Value;
                    maxGap = (ushort)maxGapInput.Value;

                    return Pipelines.GrayscalePipeline(processingImage, thetaDetail, rDetail, minIntensity, minSegLength, maxGap, t_peak, selectedRegionFinder());

                case ProcessingFunctions.DrawIntersectionPoints:
                    thetaDetail = processingImage.width * (int)thetaDetailInput.Value;
                    rDetail = processingImage.height * (int)rDetailInput.Value;

                    t_peak = (byte)t_peakInput.Value;
                    minIntensity = (byte)minIntensityInput.Value;
                    minSegLength = (ushort)minSegLengthInput.Value;
                    maxGap = (ushort)maxGapInput.Value;

                    HoughTransform htDrawIntersects = new HoughTransform(processingImage.toArray(), thetaDetail, rDetail);
                    accumulatorArray = htDrawIntersects.houghTransform();
                    peaks = Pipelines.peakFinding(accumulatorArray, t_peak);
                    List<(int X, int Y)> intersects = htDrawIntersects.getHoughLineIntersections(peaks, minIntensity, maxGap, minSegLength);
                    outputImage = htDrawIntersects.drawPoints(intersects, Color.Red);
                    return new RGBProcessingImage(processingImage.toArray(), outputImage);
                default:
                    return processingImage;
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