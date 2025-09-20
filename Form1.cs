using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
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
            GrayscaleDilation
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
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage.GetPixel(x, y);                // set pixel color in array at (x,y)

            // execute image processing steps
            byte[,] workingImage = convertToGrayscale(Image);               // convert image to grayscale
            workingImage = applyProcessingFunction(workingImage);           // processing functions

            // copy array to output Bitmap
            for (int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }
            
            pictureBox2.Image = (Image)OutputImage;                         // display output image
        }

        /*
         * applyProcessingFunction: defines behavior of function calls when "Apply" is pressed
         */
        private byte[,] applyProcessingFunction(byte[,] workingImage)
        {
            switch ((ProcessingFunctions)comboBox.SelectedIndex)
            {
                case ProcessingFunctions.Invert:
                    return invertImage(workingImage);
                case ProcessingFunctions.AdjustContrast:
                    return adjustContrast(workingImage);
                case ProcessingFunctions.ConvolveImage:
                    float[,] filter = createGaussianFilter(filterSize, filterSigma);
                    return convolveImage(workingImage, filter);
                case ProcessingFunctions.DetectEdges:
                    sbyte[,] horizontalKernel = null;                       // Define this kernel yourself
                    sbyte[,] verticalKernel = null;                         // Define this kernel yourself
                    return edgeMagnitude(workingImage, horizontalKernel, verticalKernel);
                case ProcessingFunctions.Threshold:
                    return thresholdImage(workingImage, threshold);

                case ProcessingFunctions.BinaryErosion:
                    bool[,] structElem = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    }; // Define this structuring element yourself
                    return binaryErodeImage(workingImage, structElem);

                case ProcessingFunctions.BinaryDilation:
                    bool[,] structElem2 = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    };
                    return binaryDilateImage(workingImage, structElem2);

                case ProcessingFunctions.BinaryOpening:
                    bool[,] structElem3 = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    };
                    return binaryOpenImage(workingImage, structElem3);

                case ProcessingFunctions.BinaryClosing:
                    bool[,] structElem4 = {
                        { false, true, false},
                        { true, false, true},
                        { false, true, false}
                    };
                    return binaryCloseImage(workingImage, structElem4);

                case ProcessingFunctions.GrayscaleErosion:
                    int[,] grayStructElem = null; // Define this structuring element yourself
                    return grayscaleErodeImage(workingImage, grayStructElem);

                case ProcessingFunctions.GrayscaleDilation:
                    grayStructElem = null;
                    return grayscaleDilateImage(workingImage, grayStructElem);


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
            // create temporary grayscale image of the same size as input, with a single channel
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    Color pixelColor = inputImage[x, y];                    // get pixel color
                    byte average = (byte)((pixelColor.R + pixelColor.B + pixelColor.G) / 3); // calculate average over the three channels
                    tempImage[x, y] = average;                              // set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // increment progress bar
                }

            progressBar.Visible = false;                                    // hide progress bar

            return tempImage;
        }


        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 1 GO HERE ==============
        // ====================================================================

        /*
         * invertImage: invert a single channel (grayscale) image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        private byte[,] invertImage(byte[,] inputImage)
        {
            int width = inputImage.GetLength(0), height = inputImage.GetLength(1);
            
            // create temporary grayscale image
            byte[,] tempImage = new byte[width, height];
            
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++)
                tempImage[x, y] = (byte)(255 - inputImage[x, y]);

            return tempImage;
        }


        /*
         * adjustContrast: create an image with the full range of intensity values used
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        private byte[,] adjustContrast(byte[,] inputImage)
        {
            int width = inputImage.GetLength(0), height = inputImage.GetLength(1);
            
            int alow = 255, ahigh = 0;
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++)
            { 
                if (inputImage[x, y] > ahigh) ahigh = inputImage[x, y];
                if (inputImage[x, y] < alow) alow = inputImage[x, y]; 
            }

            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    tempImage[x, y] = (byte)((inputImage[x, y] - alow) * 255 / (ahigh - alow));

            return tempImage;
        }


        /*
         * createGaussianFilter: create a Gaussian filter of specific square size and with a specified sigma
         * input:   size                length and width of the Gaussian filter (only odd sizes)
         *          sigma               standard deviation of the Gaussian distribution
         * output:                      Gaussian filter
         */
        private float[,] createGaussianFilter(byte size, float sigma)
        {
            if (size % 2 == 0) throw new Exception("size of gaussian filter cannot be even");
            int halfSize = size / 2;
            
            // create temporary grayscale image
            float[,] filter = new float[size, size];

            for (int x = -halfSize; x <= halfSize; x++) for (int y = -halfSize; y <= halfSize; y++)
                filter[x + halfSize, y + halfSize] = (float)Math.Exp(
                    -Math.Pow(x, 2) - Math.Pow(y, 2) / (2 * Math.Pow(sigma, 2))
                );

            float sum = 0;
            for (int x = 0; x < size; x++) for (int y = 0; y < size; y++)
                sum += filter[x, y];
            
            for (int x = 0; x < size; x++) for (int y = 0; y < size; y++)
                filter[x, y] /= sum;

            return filter;
        }


        /*
         * convolveImage: apply linear filtering of an input image
         * input:   inputImage          single-channel (byte) image
         *          filter              linear kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] convolveImage(byte[,] inputImage, float[,] filter)
        {          
            Padder padder = new ConstantValuePadder(filter, 0);
            float[,] tempImage = HelperFunctions.applyUnevenFilter(inputImage, filter, padder);

            return HelperFunctions.convertToBytes(tempImage);
        }


        /*
         * edgeMagnitude: calculate the image derivative of an input image and a provided edge kernel
         * input:   inputImage          single-channel (byte) image
         *          horizontalKernel    horizontal edge kernel
         *          verticalKernel      vertical edge kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] edgeMagnitude(byte[,] inputImage, sbyte[,] horizontalKernel, sbyte[,] verticalKernel)
        {
            // create temporary grayscale image
            byte[,] resultImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            float[,] tempImage = new float[inputImage.GetLength(0), inputImage.GetLength(1)];

            #region test values
            //change these around later for the horizontalKernel and verticalKernel parameters
            float[,] vert = {
                { -1, -2, -1},
                { 0, 0, 0},
                { 1, 2, 1}
            };

            float[,] hor = {
                { -1, 0, 1},
                { -2, 0, 2},
                { -1, 0, 1}
            };
            #endregion

            Padder horizontalPadder = new CopyPerimeterPadder(hor);
            Padder verticalPadder = new CopyPerimeterPadder(vert);

            float[,] Dx = HelperFunctions.applyUnevenFilter(inputImage, hor, horizontalPadder);
            float[,] Dy = HelperFunctions.applyUnevenFilter(inputImage, vert, verticalPadder);

            float min = 0.0f;
            float max = 0.0f;

            // calculate edge magnitude
            for (int i = 0; i < tempImage.GetLength(0); i++)
            {
                for (int j = 0; j < tempImage.GetLength(1); j++)
                {
                    float result = (float)Math.Sqrt(Math.Pow(Dx[i, j], 2) + Math.Pow(Dy[i, j], 2));

                    if (result < min) min = result;
                    if (result > max) max = result;

                    tempImage[i, j] = (float)result;
                }
            }

            //normalise edge magnitudes to range 0 - 255
            // and copy to the resulting image
            float trueRange = max - min; //the entire range of values that's currently used

            for (int i = 0; i < tempImage.GetLength(0); i++)
            {
                for (int j = 0; j < tempImage.GetLength(1); j++)
                {

                    resultImage[i, j] = (byte)(255.0f * ((tempImage[i, j] - min) / trueRange));
                }
            }


            return resultImage;
        }
        

        /*
         * thresholdImage: threshold a grayscale image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image with on/off values
         */
        private byte[,] thresholdImage(byte[,] inputImage, byte threshold)
        {
            int width = inputImage.GetLength(0), height = inputImage.GetLength(1);
            
            // create temporary grayscale image
            byte[,] tempImage = new byte[width, height];
            
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++)
                tempImage[x, y] = (byte)(inputImage[x, y] > threshold ? 255 : 0);

            return tempImage;
        }

        // Binary morphology

        /*
         * binaryErodeImage: perform binary erosion on a binary image using a structuring element
         * input:   inputImage          single-channel (byte) binary image 
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after erosion
         */
        private byte[,] binaryErodeImage(byte[,] inputImage, bool[,] structElem)
        {
            float[,] floatStructElem = HelperFunctions.floatifyBoolArray(structElem);
            Padder padder = new ConstantValuePadder(floatStructElem, 0);

            float[,] floatyresult = HelperFunctions.applyMorphologicalFilter(inputImage, floatStructElem, padder, Enumerable.Min<float>, (x, y) => x - y);

            byte[,] output = HelperFunctions.convertToBytes(floatyresult);

            return output;
        }

        /*
         * binaryDilateImage: perform binary dilation on a binary image using a structuring element 
         * input:   inputImage          single-channel (byte) binary image 
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after dilation
         */
        private byte[,] binaryDilateImage(byte[,] inputImage, bool[,] structElem)
        {
            
            float[,] floatStructElem = HelperFunctions.floatifyBoolArray(structElem);
            Padder padder = new ConstantValuePadder(floatStructElem, 0);

            float[,] floatyresult = HelperFunctions.applyMorphologicalFilter(inputImage, floatStructElem, padder, Enumerable.Max<float>, (x, y) => x + y);

            byte[,] output = HelperFunctions.convertToBytes(floatyresult);

            for (int i = 0; i < 5; i++)
            {
                float[,] interres = HelperFunctions.applyMorphologicalFilter(output, floatStructElem, padder, Enumerable.Max<float>, (x, y) => x + y);

                output = HelperFunctions.convertToBytes(interres);
            }
            return output;
        }

        /*
         * binaryOpen
         * Image: perform binary opening on a binary image
         * input:   inputImage          single-channel (byte) binary image 
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after opening
         */
        private byte[,] binaryOpenImage(byte[,] inputImage, bool[,] structElem)
        {
            byte[,] output = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            // TODO: implement binary opening
            return output;
        }

        /*
         * binaryCloseImage: perform binary closing on a binary image
         * input:   inputImage          single-channel (byte) binary image
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after closing
         */
        private byte[,] binaryCloseImage(byte[,] inputImage, bool[,] structElem)
        {
            byte[,] output = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            // TODO: implement binary closing
            return output;
        }

        // Grayscale morphology

        /*
         * grayscaleErodeImage: perform grayscale erosion on a grayscale image using a structuring element
         * input:   inputImage          single-channel (byte) grayscale image
         *          structElem          integer structuring element 
         * output:                      single-channel (byte) grayscale image after erosion
         */
        private byte[,] grayscaleErodeImage(byte[,] inputImage, int[,] structElem)
        {
            byte[,] output = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            // TODO: implement grayscale erosion
            return output;
        }

        /*
         * grayscaleDilateImage: perform grayscale dilation on a grayscale image using a structuring element
         * input:   inputImage          single-channel (byte) grayscale image
         *          structElem          integer structuring element
         * output:                      single-channel (byte) grayscale image after dilation
         */
        private byte[,] grayscaleDilateImage(byte[,] inputImage, int[,] structElem)
        {
            byte[,] output = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            // TODO: implement grayscale dilation
            return output;
        }

        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 2 GO HERE ==============
        // ====================================================================


        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 3 GO HERE ==============
        // ====================================================================

    }
}