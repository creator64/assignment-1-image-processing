using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using INFOIBV.Core;
using INFOIBV.Core.Main;
using static System.Windows.Forms.LinkLabel;

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
        #region Thresholds / parameters

        float upper_amountStDevs = 2.0f;        //maximum amount of stDevs that a line may be larger than the mean to still qualify as a valid line
        float lower_amountStDevs = 0.75f;       //maximum amount of stDevs that a line may be smaller than the mean to still qualify as a valid line

        float minForegroundRatio = 0.03f;       //The minimum percentage of foreground pixels a line must hold to qualify as a line
        float maxForegroundRatio = 0.60f;       //The maximum percentage of foreground pixels a line must have to qualify as a line
        float minCharForegroundRatio = 0.05f;   //The minimum percentage of foreground pixels a character must have to qualify as a character.

        float minWhiteSpaceRatio = 0.75f;       //The minimum percentage of the mean whitespace-size a whitespace must have to qualify as a valid whitespace.

        float minWidthHeightRatio = 0.5f;       //The minimum ratio a detected characters width may have to the height
        float maxWidthHeightRatio = 1.5f;       //The maximum ratio a detected characters width may have to the height

        #endregion 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryData">Binarised form of inscription with the glyps being foreground pixels (pixels of value 255)</param>
        public ProjectionSegmentator(ProcessingImage binaryData) : base(binaryData)
        {
            this.segments = segmentImage();
            this.visualisedSegmentation = visualiseSegments();
        }

        protected override List<SubImage> segmentImage()
        {
            int[] rowHistogram = rowProjection();
            int rowThreshold = getProjectionThreshold(rowHistogram);

            Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Violet, Color.ForestGreen };
            List<Line> horizontalLines = getLinesFromHistogram(rowHistogram, rowThreshold);

            horizontalLines = filter(horizontalLines, (line) =>
            {

                double meanHeight = horizontalLines.Average((Line a) => a.Height);
                double stDev = Math.Sqrt(horizontalLines.Sum((Line a) => (a.Height - meanHeight) * (a.Height - meanHeight)) / (double)horizontalLines.Count);

                return (line.Height >= (meanHeight - (lower_amountStDevs * stDev)) && line.Height <= (meanHeight + (upper_amountStDevs * stDev)))    // line height has to be roughly similar to the mean
                        && line.foregroundRatio >= minForegroundRatio                                                                                              // line must contain a certain ratio of foreground pixels
                        && line.foregroundRatio <= maxForegroundRatio;
            });

            List<SubImage> segments = new List<SubImage>();

            
            foreach(Line line in horizontalLines)
            {
                List<SubImage> characters = getCharactersFromLine(line);
                characters = filter(characters, (SubImage ch) =>
                {
                    return ch.foregroundRatio >= minCharForegroundRatio;
                });

                characters = correctErroneousSegments(characters);

                segments.AddRange(characters);
            }

            return segments;
        }

        private List<SubImage> correctErroneousSegments(List<SubImage> characters)
        {
            List<SubImage> processed = new List<SubImage>();

            for(int i = 0; i < characters.Count; i++)
            {
                SubImage cur = characters[i];

                if (cur.widthHeightRatio >= minWidthHeightRatio &&
                    cur.widthHeightRatio <= maxWidthHeightRatio)
                {
                    processed.Add(cur);
                }
                else if (cur.widthHeightRatio < minWidthHeightRatio)
                {
                    //Debug.WriteLine("merging");
                    (bool anyMerge, bool mergedToRight) changes = mergeSmallSegment(cur, i, characters, processed);

                    SubImage mergedChar = processed.Last();

                    if(changes.anyMerge)
                        characters[i] = mergedChar;
                    if (changes.mergedToRight)
                        characters[i + 1] = mergedChar;

                    if (mergedChar.widthHeightRatio > maxWidthHeightRatio && changes.anyMerge)
                    {
                        splitLargeSegment(mergedChar, processed);
                        processed.RemoveAt(processed.Count - 2);
                        characters[i] = processed.Last();
                        if(changes.mergedToRight) characters[i + 1] = processed.Last();
                    }

                }
                else
                {
                    splitLargeSegment(cur, processed);
                    characters[i] = processed.Last();
                }
            }


            return processed;
        }

        private (bool anyMerge, bool mergedToRight) mergeSmallSegment(SubImage cur, int index, List<SubImage> characters, List<SubImage> processed)
        {
            //find the neighbouring characters
            ProcessingImage parent = cur.parentImage;
            SubImage leftChar = null;
            SubImage rightChar = null;
            if (index > 0)
                leftChar = characters[index - 1];
            if (index < characters.Count - 1)
                rightChar = characters[index + 1];

            List<Vector2> largestRegion = cur.getLargestRegion();

            float maxX_largestReg = largestRegion.Max((Vector2 v) => v.X);
            float minX_largestReg = largestRegion.Min((Vector2 v) => v.X);

            // Select neighbouring character to merge with
            if (leftChar != null && rightChar != null)
            {
                //Merge with the character that is closest horizontally
                float leftX = leftChar.getRightMostPixel(leftChar.foregroundPixels).X;
                float rightX = rightChar.getLeftMostPixel(rightChar.foregroundPixels).X;

                float diffLeft = minX_largestReg - leftX;
                float diffRight = maxX_largestReg - rightX;

                if (diffLeft <= diffRight)
                {
                    //merge subimages
                    SubImage mergedImage = parent.createSubImage(leftChar.startPos, cur.endPos);
                    if(mergedImage.widthHeightRatio > maxWidthHeightRatio)
                    {

                    }
                    processed.RemoveAll((SubImage s) => s.startPos == leftChar.startPos && s.endPos == leftChar.endPos);
                    processed.Add(mergedImage);
                    return (true, false);
                }
                else
                {
                    SubImage mergedImage = parent.createSubImage(cur.startPos, rightChar.endPos);

                    processed.RemoveAll((SubImage s) => s.startPos == rightChar.startPos && s.endPos == rightChar.endPos);
                    processed.Add(mergedImage);
                    return (true, true);
                }
            }
            else if (leftChar != null)
            {
                SubImage mergedImage = parent.createSubImage(leftChar.startPos, cur.endPos);
                processed.RemoveAll((SubImage s) => s.startPos == leftChar.startPos && s.endPos == leftChar.endPos);
                processed.Add(mergedImage);
                return (true, false);

            }
            else if (rightChar != null)
            {
                SubImage mergedImage = parent.createSubImage(cur.startPos, rightChar.endPos);
                processed.RemoveAll((SubImage s) => s.startPos == rightChar.startPos && s.endPos == rightChar.endPos);
                processed.Add(mergedImage);
                return (true, true);
            }
            return (false, false);
        }

        private void splitLargeSegment(SubImage cur, List<SubImage> processed)
        {
            ProcessingImage parent = cur.parentImage;
            int[] curProjection = new int[cur.width];

            for(int i = cur.startPos.X; i < cur.startPos.X + cur.width; i++)
            {
                for(int j = cur.startPos.Y; j < cur.startPos.Y + cur.height; j++)
                {
                    curProjection[i - cur.startPos.X] += cur.toArray()[i - cur.startPos.X, j - cur.startPos.Y] == 255 ? 1 : 0;
                }
            }

            int threshold = getProjectionThreshold(curProjection);

            int bestCol = cur.width / 2;
            float bestHeuristic = float.MaxValue;

            for(int i = 0; i < curProjection.Length; i++)
            {
                if (curProjection[i] > threshold) continue;

                int leftWidth = i;
                int rightWidth = cur.width - leftWidth;

                float leftRatio = (float)leftWidth / cur.height;
                float rightRatio = (float)rightWidth / cur.height;

                float heuristic = (float)Math.Sqrt(((leftRatio - 1.0f) * (leftRatio - 1.0f)) + ((rightRatio - 1.0f) * (rightRatio - 1.0f)));

                if (heuristic < bestHeuristic)
                {
                    bestHeuristic = heuristic;
                    bestCol = i;
                }
            }


            SubImage leftHalf = parent.createSubImage(cur.startPos, (bestCol + cur.startPos.X, cur.endPos.Y));
            SubImage rightHalf = parent.createSubImage((bestCol + cur.startPos.X + 1, cur.startPos.Y), cur.endPos);

            processed.Add(leftHalf);
            processed.Add(rightHalf);
        }

        protected override RGBImage visualiseSegments()
        {
            Bitmap image = binaryData.getImage();

            Color[] colors = new Color[] { Color.Red, Color.Blue, Color.Orange, Color.ForestGreen };
            int index = 0;

            foreach (SubImage s in segments)
            {
                Color currentCol = colors[index % 4];

                for (int x = s.startPos.X; x <= s.endPos.X; x++)
                    for (int y = s.startPos.Y; y <= s.endPos.Y; y++)
                            if(binaryData.toArray()[x, y] != 255) image.SetPixel(x, y, currentCol);


                //Debug.WriteLine($"-----------------------------------");
                //Debug.WriteLine($"Character: {index}");
                //Debug.WriteLine($"current width: {s.width}");
                //Debug.WriteLine($"current height: {s.height}");
                //Debug.WriteLine($"current width height ratio: {s.widthHeightRatio}");
                //Debug.WriteLine($"-----------------------------------");
                index++;
            }

            return new RGBImage(image);
        }

        private  RGBImage visualiseTextLines()
        {

            Bitmap image = binaryData.getImage();

            int[] histogram = rowProjection();
            int threshold = getProjectionThreshold(histogram);

            Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Violet, Color.ForestGreen };
            int ind = 0;
            List<Line> lines = getLinesFromHistogram(histogram, threshold);
            lines = filter(lines, (line) =>
            {
                double meanHeight = lines.Average((Line a) => a.Height);
                double stDev = Math.Sqrt(lines.Sum((Line a) => (a.Height - meanHeight) * (a.Height - meanHeight)) / (double)lines.Count);

                return (line.Height >= (meanHeight - (lower_amountStDevs * stDev)) && line.Height <= (meanHeight + (upper_amountStDevs * stDev)))   // line height has to be roughly similar to the mean
                        && line.foregroundRatio >= minForegroundRatio                                                                               // line must contain a certain ratio of foreground pixels
                        && line.foregroundRatio <= maxForegroundRatio;
            });

            foreach (Line line in lines)
            {
                for (int i = 0; i < binaryData.width; i++)
                {
                    for (int j = line.startRow; j <= line.endRow; j++)
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

        private List<T> filter<T>(IEnumerable<T> list, Func<T, bool> filter)
        {
            List<T> filtered = new List<T>();

            foreach(T obj in list)
            {
                if (filter(obj))
                    filtered.Add(obj);
            }

            return filtered;
        }

        private List<Line> getLinesFromHistogram(int[] histogram, int threshold)
        {
            List<Interval> whiteSpaces = getWhiteSpaces(histogram, threshold);
            double meanSpaceSize = whiteSpaces.Average((Interval a) => a.Size);
            whiteSpaces = filter(whiteSpaces, (Interval a) => a.Size > minWhiteSpaceRatio * meanSpaceSize);
            HashSet<int> lineBorders = new HashSet<int>();
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
                    lineBorders.Add(candidates.First());
                    lineBorders.Add(candidates.Last());
                }
                else
                    lineBorders.Add(candidates[0]);

            }
            lineBorders.Add(binaryData.height - 1);
            List<int> lineBrdrList = lineBorders.ToList();
            lineBrdrList.Sort();
            for(int i = 1; i < lineBrdrList.Count; i++)
                lines.Add(new Line(lineBrdrList[i - 1], lineBrdrList[i], binaryData));
            
            return lines;
        }

        private List<SubImage> getCharactersFromLine(Line line)
        {
            int[] histogram = colProjection(line.startRow, line.endRow);
            int threshold = getProjectionThreshold(histogram);

            List<Interval> whiteSpaces = getWhiteSpaces(histogram, threshold);
            double meanSpaceSize = whiteSpaces.Average((Interval a) => a.Size);
            whiteSpaces = filter(whiteSpaces, (Interval a) => a.Size > minWhiteSpaceRatio * meanSpaceSize);
            HashSet<int> lineBorders = new HashSet<int>();
            lineBorders.Add(0);
            List<SubImage> characters = new List<SubImage>();

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
            lineBorders.Add(binaryData.width - 1);
            List<int> lineBrdrList = lineBorders.ToList();
            lineBrdrList.Sort();
            for (int i = 1; i < lineBrdrList.Count; i++)
                characters.Add(binaryData.createSubImage((lineBrdrList[i - 1], line.startRow), (lineBrdrList[i], line.endRow)));

            return characters;
        }

        /// <summary>
        /// Method that determines a threshold for when a row is determined as an empty line between lines of text.
        /// It does this via otsu thresholding.
        /// </summary>
        /// <param name="projection"></param>
        /// <returns>Threshold for when a row qualifies as whitespace (or in our case blackspace)</returns>
        private int getProjectionThreshold(int[] projection)
        {
            HashSet<int> ValuesSet = new HashSet<int>(projection); //all unique values in the rowHistogram
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
