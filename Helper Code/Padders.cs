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
        public int paddingWidth { get; protected set; }
        public int paddingHeight { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paddingWidth"> The amount of pixels the horizontal sides are padded with</param>
        protected Padder(int paddingWidth, int paddingHeight)
        {
            this.paddingWidth = paddingWidth;
            this.paddingHeight = paddingHeight;
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
        /// <param name="paddingWidth">The amount of pixels each of the horizontal sides is padded with</param>
        /// <param name="paddingValue">The value that will be padded with</param>
        public ConstantValuePadder(int paddingWidth, int paddingHeight, byte paddingValue) : base(paddingWidth, paddingHeight)
        {
            this.paddingValue = paddingValue;
        }

        public override byte[,] padImage(byte[,] image)
        {
            byte[,] newImage = new byte[image.GetLength(0) + (2 * paddingWidth), image.GetLength(1) + (2 * paddingHeight)];

            for (int i = 0; i < newImage.GetLength(0); i++)
            {
                for (int j = 0; j < newImage.GetLength(1); j++)
                {
                    if (i >= paddingWidth && j >= paddingHeight && (i - paddingWidth) < image.GetLength(0) && (j - paddingHeight) < image.GetLength(1))
                        newImage[i, j] = image[i - paddingWidth, j - paddingHeight];
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
        public CopyPerimeterPadder(int paddingWidth, int paddingHeight) : base(paddingWidth, paddingHeight) { }

        public override byte[,] padImage(byte[,] image)
        {
            byte[,] newImage = new byte[image.GetLength(0) + (2 * paddingWidth), image.GetLength(1) + (2 * paddingHeight)];

            for (int i = 0; i < newImage.GetLength(0); i++)
            {
                for (int j = 0; j < newImage.GetLength(1); j++)
                {
                    int x = i;
                    int y = j;

                    if (i < paddingWidth) x = 0;
                    else if (i >= image.GetLength(0)) x = image.GetLength(0) - 1;

                    if (j < paddingHeight) y = 0;
                    else if (j >= image.GetLength(1)) y = image.GetLength(1) - 1;

                    newImage[i, j] = image[x, y];
                }
            }
            return newImage;
        }
    }
}