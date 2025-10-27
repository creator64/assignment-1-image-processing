using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using INFOIBV.Core;

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
            ImageData data  = new ImageData(binaryData.toArray());
            if (!data.isBinary()) throw new ArgumentException("A segmentator should receive a binary image as input");
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
            RegionalProcessingImage regionalImage = new RegionalProcessingImage(binaryData.toArray(), new FloodFill());

            Dictionary<int, List<Vector2>> regions = regionalImage.regions;

            List<SubImage> result = new List<SubImage>();

            foreach(List<Vector2> region in regions.Values)
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

            foreach(SubImage s in segments)
            {
                for (int x = s.startPos.X; x <= s.endPos.X; x++)
                    foreach (int y in new int[] { s.startPos.Y, s.endPos.Y })
                        if(x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                            image.SetPixel(x, y, Color.Red);

                for (int y = s.startPos.Y; y <= s.endPos.Y; y++)
                    foreach (int x in new int[] { s.startPos.X, s.endPos.X })
                        if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                            image.SetPixel(x, y, Color.Red);
            }

            return new RGBImage(image);
        }
    }

    /// <summary>
    /// NOT YET IMPLEMENTED
    /// 
    /// Segmentation is a relatively straightforward method of Segmenting a binary image with text into its respective (candidate) characters
    /// The problem is, that it requires a relatively good image: little noise, few graphics, the photo must be taken frontally and without rotation.
    /// 
    /// Another problem is that the resulting subimages could be of various different sizes, naturally this isn't very handy for template matching.
    /// </summary>
    public class ProjectionSegmentator : Segmentator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryData">Binarised form of inscription with the glyps being foreground pixels (pixels of value 255)</param>
        public ProjectionSegmentator(ProcessingImage binaryData) : base(binaryData)
        { }

        protected override List<SubImage> segmentImage()
        {
            throw new NotImplementedException(); //Implement later, could be a decent solution but is not very robust with Template matching
        }

        protected override RGBImage visualiseSegments()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method traverses each row of the image and aggregates how many foreground pixels said row has.
        /// It then returns this aggregate of data.
        /// </summary>
        /// <returns>An array with the amount of foreground pixels indexed for each row</returns>
        private int[] verticalProjection()
        {
            int[] result = new int[binaryData.height];

            for (int i = 0; i < binaryData.width; i++)
            {
                for (int j = 0; j < binaryData.height; j++)
                {
                    result[j] += binaryData.toArray()[i, j] == 255 ? 1 : 0; //increment if foreground pixel
                }
            }

            return result;
        }
        /// <summary>
        /// Creates an array with the amount of foreground pixels of each column, given a vertical interval on the image (basically: a line of text)
        /// </summary>
        /// <param name="startRow">The starting row of the image from which must be searched. Must be smaller than endRow</param>
        /// <param name="endRow">The last row of the image which must be searched. Must be greater than startRow</param>
        /// <returns>An array with the amount of foreground pixels indexed for each column within the bounding box</returns>
        private int[] horizontalProjection(int startRow, int endRow)
        {
            #region Exception checking
            if (startRow < 0 || startRow >= binaryData.height) throw new ArgumentException($"startRow argument was out of range: was {startRow} whilst image height was {binaryData.height}");
            if (endRow < 0 || endRow >= binaryData.height) throw new ArgumentException($"endRow argument was out of range: was {endRow} whilst image height was {binaryData.height}");
            if (startRow > endRow) throw new ArgumentException($"startRow must be greater than endRow: startRow was {startRow} whilst endRow was {endRow}");
            #endregion 
            int[] result = new int[endRow - startRow];

            for (int i = 0; i < binaryData.width; i++)
            {
                for (int j = startRow; j < endRow; j++)
                {
                    result[j] += binaryData.toArray()[i, j] == 255 ? 1 : 0; //increment if foreground pixel
                }
            }

            return result;
        }
    }
}
