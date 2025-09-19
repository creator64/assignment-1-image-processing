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
        private static int getFilterWeight(sbyte[,] filter)
        {
            int filterWeight = 0;

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
        public static float[,] applyUnevenFilter(byte[,] I, sbyte[,] H, Padder padder)
        {
            byte[,] paddedImage = padder.padImage(I);

            float[,] backupImage = new float[I.GetLength(0), I.GetLength(1)];

            for(int i = 0; i < backupImage.GetLength(0); i++)
            {
                for(int j = 0; j < backupImage.GetLength(1); j++)
                {
                    backupImage[i, j] = I[i, j];
                }
            }

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
        private static float applyUnevenFilterPass(int i, int j, byte[,] paddedImage, sbyte[,] filter, int filterWidth, int filterHeight, int filterWeight)
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
            {
                return (filteredValue / (float)filterWeight);
            }
            else
            {
                return filteredValue;
            }
        }
    }
}
