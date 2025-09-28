using System;
using System.Collections.Generic;
using System.Linq;
using INFOIBV.Helper_Code;
using System.Numerics;
using System.Windows.Forms;


namespace INFOIBV.Core
{
    public static class ProcessingFunctions
    {
        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 1 GO HERE ==============
        // ====================================================================

        /*
         * invertImage: invert a single channel (grayscale) image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        public static byte[,] invertImage(byte[,] inputImage)
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
        public static byte[,] adjustContrast(byte[,] inputImage)
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
        public static float[,] createGaussianFilter(byte size, float sigma)
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
        public static byte[,] convolveImage(byte[,] inputImage, float[,] filter)
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
        public static byte[,] edgeMagnitude(byte[,] inputImage, float[,] horizontalKernel, float[,] verticalKernel)
        {
            // create temporary grayscale image
            byte[,] resultImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            float[,] tempImage = new float[inputImage.GetLength(0), inputImage.GetLength(1)];

            #region test values
            //change these around later for the horizontalKernel and verticalKernel parameters
            
            #endregion

            Padder horizontalPadder = new CopyPerimeterPadder(horizontalKernel);
            Padder verticalPadder = new CopyPerimeterPadder(verticalKernel);

            float[,] Dx = HelperFunctions.applyUnevenFilter(inputImage, horizontalKernel, horizontalPadder);
            float[,] Dy = HelperFunctions.applyUnevenFilter(inputImage, verticalKernel, verticalPadder);

            float min = 0.0f;
            float max = 0.0f;

            // calculate edge magnitude
            for (int i = 0; i < tempImage.GetLength(0); i++) for (int j = 0; j < tempImage.GetLength(1); j++)
            {
                float result = (float)Math.Sqrt(Math.Pow(Dx[i, j], 2) + Math.Pow(Dy[i, j], 2));

                if (result < min) min = result;
                if (result > max) max = result;

                tempImage[i, j] = result;
            }

            //normalise edge magnitudes to range 0 - 255
            // and copy to the resulting image
            float trueRange = max - min; //the entire range of values that's currently used

            for (int i = 0; i < tempImage.GetLength(0); i++) for (int j = 0; j < tempImage.GetLength(1); j++)
                resultImage[i, j] = (byte)(255.0f * ((tempImage[i, j] - min) / trueRange));

            return resultImage;
        }
        

        /*
         * thresholdImage: threshold a grayscale image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image with on/off values
         */
        public static byte[,] thresholdImage(byte[,] inputImage, byte threshold)
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
        public static byte[,] binaryErodeImage(byte[,] inputImage, bool[,] structElem)
        {
            ImageData data = new ImageData(inputImage);

            if (data.amountDistinctValues > 2)
            {
                MessageBox.Show("You can only perform binary morphology over binary images. Threshold this image first");
                return inputImage;
            }
            else
            {
                HashSet<Vector2> imageSet = BinaryMorphologyHelpers.pointSetFromImage(inputImage);
                HashSet<Vector2> structSet = BinaryMorphologyHelpers.pointSetFromFilter(structElem);

                HashSet<Vector2> resultSet = new HashSet<Vector2>();

                foreach (Vector2 pixel in imageSet)
                {
                    bool pixelMayRemain = true;
                    foreach (Vector2 element in structSet)
                    {
                        if (!imageSet.Contains(pixel + element))
                        {
                            pixelMayRemain = false;
                            break;
                        }

                    }

                    if (pixelMayRemain) resultSet.Add(pixel);
                }

                return BinaryMorphologyHelpers.pointSetToImage(resultSet, new Vector2(inputImage.GetLength(0), inputImage.GetLength(1)));
            }
        }

        /*
         * binaryDilateImage: perform binary dilation on a binary image using a structuring element
         * input:   inputImage          single-channel (byte) binary image 
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after dilation
         */
        public static byte[,] binaryDilateImage(byte[,] inputImage, bool[,] structElem)
        {
            
            HashSet<Vector2> imageSet = BinaryMorphologyHelpers.pointSetFromImage(inputImage);
            HashSet<Vector2> structSet = BinaryMorphologyHelpers.pointSetFromFilter(structElem);

            HashSet<Vector2> resultSet = new HashSet<Vector2>();
            foreach(Vector2 pixel in imageSet)
                foreach(Vector2 element in structSet)
                    resultSet.Add(pixel + element);

            return BinaryMorphologyHelpers.pointSetToImage(resultSet, new Vector2(inputImage.GetLength(0), inputImage.GetLength(1)));
        }

