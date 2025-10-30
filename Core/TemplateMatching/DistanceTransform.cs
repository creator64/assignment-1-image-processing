using System;
using System.Collections.Generic;
using System.Linq;
using INFOIBV.Core.Main;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core.TemplateMatching
{
    public abstract class AbstractDistanceTransform : ProcessingImage
    {
        protected AbstractDistanceTransform(byte[,] inputImage) : base(inputImage)
        {
            if (!getImageData().isBinary())
                throw new Exception("Cannot do a distance transformation on a non-binary image");
        }
        public abstract float[,] toDistances(DistanceStyle distanceStyle);

        // for visualization purposes
        public ProcessingImage toDistancesImage(DistanceStyle distanceStyle)
        {
            float maxDistance = 0;
            float[,] distances = toDistances(distanceStyle);
            
            for (int i = 0; i < distances.GetLength(0); i++)
            for (int j = 0; j < distances.GetLength(1); j++)
                if (distances[i, j] > maxDistance) maxDistance = distances[i, j];

            for (int i = 0; i < distances.GetLength(0); i++)
            for (int j = 0; j < distances.GetLength(1); j++)
                distances[i, j] = Math.Max(0, (float)Math.Log(distances[i, j], maxDistance) * 255);

            return new ProcessingImage(ConverterMethods.convertToBytes(distances)).invertImage();
        }
    }
    
    public class ChamferDistanceTransform : AbstractDistanceTransform
    {
        private readonly int matrixSize;

        public ChamferDistanceTransform(byte[,] inputImage, int matrixSize) : base(inputImage)
        {
            this.matrixSize = matrixSize;
        }
        
        public override float[,] toDistances(DistanceStyle distanceStyle)
        {
            float[,] distances = new float[width,height];
            float maxDistance = distanceStyle.maxDistance(width, height);
            for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                if (inputImage[i, j] == 255) distances[i, j] = 0;
                else distances[i, j] = maxDistance;
            }

            float[,] distanceMatrix = distanceStyle.squareDistanceMatrix(matrixSize);
            int center = matrixSize / 2;

            void traverseDistances(string direction)
            {
                Func<int, int, bool> toTraverse;
                int[] Is = Enumerable.Range(0, width).ToArray(), Js = Enumerable.Range(0, height).ToArray();

                if (direction == "left")
                    toTraverse = (k, l) => (l < center) || (l == center && k <= center);
                else if (direction == "right")
                {
                    toTraverse = (k, l) => (l > center) || (l == center && k >= center);
                    Is = Is.Reverse().ToArray(); Js = Js.Reverse().ToArray();
                }
                else throw new Exception("invalid traverse argument");
                
                
                foreach (int j in Js)
                foreach (int i in Is)
                {
                    List<float> distancesForPixel = new List<float>();
                    for (int k = 0; k < distanceMatrix.GetLength(0); k++)
                    for (int l = 0; l < distanceMatrix.GetLength(1); l++)
                    {
                        if (!toTraverse(k, l)) continue;

                        int x = i + k - center, y = j + l - center;
                        if (outOfBounds(x, y)) continue;

                        distancesForPixel.Add(distances[x, y] + distanceMatrix[k, l]);
                    }

                    distances[i, j] = distancesForPixel.Min();
                }
            };

            // top-left -> bottom-right
            traverseDistances("left");
            // bottom-right -> top-left
            traverseDistances("right");

            return distances;
        }
    }
    
    
    
    public class DistanceTransform : AbstractDistanceTransform
    {
        public DistanceTransform(byte[,] inputImage) : base(inputImage) {}
        
        public override float[,] toDistances(DistanceStyle distanceStyle)
        {
            throw new System.NotImplementedException();
        }
    }
}