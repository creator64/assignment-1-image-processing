using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core.Main
{
    public class SubImage : ProcessingImage
    {
        public ProcessingImage parentImage { get; private set; }

        public (int X, int Y) startPos { get; private set; }

        public (int X, int Y) endPos { get; private set; }
        public int subWidth => endPos.X - startPos.X;
        public int subHeight => endPos.Y - startPos.Y;

        public HashSet<Vector2> foregroundPixels { get; private set; }

        public int totalAmountOfPixels { get; private set; }
        public float foregroundRatio { get; private set; }
        public int amountForegroundPixels => foregroundPixels.Count;

        /// <summary>
        /// The width as a fraction of the height
        /// </summary>
        public float widthHeightRatio => (float)this.width / this.height;


        public static SubImage create(ProcessingImage processingImage, (int X, int Y) startPos, (int X, int Y) endPos)
        {
            if (startPos.X == endPos.X) endPos.X++;
            if (startPos.Y == endPos.Y) endPos.Y++;
            byte[,] subImageArray = new byte[endPos.X - startPos.X, endPos.Y - startPos.Y];
            byte[,] inputImage = processingImage.toArray();

            for (int u = startPos.X; u <= endPos.X; u++)
            for (int v = startPos.Y; v <= endPos.Y; v++)
            {
                if ((u - startPos.X < 0 || u - startPos.X >= subImageArray.GetLength(0))
                    || (v - startPos.Y < 0 || v - startPos.Y >= subImageArray.GetLength(1))
                    || (u < 0 || u >= processingImage.width)
                    || (v < 0 || v >= processingImage.height)) // out of array bounds check
                    continue;

                subImageArray[u - startPos.X, v - startPos.Y] = inputImage[u, v];
            }

            return new SubImage(processingImage, subImageArray, startPos, endPos);
        }

        public SubImage(ProcessingImage parentImage, byte[,] subImageArray, (int X, int Y) startPos,
            (int X, int Y) endPos) : base(subImageArray)
        {
            this.parentImage = parentImage;
            this.startPos = startPos;
            this.endPos = endPos;
            this.foregroundPixels = new HashSet<Vector2>();
            this.totalAmountOfPixels = this.width * this.height;

            for (int i = 0; i < subImageArray.GetLength(0); i++)
            {
                for (int j = 0; j < subImageArray.GetLength(1); j++)
                {
                    if (subImageArray[i, j] == 255) foregroundPixels.Add(new Vector2(i, j));
                }
            }

            this.foregroundRatio = (float)this.amountForegroundPixels / this.totalAmountOfPixels;
        }

        public SubImage removePadding(double thresholdRate = 1 / 50d)
        {
            if (!getImageData().isBinary())
                throw new Exception("Cannot remove padding from a non-binary subImage");

            List<Region> regions = toRegionalImage(new FloodFill()).regions;
            List<Vector2> foregroundPixels = regions
                .Where(r => r.Size > thresholdRate * subWidth * subHeight) // remove noise regions
                .Aggregate(new List<Vector2>(), (list, region) => list.Concat(region.coordinates).ToList());

            if (foregroundPixels.Count == 0) return this;

            int minX = foregroundPixels.Min(v => (int)v.X) + startPos.X;
            int minY = foregroundPixels.Min(v => (int)v.Y) + startPos.Y;
            int maxX = foregroundPixels.Max(v => (int)v.X) + startPos.X;
            int maxY = foregroundPixels.Max(v => (int)v.Y) + startPos.Y;

            return SubImage.create(this.parentImage, (minX, minY), (maxX, maxY));
        }

        public Rectangle toRectangle()
        {
            return new Rectangle(startPos.X, startPos.Y, subWidth, subHeight);
        }

        public List<Vector2> getLargestRegion()
        {
            List<Vector2> maxReg = this.toRegionalImage(new FloodFill())
                .regions.Select(r => r.coordinates)
                .OrderBy((List<Vector2> a) => a.Count)
                .ToList().First(); //take the largest region

            return maxReg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The left most foreground pixel</returns>
        public Vector2 getLeftMostPixel(IEnumerable<Vector2> region)
        {
            Vector2 leftPixel = new Vector2(int.MaxValue, 0);
            foreach (Vector2 p in region)
            {
                if (p.X < leftPixel.X) leftPixel = p;
            }

            return leftPixel;
        }

        public Vector2 getRightMostPixel(IEnumerable<Vector2> region)
        {
            Vector2 rightPixel = new Vector2(int.MinValue, 0);
            foreach (Vector2 p in region)
            {
                if (p.X > rightPixel.X) rightPixel = p;
            }

            return rightPixel;
        }

        public override bool Equals(object obj)
        {
            if (obj is SubImage)
            {
                SubImage sub = (SubImage)obj;

                return sub.startPos == this.startPos && sub.endPos == this.endPos &&
                       sub.parentImage == this.parentImage;
            }
            else
                return false;
        }
    }
}