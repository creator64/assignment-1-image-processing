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


        public int amountDistinctValues { get; private set; }
        public int amountForegroundPixels { get { return histogram[255]; } }
        public int amountBackgroundPixels { get { return histogram[0]; } }

        public ImageData(byte[,] I)
        {
            histogram = createHistogram(I);
            averageIntensity = getAverageIntensityValue(I, histogram);
            amountDistinctValues = countDistinctValues(histogram);
        }
        /// <summary>
        /// Function that counts the amount of distinct grayscale values from
        /// the histogram of an image.
        /// </summary>
        /// <param name="histogram"></param>
        /// <returns></returns>
        private int countDistinctValues(int[] histogram)
        {
            int distinctValues = 0;
            for(int i = 0; i < histogram.Length; i++)
                if (histogram[i] > 0) distinctValues++;

            return distinctValues;
        }
        /// <summary>
        /// Calculates the chance that a pixel falls within the given intensity interval
        /// </summary>
        /// <param name="t_low">Inclusive lower boundary of the grayscale intensity value interval</param>
        /// <param name="t_high">Inclusive upper boundary of the grayscale intensity value interval</param>
        /// <returns></returns>
        public float getProbability(byte t_low, byte t_high)
        {
            float total = 0.0f;
            float P = 0.0f;

            for (int i = t_low; i <= t_high; i++)
                total += histogram[i];

            if (total == 0.0f) return P;

            for (int i = t_low; i < t_high; i++)
                P += histogram[i] / total;

            return P;
        }

        /// <summary>
        /// Calculates the mean of an interval from the histogram specified by an upper and lower boundary
        /// </summary>
        /// <param name="t_low">lower boundary of the intensity value range (inclusive)</param>
        /// <param name="t_high">upper boundary of the intensity value range (inclusive)</param>
        /// <returns></returns>
        public float calculateMean(byte t_low = 0, byte t_high = 255)
        {
            float sum = 0.0f;
            float N = 0.0f;
            for (int g = t_low; g <= t_high; g++)
            {
                N += histogram[g];
                sum += g * histogram[g];
            }

            return N > 0 ? (1 / N) * sum : -1;
        }
        /// <summary>
        /// Calculates the variance of the grayscale values over the given interval of the histogram
        /// </summary>
        /// <param name="t_low">Inclusive lower boundary of the intensity value range</param>
        /// <param name="t_high">Inclusive upper boundary of the intensity value range</param>
        /// <returns></returns>
        public float calculateVariance(byte t_low = 0, byte t_high = 255)
        {
            float mu = calculateMean(t_low, t_high);

            float sum = 0.0f;
            float N = 0.0f;
            
            for (int g = t_low; g <= t_high; g++)
            {
                N += histogram[g];
                sum += ((g - mu) * (g - mu)) * histogram[g];
            }


            return N > 0 ? (1 / N) * sum : -1;
        }
        /// <summary>
        /// Calculates the number of pixels that exists in a given interval
        /// </summary>
        /// <param name="t_low">Inclusive lower grayscale intensity value boundary</param>
        /// <param name="t_high">Inclusive upper grayscale intensity value boundary</param>
        /// <returns></returns>
        public int getNumPixels(byte t_low = 0, byte t_high = 255)
        {
            int result = 0;
            for (int i = t_low; i <= t_high; i++)
                result += histogram[i];

            return result;
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

        public bool isBinary()
        {
            return amountDistinctValues <= 2 && histogram.Skip(1).Take(254).All(iv => iv == 0);
        }
    }
}
