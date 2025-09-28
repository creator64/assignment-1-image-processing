using System.Collections.Generic;
using System.Numerics;

namespace INFOIBV.Helper_Code
{
    public static class ImageRegions
    {
        public static List<List<Vector2>> findRegions(byte[,] inputImage)
        {
            int width = inputImage.GetLength(0), height = inputImage.GetLength(1);
            bool[,] covered = new bool[width, height];
            List<List<Vector2>> regions = new List<List<Vector2>>();
            
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (inputImage[x, y] == 0) continue;
                if (covered[x, y]) continue;

                List<Vector2> regionPoints = new List<Vector2>();
                Queue<Vector2> queue = new Queue<Vector2>();
                queue.Enqueue(new Vector2(x, y));
                
                while (queue.Count != 0)
                {
                    Vector2 point = queue.Dequeue();
                    int px = (int)point.X, py = (int)point.Y;
                    
                    if (covered[px, py]) continue;
                    regionPoints.Add(point);
                    covered[px, py] = true;

                    foreach (Vector2 neighbour in HelperFunctions.FourNeighbours(point))
                    {
                        int neighbourX = (int)neighbour.X, neighbourY = (int)neighbour.Y;
                        if (neighbourX >= width || neighbourX < 0 || neighbourY >= height || neighbourY < 0) continue;
                        if (covered[neighbourX, neighbourY]) continue;
                        if (inputImage[neighbourX, neighbourY] == 255) queue.Enqueue(neighbour);
                    }
                }
                
                regions.Add(regionPoints);
            }
                
            return regions;
        }
    }
}