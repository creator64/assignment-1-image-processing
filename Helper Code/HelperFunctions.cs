using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static byte[,] applyUnevenFilter(byte[,] I, byte[,] H)
        {

            return I;
        }
    }
}
