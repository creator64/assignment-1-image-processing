using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using INFOIBV.Core.Main;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core.TemplateMatching
{
    public enum DistanceTransformType
    {
        ChamferMatching,
        RegularDistanceTransform
    }
    
    public partial class TemplateMatchingImage : ProcessingImage
    {
        private DistanceTransformType distanceTransformType = DistanceTransformType.ChamferMatching;
        public TemplateMatchingImage(byte[,] inputImage) : base(inputImage) {}

        public TemplateMatchingImage(byte[,] inputImage, DistanceTransformType distanceTransformType) : base(inputImage)
        {
            this.distanceTransformType = distanceTransformType;
        }

        private AbstractDistanceTransform getDistanceTransform()
        {
            switch (distanceTransformType)
            {
                case DistanceTransformType.ChamferMatching: return new ChamferDistanceTransform(inputImage, 3);
                case DistanceTransformType.RegularDistanceTransform: return new DistanceTransform(inputImage);
            }

            return null;
        }

        public Dictionary<Point, float> findMatchesBinaryAllPixels(TemplateMatchingImage templateImage, int threshold = -1)
        {
            DistanceStyle ds = new ManhattanDistance();
            float[,] distances = new ChamferDistanceTransform(inputImage, 3).toDistances(ds);
            float K = templateImage.getImageData().amountForegroundPixels;
            Dictionary<Point, float> scores = new Dictionary<Point, float>();

            for (int r = 0; r < width; r++)
            for (int s = 0; s < height; s++)
            {
                if (r + templateImage.width >= width || s + templateImage.height > height) continue;
                float score = calculateScore(r, s, templateImage, distances, K);
                if (threshold <= 0 || score < threshold) scores.Add(new Point(r, s), score);
            }

            return scores;
        }
        
        public Dictionary<SubImage, float> findMatchesBinary(TemplateMatchingImage templateImage, List<SubImage> subImages, double threshold = -1)
        {
            DistanceStyle ds = new ManhattanDistance();
            float[,] distances = getDistanceTransform().toDistances(ds);
            Dictionary<SubImage, float> scores = new Dictionary<SubImage, float>();

            foreach (SubImage subImage in subImages)
            {
                SubImage unpaddedSubImage = subImage.removePadding();
                TemplateMatchingImage optimizedTemplateImage = templateImage.adaptTo(unpaddedSubImage);
                (int r, int s) = unpaddedSubImage.startPos;
                
                float score = calculateScore(r, s, optimizedTemplateImage, distances);
                if (score < threshold) scores.Add(unpaddedSubImage, score);
            }

            return scores;
        }

        public float calculateScore(int r, int s, TemplateMatchingImage templateImage, float[,] distances, float K = -1)
        {
            float score = 0;
            for (int k = 0; k < templateImage.width; k++)
            for (int l = 0; l < templateImage.height; l++)
            {
                if (templateImage.inputImage[k, l] != 255) continue;
                int x = r + k, y = s + l; if (outOfBounds(x, y)) continue;
                score += distances[x, y];
            }

            K = K <= 0 ? templateImage.getImageData().amountForegroundPixels : K;
            score /= K;

            return score;
        }

        public Point findBestMatchBinary(TemplateMatchingImage templateImage)
        {
            Dictionary<Point, float> scores = findMatchesBinaryAllPixels(templateImage);
            return scores.Aggregate(
                    (s1, s2) => s1.Value < s2.Value ? s1 : s2)
                .Key;
        }
        
        public RGBImage visualiseMatchesBinary(TemplateMatchingImage templateImage, double threshold = -1, List<SubImage> pointsToCheck = null)
        {
            return drawRectangles(
                findMatchesBinary(templateImage, pointsToCheck, threshold).Keys
                    .Select(s => s.toRectangle())
                    .ToList()
            );
        }

        public RGBImage visualiseBestMatchBinary(TemplateMatchingImage templateImage)
        {
            Point bestMatch = findBestMatchBinary(templateImage);
            return drawRectangles(new List<Rectangle> { new Rectangle(bestMatch.X, bestMatch.Y, templateImage.width, templateImage.height) });
        }

        public TemplateMatchingImage adaptTo(ProcessingImage image)
        {
            return fromBitmap(
                new Bitmap(getImage(), new Size(image.width, image.height))
            ).toTemplateMatchingImage();
        }
    }
}