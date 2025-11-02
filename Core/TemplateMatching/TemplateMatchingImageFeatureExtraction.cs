using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using INFOIBV.Core.Main;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core.TemplateMatching
{
    public partial class TemplateMatchingImage
    {
        private readonly static List<CuneiATemplate> _templates = new List<CuneiATemplate>
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

        public List<Rectangle> cuneiADetection()
        {
            ProcessingImage binaryEdgeMap = adjustContrast()
                .bilateralSmoothing(2, 50)
                .otsuThreshold()
                .invertImage();

            Segmentator segmentator = new ProjectionSegmentator(binaryEdgeMap);
            List<SubImage> segments = segmentator.segments;

            return filterSegments(segments);
        }
        
        private List<Rectangle> filterSegments(List<SubImage> segments)
        {
            List<Rectangle> detectedSegments = new List<Rectangle>();
            foreach (SubImage subImage in segments)
            {
                SubImage unpadded = subImage.removePadding();
                Rectangle? segmentRectangle = validateSegment(unpadded);
                if (segmentRectangle != null) detectedSegments.Add(segmentRectangle.Value);
            }

            return detectedSegments;
        }

        private Rectangle? validateSegment(SubImage segment)
        {
            Dictionary<Point, LetterPart> letters = extractLetters(segment, out List<Region> regionsUsed);
            var Ps = letters.Where(kvp => kvp.Value == LetterPart.P);
            var Qs = letters.Where(kvp => kvp.Value == LetterPart.Q);

            if (Ps.Count() != 3 || Qs.Count() != 1) return null;
            var Q = Qs.First();
            if (Ps.Any(p => p.Key.Y < Q.Key.Y)) return null;
            
            int minX = regionsUsed.Min(r => r.minX), minY = regionsUsed.Min(r => r.minY),
                maxX = regionsUsed.Max(r => r.maxX), maxY = regionsUsed.Max(r => r.maxY);

            return new Rectangle(minX + segment.startPos.X, minY + segment.startPos.Y, (maxX - minX), (maxY - minY));
        }

        private Dictionary<Point, LetterPart> extractLetters(SubImage segment, out List<Region> regionsUsed)
        {
            int regionSizeThreshold = 29, regionheightThreshold = 3;
            double scoreThreshold = 0.1;
            List<Region> regions = segment.toRegionalImage(new FloodFill()).regions
                .Where(r => r.Size > regionSizeThreshold && r.height > regionheightThreshold).ToList();
            
            Dictionary<Region, CuneiATemplate> regionsToTemplate = new Dictionary<Region, CuneiATemplate>();
            
            foreach (Region region in regions)
            {
                (float bestScore, CuneiATemplate chosenTemplate) = (int.MinValue, null);

                foreach (CuneiATemplate template in _templates)
                {
                    float score = letterScore(region, template);
                    if (score > bestScore) 
                        (bestScore, chosenTemplate) = (score, template);
                }
                
                if (bestScore > scoreThreshold) regionsToTemplate.Add(region, chosenTemplate);
            }

            regionsUsed = regionsToTemplate.Keys.ToList();
            return regionsToTemplate
                .Select(pair => pair.Value.extractLetters(pair.Key))
                .Aggregate(new Dictionary<Point, LetterPart>(), (a, b) => a
                    .Concat(b)
                    .ToDictionary(x=>x.Key,x=>x.Value)
                );
        }

        private float letterScore(Region region, CuneiATemplate template)
        {
            (int x, int y) = (region.minX, region.minY);
            TemplateMatchingImage templateImage = template.templateImage.resize(region.width, region.height).toTemplateMatchingImage();
            float score = 0;

            for (int i = 0; i < templateImage.width; i++)
            for (int j = 0; j < templateImage.height; j++)
            {
                bool inRegion = region.coordinates.Contains(new Vector2(x + i, y + j)); 
                if (templateImage.inputImage[i, j] == 255)
                    score += inRegion ? .5f : -1;
                else if (templateImage.inputImage[i, j] == 0)
                    score += !inRegion ? .5f : -1;
            }

            score /= templateImage.width * templateImage.height;

            return score;
        }
    }
}