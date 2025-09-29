using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using INFOIBV.Helper_Code;

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
    }
}
