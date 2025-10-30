using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core.Main
{
    public partial class ProcessingImage
    {
        protected bool outOfBounds(int x, int y)
        {
            return x < 0 || x >= width || y < 0 || y >= height;
        }
        
        public Dictionary<SubImage, float> findMatchesBinary(ProcessingImage templateImage, List<SubImage> subImages, int threshold = -1)
        {
            DistanceStyle ds = new ManhattanDistance();
            float[,] distances = new ChamferDistanceTransform(inputImage, 3).toDistances(ds);
            Dictionary<SubImage, float> scores = new Dictionary<SubImage, float>();

            foreach (SubImage subImage in subImages)
            {
                (int r, int s) = subImage.startPos;
                float score = calculateScore(r, s, templateImage, distances);
                if (threshold <= 0 || score < threshold) scores.Add(subImage, score);
            }

            return scores;
        }
        
        public Dictionary<Point, float> findMatchesBinaryAllPixels(ProcessingImage templateImage, int threshold = -1)
        {
            DistanceStyle ds = new ManhattanDistance();
            float[,] distances = new ChamferDistanceTransform(inputImage, 3).toDistances(ds);
            Dictionary<Point, float> scores = new Dictionary<Point, float>();
            
            // TODO: deal with check edges

            for (int r = 0; r < width; r++)
            for (int s = 0; s < height; s++)
            {
                if (r + templateImage.width >= width || s + templateImage.height > height) continue;
                float score = calculateScore(r, s, templateImage, distances);
                if (threshold <= 0 || score < threshold) scores.Add(new Point(r, s), score);
            }

            return scores;
        }

        public float calculateScore(int r, int s, ProcessingImage templateImage, float[,] distances)
        {
            int K = templateImage.getImageData().amountForegroundPixels;
            float score = 0;
            for (int k = 0; k < templateImage.width; k++)
            for (int l = 0; l < templateImage.height; l++)
            {
                if (templateImage.inputImage[k, l] != 255) continue;
                int x = r + k, y = s + l; if (outOfBounds(x, y)) continue;
                score += distances[x, y];
            }

            score /= K;

            return score;
        }

        public Point findBestMatchBinary(ProcessingImage templateImage)
        {
            Dictionary<Point, float> scores = findMatchesBinaryAllPixels(templateImage);
            return scores.Aggregate(
                    (s1, s2) => s1.Value < s2.Value ? s1 : s2)
                .Key;
        }
        
        public RegionalProcessingImage toRegionalImage(ImageRegionFinder regionFinder)
        {
            return new RegionalProcessingImage(inputImage, regionFinder);
        }

        public HoughTransform toHoughTransform(int thetaDetail, int rDetail)
        {
            return new HoughTransform(inputImage, thetaDetail, rDetail);
        }
        
        public SubImage createSubImage((int X, int Y) startPos, (int X, int Y) endPos)
        {
            return SubImage.create(this, startPos, endPos);
        }

        public Bitmap getImage()
        {
            Bitmap OutputImage = new Bitmap(inputImage.GetLength(0), inputImage.GetLength(1)); // create new output image
            for (int x = 0; x < inputImage.GetLength(0); x++)             // loop over columns
            for (int y = 0; y < inputImage.GetLength(1); y++)         // loop over rows
            {
                Color newColor = Color.FromArgb(inputImage[x, y], inputImage[x, y], inputImage[x, y]);
                OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
            }

            return OutputImage;
        }

        public ImageData getImageData()
        {
            return new ImageData(inputImage);
        }
        
        public byte[,] toArray()
        {
            return inputImage;
        }
    }
}