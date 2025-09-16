using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.Helper_Code
{
    /// <summary>
    /// Helper class for padding images.
    /// </summary>
    public abstract class Padder
    {
        public byte paddingWidth { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paddingWidth"> The amount of pixels each of the side is padded with</param>
        protected Padder(byte paddingWidth)
        {
            this.paddingWidth = paddingWidth;
        }

        public abstract byte[,] padImage(byte[,] image);

    }

    /// <summary>
    /// Padder that pads an image with a given constant value
    /// </summary>
    public class ConstantValuePadder : Padder
    {
        private byte paddingValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paddingWidth">The amount of pixels each of the side is padded with</param>
        /// <param name="paddingValue">The value that will be padded with</param>
        public ConstantValuePadder(byte paddingWidth, byte paddingValue) : base(paddingWidth)
        {
            this.paddingValue = paddingValue;
        }

        public override byte[,] padImage(byte[,] image)
        {
            byte[,] newImage = new byte[image.GetLength(0) + (2 * paddingWidth), image.GetLength(1) + (2 * paddingWidth)];

            for (int i = 0; i < newImage.GetLength(0); i++)
            {
                for (int j = 0; j < newImage.GetLength(1); j++)
                {
                    if (i >= paddingWidth && j >= paddingWidth && (i - paddingWidth) < image.GetLength(0) && (j - paddingWidth) < image.GetLength(1))
                        newImage[i, j] = image[i - paddingWidth, j - paddingWidth];
                    else
                        newImage[i, j] = paddingValue;
                }
            }
            return newImage;
        }
    }

    /// <summary>
    /// Padder that pads by copying the value at the border (perimeter)
    /// </summary>
    public class CopyPerimeterPadder : Padder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paddingWidth"> The amount of pixels each of the side is padded with</param>
        public CopyPerimeterPadder(byte paddingWidth) : base(paddingWidth) { }

        public override byte[,] padImage(byte[,] image)
        {
            byte[,] newImage = new byte[image.GetLength(0) + (2 * paddingWidth), image.GetLength(1) + (2 * paddingWidth)];

            for (int i = 0 i < newImage.GetLength(0); i++)
            {
                for (int j = 0; j < newImage.GetLength(1); j++)
                {
                    int x = i;
                    int y = j;

                    if (i < paddingWidth) x = 0;
                    else if (i >= image.GetLength(0)) x = image.GetLength(0) - 1;

                    if (j < paddingWidth) y = 0;
                    else if (j >= image.GetLength(1)) y = image.GetLength(1) - 1;

                    newImage[i, j] = image[x, y];
                }
            }
            return newImage;
        }
    }
}