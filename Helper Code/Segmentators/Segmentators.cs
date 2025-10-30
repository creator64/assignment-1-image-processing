using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using INFOIBV.Core;
using INFOIBV.Core.Main;

namespace INFOIBV.Helper_Code
{
    public abstract class Segmentator
    {
        public ProcessingImage binaryData { get; protected set; }

        public RGBImage visualisedSegmentation { get; protected set; }

        public List<SubImage> segments { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryData">Binarised form of inscription with the glyps being foreground pixels (pixels of value 255)</param>
        public Segmentator(ProcessingImage binaryData)
        {
            if (!binaryData.getImageData().isBinary()) throw new ArgumentException("A segmentator should receive a binary image as input");
            this.binaryData = binaryData;
        }

        protected abstract List<SubImage> segmentImage();

        protected abstract RGBImage visualiseSegments();
    }
    /// <summary>
    /// Segmentator that makes use of the Connected Component technique (regions).
    /// This one makes use of a rather simple technique by which each region is seen as a candidate character.
    /// </summary>
    public class SimpleConnectionSegmentator : Segmentator
    {
        private (int Width, int Height) glyphDimensions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryData">Image in binary format that must be segmented</param>
        /// <param name="dimensions">Dimensions of your Reference Image for template matching</param>
        public SimpleConnectionSegmentator(ProcessingImage binaryData, (int Width, int Height) dimensions) : base(binaryData)
        {
            this.glyphDimensions = dimensions;

            this.segments = segmentImage();
            this.visualisedSegmentation = visualiseSegments();
        }

        protected override List<SubImage> segmentImage()
        {
            Dictionary<int, List<Vector2>> regions = binaryData.
                toRegionalImage(new FloodFill())
                .regions;

            List<SubImage> result = new List<SubImage>();

            foreach (List<Vector2> region in regions.Values)
            {
                int minX = (int)Math.Floor(region.Min(v => v.X));
                int minY = (int)Math.Floor(region.Min(v => v.Y));

                result.Add(binaryData.createSubImage((minX, minY), (minX + glyphDimensions.Width, minY + glyphDimensions.Height)));
            }

            return result;
        }

        protected override RGBImage visualiseSegments()
        {
            Bitmap image = binaryData.getImage();

            foreach (SubImage s in segments)
            {
                for (int x = s.startPos.X; x <= s.endPos.X; x++)
                    foreach (int y in new int[] { s.startPos.Y, s.endPos.Y })
                        if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                            image.SetPixel(x, y, Color.Red);

                for (int y = s.startPos.Y; y <= s.endPos.Y; y++)
                    foreach (int x in new int[] { s.startPos.X, s.endPos.X })
                        if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                            image.SetPixel(x, y, Color.Red);
            }

            return new RGBImage(image);
        }
    }
}
