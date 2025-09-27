using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace INFOIBV.Helper_Code
{
    public class ImageData
    {
        public int[] histogram { get; private set; }
        public float averageIntensity { get; private set; }

        public int amountForegroundPixels { get { return histogram[255]; } }
        public int amountBackgroundPixels { get { return histogram[0]; } }

        public ImageData(byte[,] I)
        {
            histogram = createHistogram(I);
            averageIntensity = getAverageIntensityValue(I, histogram);
        }

        /// <param name="I">The image in the form of a 2D byte array</param>
        /// <returns>An array with indices 0 to 255, the value contained in each index is the amount of pixels of that intensity value</returns>
        private int[] createHistogram(byte[,] I)
        {
            int[] histogram = new int[256];

            for (int i = 0; i < I.GetLength(0); i++)
                for (int j = 0; j < I.GetLength(1); j++)
                    histogram[I[i, j]]++;

            return histogram;
        }

        /// <param name="I">The image for which you want to calculate the average intensity value</param>
        /// <returns>The average intensity</returns>
        private float getAverageIntensityValue(byte[,] I, int[] histogram)
        {
            int totalVal = 0;

            for (int i = 0; i < histogram.Length; i++)
                totalVal += (i * histogram[i]);

            return (float)Math.Round((decimal)totalVal / I.LongLength);
        }
    }
}
