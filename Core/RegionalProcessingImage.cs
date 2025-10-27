using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core
{
    public class RegionalProcessingImage : ProcessingImage
    {
        public int amountOfRegions => regions.Count;
        private readonly int[,] regionGrid;
        public Dictionary<int, List<Vector2>> regions { get; private set; }

        public RegionalProcessingImage(byte[,] inputImage, ImageRegionFinder regionFinder) : base(inputImage)
        {
            if (!getImageData().isBinary())
                throw new Exception("Regional Processing Images must be binary");
            
            regionGrid = regionFinder.findRegions(inputImage);
            regions = new Dictionary<int, List<Vector2>>();
            getRegionsFromGrid();
        }

        private void getRegionsFromGrid()
        {
            for (int x = 0; x < regionGrid.GetLength(0); x++)
            for (int y = 0; y < regionGrid.GetLength(1); y++)
            {
                if (regionGrid[x, y] == 0) continue;
                if (!regions.ContainsKey(regionGrid[x, y])) regions.Add(regionGrid[x, y], new List<Vector2>() {new Vector2(x, y)});
                else regions[regionGrid[x, y]].Add(new Vector2(x, y));
            }
        }
        
        public ProcessingImage displayLargestRegion()
        {
            byte[,] outputImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            int largestRegion = regions.Select(region => new KeyValuePair<int, int>(region.Key, region.Value.Count))
                .Aggregate((r1, r2) => r1.Value > r2.Value ? r1 : r2).Key;
            
            for (int x = 0; x < regionGrid.GetLength(0); x++)
            for (int y = 0; y < regionGrid.GetLength(1); y++) 
                if (regionGrid[x, y] == largestRegion) outputImage[x, y] = 255;
            
            return new ProcessingImage(outputImage);
        }
        
        public ProcessingImage highlightRegions()
        {
            byte[,] outputImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            for (int x = 0; x < regionGrid.GetLength(0); x++)
            for (int y = 0; y < regionGrid.GetLength(1); y++)
            {
                if (regionGrid[x, y] == 0) continue;
                if (regionGrid[x, y] % 8 == 0) outputImage[x, y] = 64;
                else outputImage[x, y] = (byte)((regionGrid[x, y] % 8) * 32);
            }

            return new ProcessingImage(outputImage);
        }
        public List<Vector2> getThetaRPairs()
        {
            return regionCenters()
                .Select(p =>
                    HelperFunctions.coordinateToThetaRPair(p.Value,
                        width, height))
                .ToList();
        }

        public Dictionary<int, Vector2> regionCenters()
        {
            // TODO: maybe this is not the best way of finding the centers of the regions
            return regions
                .Select(region => 
                    new KeyValuePair<int, Vector2>(region.Key, region.Value[region.Value.Count / 2]))
                .ToDictionary(p => p.Key, p => p.Value);
        }
    }
}