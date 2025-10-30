using INFOIBV.Core.Main;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.Helper_Code
{
    /// <summary>
    /// NOT YET IMPLEMENTED
    /// 
    /// Projection is a relatively straightforward method of Segmenting a binary image with text into its respective (candidate) characters
    /// The problem is, that it requires a relatively good image: little noise, few graphics, the photo must be taken frontally and without rotation.
    /// 
    /// Another problem is that the resulting subimages could be of various different sizes, naturally this isn't very handy for template matching.
    /// </summary>
    public partial class ProjectionSegmentator : Segmentator
    {
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
            int[] rowHistogram = rowProjection();
            int row_threshold = getProjectionThreshold(rowHistogram);

            List<Line> rowLines = getLinesFromHistogram(histogram, threshold, binaryImage.height);
            rowLines = HelperFunctions.filter(lines, (line) =>
            {
                double upper_amountStDevs = 2.0f;
                double lower_amountStDevs = 0.75f;

                double meanHeight = lines.Average((Line a) => a.Height);
                double stDev = Math.Sqrt(lines.Sum((Line a) => (a.Height - meanHeight) * (a.Height - meanHeight)) / (double)lines.Count);

                return line.Height >= (meanHeight - (lower_amountStDevs * stDev))
                        && line.Height <= (meanHeight + (upper_amountStDevs * stDev))    // line height has to be roughly similar to the mean
                        && line.foregroundRatio >= 0.03                                  // line must contain a certain ratio of foreground pixels
                        && line.foregroundRatio <= 0.60;
            });

            List<SubImage> characters = new List<SubImage>();

            foreach (Line line in rowLines)
            {
                int[] colHistogram = colProjection(line.startRow, line.endRow);
                int col_threshold = getProjectionThreshold(colHistogram);
                List<Interval> whiteSpaces = getWhiteSpaces(colHistogram, col_threshold);
                List<Line> columnLines = getLinesFromHistogram();
                columnLines = HelperFunctions.filter(lines, (line) =>
                {
                    double upper_amountStDevs = 2.0f;
                    double lower_amountStDevs = 0.75f;

                    double meanHeight = lines.Average((Line a) => a.Height);
                    double stDev = Math.Sqrt(lines.Sum((Line a) => (a.Height - meanHeight) * (a.Height - meanHeight)) / (double)lines.Count);

                    return  line.Height >= (meanHeight - (lower_amountStDevs * stDev))
                            && line.Height <= (meanHeight + (upper_amountStDevs * stDev))    // line height has to be roughly similar to the mean
                            && line.foregroundRatio >= 0.03                                  // line must contain a certain ratio of foreground pixels
                            && line.foregroundRatio <= 0.60;
                });

                characters.Add(binaryData.createSubImage((minX, minY), (minX + glyphDimensions.Width, minY + glyphDimensions.Height)));
            }


            throw new NotImplementedException(); //Implement later, could be a decent solution but is not very robust with Template matching
        }

        protected override RGBImage visualiseSegments()
        {
            return visualiseTextLines();
        }

        private List<Line> charLines()
        {
            int[] rowHistogram = rowProjection();
            int row_threshold = getProjectionThreshold(rowHistogram);

            List<Line> rowLines = getLinesFromHistogram(histogram, threshold, binaryImage.height);
            rowLines = HelperFunctions.filter(lines, (line) =>
            {
                double upper_amountStDevs = 2.0f;
                double lower_amountStDevs = 0.75f;

                double meanHeight = lines.Average((Line a) => a.Height);
                double stDev = Math.Sqrt(lines.Sum((Line a) => (a.Height - meanHeight) * (a.Height - meanHeight)) / (double)lines.Count);

                return line.Height >= (meanHeight - (lower_amountStDevs * stDev))
                        && line.Height <= (meanHeight + (upper_amountStDevs * stDev))    // line height has to be roughly similar to the mean
                        && line.foregroundRatio >= 0.03                                  // line must contain a certain ratio of foreground pixels
                        && line.foregroundRatio <= 0.60;
            });

            List<Line> characters = new List<Line>();

            foreach (Line line in rowLines)
            {
                int[] colHistogram = colProjection(line.startRow, line.endRow);
                int col_threshold = getProjectionThreshold(colHistogram);
                List<Interval> whiteSpaces = getWhiteSpaces(colHistogram, col_threshold);
                List<Line> columnLines = getLinesFromHistogram();
                columnLines = HelperFunctions.filter(lines, (line) =>
                {
                    double upper_amountStDevs = 2.0f;
                    double lower_amountStDevs = 0.75f;

                    double meanHeight = lines.Average((Line a) => a.Height);
                    double stDev = Math.Sqrt(lines.Sum((Line a) => (a.Height - meanHeight) * (a.Height - meanHeight)) / (double)lines.Count);

                    return line.Height >= (meanHeight - (lower_amountStDevs * stDev))
                            && line.Height <= (meanHeight + (upper_amountStDevs * stDev))    // line height has to be roughly similar to the mean
                            && line.foregroundRatio >= 0.03                                  // line must contain a certain ratio of foreground pixels
                            && line.foregroundRatio <= 0.60;
                });

                characters.Add(columnLines);
            }

        }

        private RGBImage visualiseTextLines()
        {
            Bitmap image = binaryData.getImage();

            int[] histogram = rowProjection();
            int threshold = getProjectionThreshold(histogram);

            Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Violet, Color.ForestGreen };
            int ind = 0;
            List<Line> lines = getLinesFromHistogram(histogram, threshold, binaryImage.height);
            lines = HelperFunctions.filter(lines, (line) =>
            {
                double upper_amountStDevs = 2.0f;
                double lower_amountStDevs = 0.75f;

                double meanHeight = lines.Average((Line a) => a.Height);
                double stDev = Math.Sqrt(lines.Sum((Line a) => (a.Height - meanHeight) * (a.Height - meanHeight)) / (double)lines.Count);

                return  line.Height >= (meanHeight - (lower_amountStDevs * stDev)) 
                        && line.Height <= (meanHeight + (upper_amountStDevs * stDev))    // line height has to be roughly similar to the mean
                        && line.foregroundRatio >= 0.03                                  // line must contain a certain ratio of foreground pixels
                        && line.foregroundRatio <= 0.60;
            });

            foreach (Line line in lines)
            {
                for (int i = 0; i < binaryData.width; i++)
                {
                    for (int j = line.startRow; j <= line.endRow; j++)
                    {
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="histogram"></param>
        /// <param name="threshold"></param>
        /// <param name="maxLine">The maximum column or row that can be taken</param>
        /// <returns></returns>
        private List<Line> getLinesFromHistogram(int[] histogram, int threshold, int maxLine)
        {
            List<Interval> whiteSpaces = getWhiteSpaces(histogram, threshold);
            double meanSpaceSize = whiteSpaces.Average((Interval a) => a.Size);
            whiteSpaces = HelperFunctions.filter(whiteSpaces, (Interval a) => a.Size > 0.75 * meanSpaceSize);
            HashSet<int> lineBorders = new HashSet<int>();
            lineBorders.Add(0);
            List<Line> lines = new List<Line>();

            foreach (Interval whiteSpace in whiteSpaces)
            {
                int minVal = int.MaxValue;
                for (int i = whiteSpace.start; i <= whiteSpace.end; i++)
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
                    lineBorders.Add(candidates.First());
                    lineBorders.Add(candidates.Last());
                }
                else
                    lineBorders.Add(candidates[0]);

            }
            lineBorders.Add(maxLine - 1);
            List<int> lineBrdrList = lineBorders.ToList();
            lineBrdrList.Sort();
            for (int i = 1; i < lineBrdrList.Count; i++)
                lines.Add(new Line(lineBrdrList[i - 1], lineBrdrList[i], binaryData));

            return lines;
        }

        /// <summary>
        /// Method that determines a threshold for when a row or column is determined as an empty line between lines of text.
        /// It does this via otsu thresholding.
        /// </summary>
        /// <param name="rowHistogram"></param>
        /// <returns>Threshold for when a row qualifies as whitespace (or in our case blackspace)</returns>
        private int getProjectionThreshold(int[] projection)
        {
            HashSet<int> ValuesSet = new HashSet<int>(projection); //all unique values in the projection
            List<int> uniqueValues = ValuesSet.ToList();
            uniqueValues.Sort();

            int N = projection.Length;
            int N_high;
            decimal maxVar = 0.0m;
            int qMax = 0;

            foreach (int q in uniqueValues) //maximise variance between the high and low class
            {
                (List<int> highs, List<int> lows, int N_low) = getHighLowClasses(projection, q, (x, y) => x - y);
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
            for (int i = 0; i < projection.Length; i++)
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
            int[] result = new int[binaryData.width];

            for (int i = 0; i < binaryData.width; i++)
            {
                for (int j = startRow; j <= endRow; j++)
                {
                    result[i] += binaryData.toArray()[i, j] == 255 ? 1 : 0; //increment if foreground pixel
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
            public int amountForegroundPixels { get; private set; }

            public int Height => endRow - startRow;

            /// <summary>
            /// Ratio of foreground pixels to Total
            /// </summary>
            public float foregroundRatio { get; private set; }
            public Line(int startRow, int endRow, ProcessingImage binaryData)
            {
                this.startRow = startRow;
                this.endRow = endRow;
                this.amountForegroundPixels = getAmountForegroundPixels(binaryData, startRow, endRow);
                this.foregroundRatio = getForegroundRatio(binaryData, startRow, endRow, amountForegroundPixels);
            }
            private int getAmountForegroundPixels(ProcessingImage binaryData, int start, int end)
            {
                int total = 0;

                for (int i = 0; i < binaryData.width; i++)
                {
                    for (int j = start; j <= end; j++)
                    {
                        if (binaryData.toArray()[i, j] == 255) total++;
                    }
                }

                return total;
            }

            private float getForegroundRatio(ProcessingImage binaryData, int startRow, int endRow, int nfgPixels)
            {
                return (float)nfgPixels / (float)((endRow - startRow) * binaryData.width);
            }

        }
    }
}
