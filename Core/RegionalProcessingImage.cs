using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using INFOIBV.Core.Main;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core
{
    public class Region
    {
        public int id;
        public List<Vector2> coordinates;
        public int Size => coordinates.Count;
        public int minX => (int)coordinates.Min(v => v.X);
        public int minY => (int)coordinates.Min(v => v.Y);
        public int maxX => (int)coordinates.Max(v => v.X);
        public int maxY => (int)coordinates.Max(v => v.Y);
        public int width => maxX - minX;
        public int height => maxY - minY;

        public Region(int id, List<Vector2> coordinates)
        {
            this.id = id;
            this.coordinates = coordinates;
        }

        public void addCoordinate(Vector2 vector2)
        {
            coordinates.Add(vector2);
        }
    }
    public class RegionalProcessingImage : ProcessingImage
    {
        public int amountOfRegions => regions.Count;
        private readonly int[,] regionGrid;
        public List<Region> regions { get; private set; }

        public RegionalProcessingImage(byte[,] inputImage, ImageRegionFinder regionFinder) : base(inputImage)
        {
            if (!getImageData().isBinary())
                throw new Exception("Regional Processing Images must be binary");
            
            regionGrid = regionFinder.findRegions(inputImage);
            regions = new List<Region>();
            getRegionsFromGrid();
        }

        private Region getRegion(int id)
        {
            return regions.Find(r => r.id == id);
        }

        private void getRegionsFromGrid()
        {
            for (int x = 0; x < regionGrid.GetLength(0); x++)
            for (int y = 0; y < regionGrid.GetLength(1); y++)
            {
                if (regionGrid[x, y] == 0) continue;
                if (getRegion(regionGrid[x, y]) == null)
                    regions.Add(new Region(regionGrid[x, y], new List<Vector2>{new Vector2(x, y)}));
                
                else getRegion(regionGrid[x, y]).addCoordinate(new Vector2(x, y));
            }
        }
        
        public ProcessingImage displayLargestRegion()
        {
            byte[,] outputImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            int largestRegion = regions
                .Aggregate((r1, r2) => r1.Size > r2.Size ? r1 : r2).id;
            
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
                    new KeyValuePair<int, Vector2>(region.id, region.coordinates[region.Size / 2]))
                .ToDictionary(p => p.Key, p => p.Value);
        }
    }
}