        /*
         * binaryOpen
         * Image: perform binary opening on a binary image
         * input:   inputImage          single-channel (byte) binary image 
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after opening
         */
        public static byte[,] binaryOpenImage(byte[,] inputImage, bool[,] structElem)
        {
            return binaryDilateImage(binaryErodeImage(inputImage, structElem), structElem);
        }

        /*
         * binaryCloseImage: perform binary closing on a binary image
         * input:   inputImage          single-channel (byte) binary image
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after closing
         */
        public static byte[,] binaryCloseImage(byte[,] inputImage, bool[,] structElem)
        {
            return binaryErodeImage(binaryDilateImage(inputImage, structElem), structElem);
        }

        // Grayscale morphology

        /*
         * grayscaleErodeImage: perform grayscale erosion on a grayscale image using a structuring element
         * input:   inputImage          single-channel (byte) grayscale image
         *          structElem          integer structuring element 
         * output:                      single-channel (byte) grayscale image after erosion
         */
        public static byte[,] grayscaleErodeImage(byte[,] inputImage, int[,] structElem)
        {
            float[,] floatStructElem = HelperFunctions.floatifyIntArray(structElem);
            Padder padder = new ConstantValuePadder(floatStructElem, 0);

            float[,] floatyresult = HelperFunctions.applyMorphologicalFilter(inputImage, floatStructElem, padder, Enumerable.Min<float>, (x, y) => x - y);

            byte[,] output = HelperFunctions.convertToBytes(floatyresult);

            return output;
        }

        /*
         * grayscaleDilateImage: perform grayscale dilation on a grayscale image using a structuring element
         * input:   inputImage          single-channel (byte) grayscale image
         *          structElem          integer structuring element
         * output:                      single-channel (byte) grayscale image after dilation
         */
        public static byte[,] grayscaleDilateImage(byte[,] inputImage, int[,] structElem)
        {
            float[,] floatStructElem = HelperFunctions.floatifyIntArray(structElem);
            Padder padder = new ConstantValuePadder(floatStructElem, 0);

            float[,] floatyresult = HelperFunctions.applyMorphologicalFilter(inputImage, floatStructElem, padder, Enumerable.Max<float>, (x, y) => x + y);

            byte[,] output = HelperFunctions.convertToBytes(floatyresult);

            return output;
        }
        
        /*
         * histogramEqualization: create the histogram-equalized version of the image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        public static byte[,] histogramEqualization(byte[,] inputImage)
        {
            int[] cumulativeHistogram = HelperFunctions.calculateCumulativeHistogram(inputImage);
            int amountOfPixels = inputImage.GetLength(0) * inputImage.GetLength(1);
            int K = cumulativeHistogram.Length;
            
            int[] mappedPixels = new int[K];
            for (int i = 0; i < K; i++)
                mappedPixels[i] = (int)Math.Floor((K - 1d) * cumulativeHistogram[i] / amountOfPixels);

            byte[,] outputImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            for (int x = 0; x < inputImage.GetLength(0); x++) for (int y = 0; y < inputImage.GetLength(1); y++)
                outputImage[x, y] = (byte)mappedPixels[inputImage[x, y]];

            return outputImage;
        }

        public static byte[,] medianFilter(byte[,] inputImage, int filterSize)
        {
            float[,] filter = FilterGenerators.createSquareFilter(filterSize, (int _, int __, float[,] ___) => 0);
            Padder padder = new ConstantValuePadder(filter, 0);
            int paddingWidth = padder.paddingWidth, paddingHeight = padder.paddingHeight;

            Func<int, int, byte[,], float> f = (i, j, paddedImage) =>
            {
                int[] intensities = new int[filterSize * filterSize];
                int a = 0;
                for (int k = -paddingWidth; k <= paddingWidth; k++)
                for (int l = -paddingHeight; l <= paddingHeight; l++)
                    intensities[a++] = paddedImage[i + k, j + l];

                Array.Sort(intensities);

                return intensities[intensities.Length / 2];
            };

            return HelperFunctions.applyNonLinearFilter(inputImage, padder, f);
        }

        public static byte[,] findLargestRegion(byte[,] inputImage)
        {
            byte[,] outputImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            List<List<Vector2>> regions = ImageRegions.findRegions(inputImage);
            List<Vector2> largestRegion = regions.Aggregate((r1, r2) => r1.Count > r2.Count ? r1 : r2);
            
            foreach (Vector2 point in largestRegion)
                outputImage[(int)point.X, (int)point.Y] = 255;

            return outputImage;
        }
    }
}