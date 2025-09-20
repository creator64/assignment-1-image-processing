using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using INFOIBV.Helper_Code;

namespace INFOIBV.Helper_Code
{
    public enum PaddingMethods : byte
    {
        ConstantValue,  // pad the image with a constant value
        CopyPerimeter,  // pad the image with the same value the neighbour on the perimeter has
        MirrorImage,    // pad the image with mirrored copies of the image
        CopyImage       // pad the image with non-mirrored copies of the image
    }

    /// <summary>
    /// A static class with helper functions to cut down
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

        public static float[,] floatifyBoolArray(bool[,] a)
        {
            float[,] result = new float[a.GetLength(0), a.GetLength(1)];

            for(int i = 0; i < a.GetLength(0); i++)
                for(int j = 0; j < a.GetLength(1); j++)
                    result[i, j] = a[i, j] ? 255.0f : 0.0f ;

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
        /// <summary>
        /// A generalised version of a morphological filter pass
        /// </summary>
        /// <param name="I">The image you want to apply the morphological filter to</param>
        /// <param name="H">The structuring element of the morphological filter</param>
        /// <param name="padder">The padder to pad the image I with</param>
        /// <param name="selector">The function with which to select the output of the filterpass, like the Math.Min or Math.Max functions</param>
        /// <param name="threshold">The threshold for a pixel to be considered a foreground element, 255 for binary, 0 for grayscale</param>
        /// <returns></returns>
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
                    Values.Add(arithmeticOperator(backupPadded[i + k, j + l], filter[k + filterWidth, l + filterHeight]));
                }
            }

            paddedImage[i, j] = (byte)selector(Values);
        }
    }
}
