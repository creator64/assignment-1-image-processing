using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.Helper_Code
{
    public static class FilterGenerators
    {
        /// <summary>
        /// Function that creates an uneven square filter:
        /// - a filter that has an equal size in both the x and y direction
        /// - a filter that has a clear center, hence the size must be uneven.
        /// </summary>
        /// <typeparam name="T">The type (bool, byte, float, etc.) of the elements contained in the filter matrix/typeparam>
        /// <param name="size">The size of of the filter in both the x and y direction</param>
        /// <param name="valueGenerator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T[,] createSquareFilter<T>(int size, Func<int, int, T[,], T> valueGenerator)
        {
            if (size % 2 != 1) throw new ArgumentException($"The size given to createEvenSquareFilter should be uneven, otherwise there will be no clear hotspot. \nCurrent size given is {size}");

            T[,] filter = new T[size, size];

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    filter[i, j] = valueGenerator(i, j, filter);

            return filter;
        }
    }

    public static class FilterValueGenerators
    {
        /// <summary>
        /// A FilterValueGenerator with the purpose of creating structuring elements for binary morphology.
        /// This specific FilterValueGenerator generates only "true" values.
        /// </summary>
        public static bool createUniformBinaryStructElem(int i, int j, bool[,] filter)
        {
            return true;
        }

        /// <summary>
        /// A FilterValueGenerator with the purpose of creating structuring elements for binary morphology.
        /// This specific FilterValueGenerator creates a circle of true values around the hotspot.
        /// </summary>
        public static bool binaryCircle(int i, int j, bool[,] filter)
        {
            int r = filter.GetLength(0);

            int xCoord = i - r;
            int yCoord = j - r;

            float dist = (float)Math.Sqrt((xCoord * xCoord) + (yCoord * yCoord));

            return dist <= r;
        public static int createUniformSquareFilter(int i, int j, int[,] filter)
        {
            return 1;
        }

        public static int createGaussianSquareFilter(int i, int j, int[,] filter)
        {
            int r = filter.GetLength(0);

            int x = i + 1;
            int y = j + 1;
            float sigma = r / 4; //value that works well
            int hotspot = (int)Math.Ceiling((float)r / 2);

            return (int)Math.Round(hotspot * Math.Exp(-((x - hotspot) * (x - hotspot) / (2 * sigma * sigma)) - ((y - hotspot) * (y - hotspot) / (2 * sigma * sigma))));
        }
    }
}
