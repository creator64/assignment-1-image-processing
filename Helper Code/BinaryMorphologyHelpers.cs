using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace INFOIBV.Helper_Code
{
    public static class BinaryMorphologyHelpers
    {
        public static HashSet<Vector2> pointSetFromImage(byte[,] I)
        {
            HashSet<Vector2> result = new HashSet<Vector2>();
            for (int i = 0; i < I.GetLength(0); i++)
                for (int j = 0; j < I.GetLength(1); j++)
                    if (I[i, j] > 0)
                        result.Add(new Vector2(i, j));

            return result;
        }

        public static HashSet<Vector2> pointSetFromFilter(bool[,] H)
        {
            HashSet<Vector2> result = new HashSet<Vector2>();
            int filterWidth = H.GetLength(0) / 2,  filterHeight = H.GetLength(1) / 2;

            for(int i = -filterWidth; i <= filterWidth; i++)
                for(int j = -filterHeight; j <= filterHeight; j++)
                    if (H[i + filterWidth, j + filterHeight])
                        result.Add(new Vector2(i, j));

            return result;
        }

        public static byte[,] pointSetToImage(HashSet<Vector2> pointSet, Vector2 imageDimensions)
        {
            byte[,] result = new byte[(int)imageDimensions.X, (int)imageDimensions.Y];

            foreach(Vector2 point in pointSet)
            {
                if (point.X < 0 || point.X >= imageDimensions.X || point.Y < 0 || point.Y >= imageDimensions.Y)
                    continue; //skip this pixel

                result[(int)point.X, (int)point.Y] = 255;
            }

            return result;
        }
    }
}
