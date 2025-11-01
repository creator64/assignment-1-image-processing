using System.Collections.Generic;
using System.Linq;
using INFOIBV.Core.Main;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core.TemplateMatching
{
    public partial class TemplateMatchingImage
    {
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
            int regionThreshold = 15;
            List<Region> regions = segment.toRegionalImage(new FloodFill()).regions
                .Where(r => r.Size > regionThreshold).ToList();

            foreach (Region region in regions)
            {
                
            }
            
            return true;
        }
    }
}