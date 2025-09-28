using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace INFOIBV.Helper_Code
{
    /// <summary>
    /// A static class with helper functions to make main codebase more readable.
    /// </summary>
    public static class HelperFunctions
    {
        private static float getFilterWeight(float[,] filter)
        {
            float filterWeight = 0;

            for (int i = 0; i < filter.GetLength(0); i++)
            {
                for (int j = 0; j < filter.GetLength(1); j++)
                {
                    filterWeight += Math.Abs(filter[i, j]);
                }
            }

            return filterWeight;
        }

        /// <summary>
        /// convert a 2D array of floats to a 2D array of bytes
        /// </summary>
        /// <param name="I">the 2D float array to be converted</param>
        public static byte[,] convertToBytes(float[,] I)
        {
            int width = I.GetLength(0), height = I.GetLength(1);
            byte[,] tempImage = new byte[width, height];
            for (int i = 0; i < width; i++) for (int j = 0; j < height; j++) 
                tempImage[i, j] = (byte)I[i, j];

            return tempImage;
        }

        public static float[,] floatifyIntArray(int[,] a)
        {
            float[,] result = new float[a.GetLength(0), a.GetLength(1)];

            for (int i = 0; i < a.GetLength(0); i++)
                for (int j = 0; j < a.GetLength(1); j++)
                    result[i, j] = (float)a[i, j];

            return result;
        }

        public static float[,] copyImage(byte[,] I)
        {
            float[,] copy = new float[I.GetLength(0), I.GetLength(1)];

            for (int i = 0; i < copy.GetLength(0); i++) for (int j = 0; j < copy.GetLength(1); j++)
                    copy[i, j] = I[i, j];
            return copy;
        }

        /// <summary>
        /// A Function to copy an altered (padded) image back to its original (unpadded) source Image
        /// </summary>
        /// <param name="source">the source Image (unpadded)</param>
        /// <param name="Ipadded">the padded variant of the source Image</param>
        /// <param name="padder">the padder used to pad the source Image</param>
        public static void overwriteImage(float[,] source, byte[,] Ipadded, Padder padder)
        {
            for (int i = padder.paddingWidth; i < source.GetLength(0) + padder.paddingWidth; i++)
            {
                for (int j = padder.paddingHeight; j < source.GetLength(1) + padder.paddingHeight; j++)
                {
                    //paddingWidth and filterWidth are equivalent to one another.
                    source[i - padder.paddingWidth, j - padder.paddingHeight] = Ipadded[i, j];
                }
            }
        }

        /// <summary>
        /// Applying an uneven filter 
        /// </summary>
        /// <param name="I">The Original Image</param>
        /// <param name="H">The filter-matrix you want to apply to I</param>
        /// <param name="method">The padding method you want to apply</param>
        /// <param name="paddingValue">
        /// Only relevant when padding with PaddingMethods.ConstantValue.
        /// Causes the paddingmethod to pad with the given value, defaults to 0 (black)
        /// </param>
        /// <returns>The result of I * H</returns>
        public static float[,] applyUnevenFilter(byte[,] I, float[,] H, Padder padder)
        {
            byte[,] paddedImage = padder.padImage(I);

            float[,] backupImage = copyImage(I);

            for (int i = padder.paddingWidth; i < backupImage.GetLength(0) + padder.paddingWidth; i++)
            {
                for (int j = padder.paddingHeight; j < backupImage.GetLength(1) + padder.paddingHeight; j++)
                {
                    //paddingWidth and filterWidth are equivalent to one another.
                    backupImage[i - padder.paddingWidth, j - padder.paddingHeight] = applyUnevenFilterPass(i, j, paddedImage, H, padder.paddingWidth, padder.paddingHeight, getFilterWeight(H));
                }
            }

            return backupImage;
        }
        /// <summary>
        /// Apply a single pass of a filter over a padded image's pixel
        /// </summary>
        /// <param name="i">x coordinate of the padded image</param>
        /// <param name="j">y coordinate of the padded image</param>
        /// <param name="paddedImage">the padded image itself</param>
        /// <param name="filter">the filter matrix H</param>
        /// <param name="filterWidth">the width of the filter (how far it extends to the left and right)</param>
        /// <param name="filterHeight">the height of the filter (how far it extends to the top and bottom)</param>
        /// <param name="filterWeight">the total weight (sum) of all elements in the filter matrix</param>
        /// <returns>The pixel at (i, j) after applying the filter</returns>
        private static float applyUnevenFilterPass(int i, int j, byte[,] paddedImage, float[,] filter, int filterWidth, int filterHeight, float filterWeight)
        {
            float filteredValue = 0.0f;
            for (int k = -filterWidth ; k <= filterWidth; k++)
            {
                for (int l = -filterHeight; l <= filterHeight; l++)
                {
                    filteredValue += (float)paddedImage[i + k, j + l] * (float)filter[k + filterWidth, l + filterHeight];
                }
            }

            if (filterWeight != 0.0f)
                return (filteredValue / (float)filterWeight);
            else
                return filteredValue;
        }

        public static float[,] applyMorphologicalFilter(byte[,] I, float[,] H, Padder padder, Func<List<float>, float> selector, Func<float, float, float> arithmeticOperator, int threshold = 0)
        {
            byte[,] paddedImage = padder.padImage(I);

            float[,] backupPadded = copyImage(paddedImage);
            float[,] backupImage = copyImage(I);

            for (int i = padder.paddingWidth; i < backupImage.GetLength(0) + padder.paddingWidth; i++)
            {
                for (int j = padder.paddingHeight; j < backupImage.GetLength(1) + padder.paddingHeight; j++)
                {
                    if (I[i - padder.paddingWidth, j - padder.paddingWidth] >= threshold)
                    {
                        applyMorphologicalFilterPass(i, j, backupPadded, paddedImage, H, padder.paddingWidth, padder.paddingHeight, selector, arithmeticOperator);
                    }
                }
            }

            overwriteImage(backupImage, paddedImage, padder);

            return backupImage;
        }

        private static void applyMorphologicalFilterPass(int i, int j, float[,] backupPadded, byte[,] paddedImage, float[,] filter, int filterWidth, int filterHeight, Func<List<float>, float> selector, Func<float, float, float> arithmeticOperator)
        {
            List<float> Values = new List<float>();

            for (int k = -filterWidth; k <= filterWidth; k++)
            {
                for (int l = -filterHeight; l <= filterHeight; l++)
                {
                    if (filter[k + filterWidth, l + filterHeight] < 0)
                        continue; //skip, negative values are the unused values

                    float newVal = arithmeticOperator(backupPadded[i + k, j + l], filter[k + filterWidth, l + filterHeight]);

                    if (newVal < 0) newVal = 0;
                    else if (newVal > 255) newVal = 255; 

                    Values.Add(newVal);
                }
            }

            paddedImage[i, j] = (byte)selector(Values);
        }

        public static byte[,] applyNonLinearFilter(byte[,] inputImage, Padder padder, Func<int, int, byte[,], float> f)
        { 
            byte[,] paddedImage = padder.padImage(inputImage);
            float[,] backupImage = copyImage(inputImage);

            int paddingWidth = padder.paddingWidth, paddingHeight = padder.paddingHeight;

            for (int i = paddingWidth; i < backupImage.GetLength(0) + paddingWidth; i++)
            for (int j = paddingHeight; j < backupImage.GetLength(1) + paddingHeight; j++)
            {
                backupImage[i - paddingWidth, j - paddingHeight] = f(i, j, paddedImage);
            }

            return convertToBytes(backupImage);
        }
        
        public static byte[,] convertToGrayscale(Color[,] inputImage, ProgressBar progressBar = null)
        {
            // create temporary grayscale image of the same size as input, with a single channel
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // process all pixels in the image
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
            for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
            {
                Color pixelColor = inputImage[x, y];                    // get pixel color
                byte average = (byte)((pixelColor.R + pixelColor.B + pixelColor.G) / 3); // calculate average over the three channels
                tempImage[x, y] = average;                              // set the new pixel color at coordinate (x,y)
                if (progressBar != null) progressBar.PerformStep();                              // increment progress bar
            }

            return tempImage;
        }

        public static Color[,] convertBitmapToColor(Bitmap InputImage)
        {
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
            for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                Image[x, y] = InputImage.GetPixel(x, y);                // set pixel color in array at (x,y)

            return Image;
        }

        public static Bitmap convertToImage(byte[,] workingImage)
        {
            Bitmap OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image
            for (int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
            for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
            {
                Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
            }

            return OutputImage;
        }

        public static int[] calculateCumulativeHistogram(int[] histogram)
        {
            int[] cumulativeHistogram = new int[histogram.Length];
            cumulativeHistogram[0] = histogram[0];
            
            for (int i = 1; i < histogram.Length; i++)
                cumulativeHistogram[i] = cumulativeHistogram[i - 1] + histogram[i];

            return cumulativeHistogram;
        }
    }
}
