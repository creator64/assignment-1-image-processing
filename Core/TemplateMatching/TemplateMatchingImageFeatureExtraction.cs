using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using INFOIBV.Core.Main;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core.TemplateMatching
{
    public partial class TemplateMatchingImage
    {
        private static List<CuneiATemplate> _templates = new List<CuneiATemplate>
        {
            new all(),
            new p(),
            new pp(),
            new ppp(),
            new q(),
            new qp_I(),
            new qp_II(),
            new qp_III(),
            new qpp_I(),
            new qpp_II(),
            new qpp_III(),
        };
        
        public RGBImage cuneiADetection(List<SubImage> pointsToCheck = null)
        {
            return drawRectangles(
                filterSegments(pointsToCheck)
                    .Select(s => s.toRectangle())
                    .ToList()
            );
        }
        
        private List<SubImage> filterSegments(List<SubImage> segments)
        {
            List<SubImage> detectedSegments = new List<SubImage>();
            foreach (SubImage subImage in segments)
            {
                SubImage unpadded = subImage.removePadding();
                if (validateSegment(unpadded)) detectedSegments.Add(unpadded);
            }

            return detectedSegments;
        }

        private bool validateSegment(SubImage segment)
        {
            Dictionary<Point, LetterPart> letters = extractLetters(segment);
            int amountP = 0, amountQ = 0;
            foreach (LetterPart letterPart in letters.Values)
                if (letterPart == LetterPart.P) amountP++; else amountQ++;

            if (amountP != 3 || amountQ != 1) return false;

            return true;
        }

        private Dictionary<Point, LetterPart> extractLetters(SubImage segment)
        {
            int regionThreshold = 40;
            double scoreThreshold = 50;
            List<Region> regions = segment.toRegionalImage(new FloodFill()).regions
                .Where(r => r.Size > regionThreshold).ToList();

            float[,] distances = getDistanceTransform().toDistances(new ManhattanDistance());
            Dictionary<Region, CuneiATemplate> regionsToTemplate = new Dictionary<Region, CuneiATemplate>();

            foreach (Region region in regions)
            {
                (float bestScore, CuneiATemplate chosenTemplate) = (int.MaxValue, null);
                int r = region.minX, s = region.minY;
                
                foreach (CuneiATemplate template in _templates)
                {
                    float score = calculateScore(
                        r, s, 
                        template.templateImage.resize(region.width, region.height), 
                        distances
                    );
                    if (score < bestScore) 
                        (bestScore, chosenTemplate) = (score, template);
                }
                
                if (bestScore < scoreThreshold) regionsToTemplate.Add(region, chosenTemplate);
            }

            return regionsToTemplate
                .Select(pair => pair.Value.extractLetters(pair.Key))
                .Aggregate(new Dictionary<Point, LetterPart>(), (a, b) => a
                    .Concat(b)
                    .ToDictionary(x=>x.Key,x=>x.Value)
                );
        }
    }
}