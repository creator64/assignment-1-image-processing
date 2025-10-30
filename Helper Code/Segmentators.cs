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
    /// Projection is a relatively straightforward method of Segmenting a binary image with text into its respective (candidate) characters
    /// The problem is, that it requires a relatively good image: little noise, few graphics, the photo must be taken frontally and without rotation.
    /// 
    /// Another problem is that the resulting subimages could be of various different sizes, naturally this isn't very handy for template matching.
    /// </summary>
    public class ProjectionSegmentator : Segmentator
    {
        public List<SubImage> lines { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryData">Binarised form of inscription with the glyps being foreground pixels (pixels of value 255)</param>
        public ProjectionSegmentator(ProcessingImage binaryData) : base(binaryData)
        {
            visualisedSegmentation = visualiseSegments();
        }

        protected override List<SubImage> segmentImage()
        {
            throw new NotImplementedException(); //Implement later, could be a decent solution but is not very robust with Template matching
        }

        protected override RGBImage visualiseSegments()
        {
            bool flag = true;

            
            Bitmap image = binaryData.getImage();

            int[] histogram = rowProjection();
            int threshold = getRowThreshold(histogram);

            if (flag)
            {
                Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Violet, Color.ForestGreen };
                int ind = 0;
                List<Line> lines = getLines();
                foreach(Line line in lines)
                {
                    for(int i = 0; i < binaryData.width; i++)
                    {
                        for(int j = line.startRow; j <= line.endRow; j++)
                        {
                            //if (j == line.startRow || j == line.endRow || i == 0 || i == binaryData.width - 1)

                            if (binaryData.toArray()[i, j] != 255 && !(j == line.startRow || j == line.endRow || i == 0 || i == binaryData.width - 1))
                                image.SetPixel(i, j, colors[ind % 4]);
                            else if (j == line.startRow || j == line.endRow || i == 0 || i == binaryData.width - 1)
                                image.SetPixel(i, j, Color.Blue);


                        }
                    }
                    ind++;
                }

                return new RGBImage(image);
            }
            else
            {
                List<Interval> whiteSpaces = getWhiteSpaces(histogram, threshold);
                double meanSpaceSize = whiteSpaces.Average((Interval a) => a.Size);
                whiteSpaces = filterWhiteSpaces(whiteSpaces, (Interval a) => a.Size > 0.75 * meanSpaceSize);

                Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Blue, Color.Yellow };
                int ind = 0;
                foreach(Interval whiteSpace in whiteSpaces)
                {
                    for (int i = 0; i < binaryData.width; i++)
                    {
                        for (int j = whiteSpace.start; j < whiteSpace.end; j++)
                        {
                            image.SetPixel(i, j, colors[ind % 4]);
                        }
                    }
                    ind++;
                }

                return new RGBImage(image);
            }
        }


        private List<Line> getLines()
        {

            int[] histogram = rowProjection();
            int threshold = getRowThreshold(histogram);

            List<Interval> whiteSpaces = getWhiteSpaces(histogram, threshold);
            double meanSpaceSize = whiteSpaces.Average((Interval a) => a.Size);
            whiteSpaces = filterWhiteSpaces(whiteSpaces, (Interval a) => a.Size > 0.75 * meanSpaceSize);
            List<int> lineBorders = new List<int>();
            lineBorders.Add(0);
            List<Line> lines = new List<Line>();

            foreach (Interval whiteSpace in whiteSpaces)
            {
                int minVal = int.MaxValue;
                for(int i = whiteSpace.start; i <= whiteSpace.end; i++)
                {
                    if (histogram[i] < minVal)
                    {
                        minVal = histogram[i];
                    }
                }

                List<int> candidates = new List<int>();

                for (int i = whiteSpace.start; i <= whiteSpace.end; i++)
                    if (histogram[i] == minVal)
                        candidates.Add(i);

                if (candidates.Count > 1)
                {
                    if (minVal == 0)
                        lineBorders.Add(candidates[candidates.Count / 2]); //take the middle one;
                    else
                    {
                        lineBorders.Add(candidates.First());
                        lineBorders.Add(candidates.Last());                    }


                }
                else
                    lineBorders.Add(candidates[0]);

            }

            for(int i = 1; i < lineBorders.Count; i++)
                lines.Add(new Line(lineBorders[i - 1], lineBorders[i]));
            
            return lines;
        }

        /// <summary>
        /// Method that determines a threshold for when a row is determined as an empty line between lines of text.
        /// It does this via otsu thresholding.
        /// </summary>
        /// <param name="rowHistogram"></param>
        /// <returns>Threshold for when a row qualifies as whitespace (or in our case blackspace)</returns>
        private int getRowThreshold(int[] rowHistogram)
        {
            HashSet<int> ValuesSet = new HashSet<int>(rowHistogram); //all unique values in the rowHistogram
            List<int> uniqueValues = ValuesSet.ToList();
            uniqueValues.Sort();

            int N = rowHistogram.Length;
            int N_high;
            decimal maxVar = 0.0m;
            int qMax = 0;

            foreach (int q in uniqueValues) //maximise variance between the high and low class
            {
                (List<int> highs, List<int> lows, int N_low) = getHighLowClasses(rowHistogram, q, (x, y) => x - y);
                N_high = N - N_low;

                if (N_low > 0 && N_high > 0)
                {
                    decimal mean_low = lows.Sum() / N_low;

                    decimal mean_high = highs.Sum() / N_high;
                    decimal variance = (1.0m / (long)(N * N)) * N_low * N_high * (decimal)((mean_low - mean_high) * (mean_low - mean_high));

                    if (variance > maxVar)
                    {
                        maxVar = variance;
                        qMax = q;
                    }
                }

            }

            return qMax;
        }

        private (List<T> highs, List<T> lows, int N_low) getHighLowClasses<T>(IEnumerable<T> collection, T mean, Comparison<T> comparer)
        {
            List<T> lows = new List<T>();
            int N_low = 0;
            List<T> highs = new List<T>();

            foreach (T val in collection)
            {
                if (comparer(val, mean) <= 0)
                {
                    lows.Add(val);
                    N_low++;
                }
                else highs.Add(val);
            }

            return (highs, lows, N_low);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projection">either a projection of the amount of foreground pixels on the rows or the columns</param>
        /// <param name="threshold">How many foreground pixels a row or column needs to have to not qualify as white space</param>
        /// <param name="maxGap">exclusive maximum gap between rows or columns of whitespace</param>
        /// <returns></returns>
        private List<Interval> getWhiteSpaces(int[] projection, int threshold, int maxGap = 0)
        {
            List<Interval> whiteSpaces = new List<Interval>();
            Interval currentInterval = null;
            for(int i = 0; i < projection.Length; i++)
            {
                if (projection[i] < threshold && currentInterval == null)
                        currentInterval = new Interval(i, i);
                else if (projection[i] < threshold)
                    currentInterval.append(i);
                else if (currentInterval != null && i - currentInterval.end > maxGap)
                {
                    whiteSpaces.Add(currentInterval);
                    currentInterval = null;
                }
            }
            if (currentInterval != null)
                whiteSpaces.Add(currentInterval);

            return whiteSpaces;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whiteSpaces">list of intervals of whitespace</param>
        /// <param name="filter">Function that returns true when the whitespace is considered "valid" and false when it isn't</param>
        /// <returns>The list of whitespaces that make the filter return true</returns>
        private List<Interval> filterWhiteSpaces(List<Interval> whiteSpaces, Func<Interval, bool> filter)
        {
            List<Interval> filteredWhiteSpaces = new List<Interval>();

            foreach(Interval whiteSpace in whiteSpaces)
            {
                if(filter(whiteSpace))
                    filteredWhiteSpaces.Add(whiteSpace);
            }

            return filteredWhiteSpaces;
        }



        /// <summary>
        /// This method traverses each row of the image and aggregates how many foreground pixels said row has.
        /// It then returns this aggregate of data.
        /// </summary>
        /// <returns>An array with the amount of foreground pixels indexed for each row</returns>
        private int[] rowProjection()
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
        private int[] colProjection(int startRow, int endRow)
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

        private class Interval
        {
            public int start { get; private set; }
            public int end { get; private set; }

            public int Size => this.end - this.start;

            public Interval(int start, int end)
            {
                if (start > end) throw new ArgumentException("end variable must be greater than start variable");
                this.start = start;
                this.end = end;
            }

            public void append(int value)
            {
                if (value < start) start = value;
                else if (value > end) end = value;
            }

            public override string ToString()
            {
                return $"[{start} ... {end}]";
            }
        }

        private class Line
        {
            public int startRow { get; private set; }
            public int endRow { get; private set; }
            public Line(int startRow, int endRow)
            {
                this.startRow = startRow;
                this.endRow = endRow;
            }
        }
    }
}
