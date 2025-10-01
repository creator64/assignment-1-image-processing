using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace INFOIBV.Helper_Code
{
    public abstract class ImageRegionFinder
    {
        /// <summary>
        /// takes a binary 2D image and returns the same image, where the regions of foreground pixels are identified
        /// with different integer labels,
        /// for example <br />
        /// [0,0,1,1,0,0,0] <br />             
        /// [0,0,1,1,0,1,0] <br />
        /// [1,0,0,0,0,1,0] <br />
        /// [1,0,0,0,0,1,0] <br />
        /// -> <br />
        /// [0,0,2,2,0,0,0] <br />
        /// [0,0,2,2,0,1,0] <br />
        /// [3,0,0,0,0,4,0] <br />
        /// [3,0,0,0,0,4,0] <br />
        /// </summary>
        public abstract int[,] findRegions(byte[,] inputImage);
    }

    public class FloodFill : ImageRegionFinder
    {
        public override int[,] findRegions(byte[,] inputImage)
        {
            int width = inputImage.GetLength(0), height = inputImage.GetLength(1);
            int[,] regions = new int[width, height];

            int regionId = 1;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (inputImage[x, y] == 0) continue;
                if (regions[x, y] != 0) continue; // means it is alr labeled

                Queue<Vector2> queue = new Queue<Vector2>();
                queue.Enqueue(new Vector2(x, y));
                regionId++;
                
                while (queue.Count != 0)
                {
                    Vector2 point = queue.Dequeue();
                    int px = (int)point.X, py = (int)point.Y;
                    
                    if (regions[px, py] != 0) continue;
                    regions[px, py] = regionId;

                    foreach (Vector2 neighbour in HelperFunctions.FourNeighbours(point))
                    {
                        int neighbourX = (int)neighbour.X, neighbourY = (int)neighbour.Y;
                        if (neighbourX >= width || neighbourX < 0 || neighbourY >= height || neighbourY < 0) continue;
                        if (regions[neighbourX, neighbourY] != 0) continue;
                        if (inputImage[neighbourX, neighbourY] == 255) queue.Enqueue(neighbour);
                    }
                }
            }
                
            return regions;
        }
    }

    public class SequentialLabeling : ImageRegionFinder
    {
        public override int[,] findRegions(byte[,] inputImage)
        {
            int width = inputImage.GetLength(0), height = inputImage.GetLength(1);
            int[,] regions = new int[width, height];

            for (int i = 0; i < regions.GetLength(0); i++) for (int j = 0; j < regions.GetLength(1); j++)
                regions[i, j] = inputImage[i, j];

            int currentRegionId = 2;
            Dictionary<int, HashSet<int>> collidingRegions = new Dictionary<int, HashSet<int>>();

            for (int j = 0; j < regions.GetLength(1); j++)
            for (int i = 0; i < regions.GetLength(0); i++)
            {
                if (regions[i, j] == 0) continue;
                int[] neighbouringRegionIds = HelperFunctions.EightNeighbours(new Vector2(i, j))
                    .Take(4)
                    .Where(v => v.X >= 0 && v.X < width && v.Y >= 0 && v.Y < height)
                    .Select(v => regions[(int)v.X, (int)v.Y])
                    .Distinct()
                    .Where(r => r != 0 && r != 255)
                    .ToArray();

                if (neighbouringRegionIds.Length == 0)
                {
                    regions[i, j] = currentRegionId;
                    collidingRegions.Add(currentRegionId++, new HashSet<int>());
                }
                else
                {
                    regions[i, j] = neighbouringRegionIds[0];
                    foreach (int neighbouringRegionId in neighbouringRegionIds.Skip(1))
                        collidingRegions[neighbouringRegionIds[0]].Add(neighbouringRegionId);
                }
            }

            void convert(int regionId, int targetRegionId)
            {
                for (int i = 0; i < regions.GetLength(0); i++)
                for (int j = 0; j < regions.GetLength(1); j++)
                    if (regions[i, j] == regionId)
                        regions[i, j] = targetRegionId;
                
            };

            void mergeRegions(int regionId, List<int> prev)
            {
                if (!collidingRegions.ContainsKey(regionId)) return;
                foreach (int colldingRegionId in collidingRegions[regionId])
                {
                    if (prev.Contains(colldingRegionId)) continue;
                    mergeRegions(colldingRegionId, new List<int>(prev) { colldingRegionId });
                    convert(colldingRegionId, regionId);
                    collidingRegions.Remove(colldingRegionId);
                }
            };

            while (collidingRegions.Count > 0)
            {
                int regionId = collidingRegions.Keys.First();
                mergeRegions(regionId, new List<int>(){regionId});
                collidingRegions.Remove(regionId);
            }


            return regions;
        }
    }
}