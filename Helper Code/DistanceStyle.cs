using System;
using System.Drawing;

namespace INFOIBV.Helper_Code
{
    public abstract class DistanceStyle
    {
        public float[,] SquareDistanceMatrix3 => squareDistanceMatrix(3);
        public abstract float distanceBetweenPoints(Point p1, Point p2);
        public float[,] squareDistanceMatrix(int size)
        {
            if (size % 2 == 0) throw new Exception(
                    "Cannot create a distance matrix without a center, please enter an odd size parameter"
                );

            float[,] distanceMatrix = new float[size, size];
            int center = size / 2;
            
            for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                distanceMatrix[i, j] = distanceBetweenPoints(new Point(i, j), new Point(center, center));
            }
            
            return distanceMatrix;
        }

        public float maxDistance(int width, int height) =>
            distanceBetweenPoints(new Point(0, 0), new Point(width, height));
    }

    public class ManhattanDistance : DistanceStyle
    {
        public override float distanceBetweenPoints(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }
    }
    
    public class EuclidianDistance : DistanceStyle
    {
        public override float distanceBetweenPoints(Point p1, Point p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}