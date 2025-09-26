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
        /// <param name="size">The size of of the filter in both the x and y direction</param>
        /// <returns></returns>
        public static T[,] createSquareFilter<T>(int size, Func<int, int, T> valueGenerator)
        {
            if (size % 2 != 0) throw new ArgumentException($"The size given to createEvenSquareFilter should be uneven, otherwise there will be no clear hotspot. \nCurrent size given is {size}");

            T[,] filter = new T[size, size];

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    filter[i, j] = valueGenerator(i, j);

            return filter;
        }
    }

    public static class FilterValueGenerators
    {

    }
}
