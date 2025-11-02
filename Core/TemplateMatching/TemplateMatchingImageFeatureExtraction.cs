using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
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
            var Ps = letters.Where(kvp => kvp.Value == LetterPart.P);
            var Qs = letters.Where(kvp => kvp.Value == LetterPart.Q);

            if (Ps.Count() != 3 || Qs.Count() != 1) return false;
            var Q = Qs.First();
            if (Ps.Any(p => p.Key.Y < Q.Key.Y)) return false;

            return true;
        }

        private Dictionary<Point, LetterPart> extractLetters(SubImage segment)
        {
            int regionSizeThreshold = 29, regionheightThreshold = 3;
            double scoreThreshold = 0.1;
            List<Region> regions = segment.toRegionalImage(new FloodFill()).regions
                .Where(r => r.Size > regionSizeThreshold && r.height > regionheightThreshold).ToList();
            
            Dictionary<Region, CuneiATemplate> regionsToTemplate = new Dictionary<Region, CuneiATemplate>();

            // string ss = "";
            foreach (Region region in regions)
            {
                (float bestScore, CuneiATemplate chosenTemplate) = (int.MinValue, null);

                // ss += "region: (" + region.minX + ", " + region.minY + "), " + region.Size + "\n";
                
                foreach (CuneiATemplate template in _templates)
                {
                    float score = letterScore(region, template);
                    // ss += "template: " + template + ", score: " + score + "\n";
                    if (score > bestScore) 
                        (bestScore, chosenTemplate) = (score, template);
                }
                
                if (bestScore > scoreThreshold) regionsToTemplate.Add(region, chosenTemplate);
            }
            
            // File.WriteAllText("test.txt", ss);

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