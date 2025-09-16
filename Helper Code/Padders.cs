using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.Helper_Code
{
    public abstract class Padder
    {
        protected byte paddingWidth;

        protected Padder(byte paddingWidth)
        {
            this.paddingWidth = paddingWidth;
        }

        public abstract byte[,] padImage(byte[,] image);

    }

    public class ConstantValuePadder : Padder
    {
        private byte paddingValue;
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

    public class CopyPerimeterPadder : Padder
    {
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