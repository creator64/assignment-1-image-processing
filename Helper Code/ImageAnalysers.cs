using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace INFOIBV.Helper_Code
{
    public static class ImageAnalysers
    {
        /// <param name="I">The image in the form of a 2D byte array</param>
        /// <returns>An array with indices 0 to 255, the value contained in each index is the amount of pixels of that intensity value</returns>
        public static int[] createHistogram(byte[,] I)
        {
            int[] histogram = new int[256];

            for(int i = 0; i < I.GetLength(0); i++)
                for(int j = 0; j < I.GetLength(1); j++)
                    histogram[I[i, j]]++;

            return histogram;
        }


        /// <param name="I">The image for which you want to calculate the average intensity value</param>
        /// <returns>The average intensity rounded to its nearest integer value</returns>
        public static int getAverageIntensityValue(byte[,] I)
        {
            int[] histogram = createHistogram(I);

            int totalVal = 0;

            for (int i = 0; i < histogram.Length; i++)
                totalVal += (i * histogram[i]);

            return (int)Math.Round((decimal)totalVal / I.LongLength);
        }
    }
}
