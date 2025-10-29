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
        
        public Dictionary<Point, float> findMatchesBinary(ProcessingImage templateImage, int threshold = 0, bool checkEdges = false, List<Point> pointsToCheck = null)
        {
            DistanceStyle ds = new ManhattanDistance();
            float[,] distances = new ChamferDistanceTransform(inputImage, 3).toDistances(ds);
            int K = templateImage.getImageData().amountForegroundPixels;
            Dictionary<Point, float> scores = new Dictionary<Point, float>();
            
            for (int r = 0; r < width; r++)
            for (int s = 0; s < height; s++)
            {
                if ((width - r < templateImage.width || height - s < templateImage.height) && !checkEdges) continue;
                if (pointsToCheck != null && !pointsToCheck.Contains(new Point(r, s))) continue;
                
                float score = 0;
                for (int k = 0; k < templateImage.width; k++)
                for (int l = 0; l < templateImage.height; l++)
                {
                    if (templateImage.inputImage[k, l] != 255) continue;
                    int x = r + k, y = s + l; if (outOfBounds(x, y)) continue;
                    score += distances[x, y];
                }

                score /= K;

                if (score > threshold) scores.Add(new Point(r, s), score);
            }
            
            return scores;
        }

        public Point findBestMatchBinary(ProcessingImage templateImage)
        {
            Dictionary<Point, float> scores = findMatchesBinary(templateImage, 0);
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