using INFOIBV.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.Helper_Code
{
    public class HoughTransform : ProcessingImage
    {
        #region Parameters
        public int thetaDetail { get; private set; }
        public int rDetail { get; private set; }
        #endregion

        #region Getters for Calculations
        public double Rmax { get { return 0.5f * (double)Math.Sqrt((width * width) + (height * height)); } }

        public double deltaTheta { get { return (Math.PI) / thetaDetail; } }

        public double deltaR { get { return (double)(2 * Rmax) / rDetail; } }
        #endregion

        public HoughTransform(byte[,] inputImage, int thetaDetail, int rDetail) : base(inputImage)
        {
            this.thetaDetail = thetaDetail;
            this.rDetail = rDetail;

            // create RGBImage
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>The accumulator array wrapped in a ProcessingImage</returns>
        public ProcessingImage houghTransform()
        {

            float[,] outputImage = new float[thetaDetail + 1, rDetail + 1];

            Func<int, int, Func<double, double>> generateWave = (x, y) => theta => x * Math.Cos(theta) + y * Math.Sin(theta);

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (inputImage[i, j] == 0) continue;
                    Func<double, double> wave = generateWave(i - (width / 2), (height / 2) - j); // used to have (height / 2) - j

                    for (int d = 0; d <= thetaDetail; d++)
                    {
                        double theta = d * deltaTheta;
                        double Rreal = wave(theta);
                        int RRounded = (int)Math.Round(Rreal / deltaR);
                        int Rindex = RRounded + (rDetail / 2);
                        outputImage[d, Rindex] = Math.Min(255, outputImage[d, Rindex] + inputImage[i, j] / 255f);
                    }
                }

            return new ProcessingImage(
                    ConverterMethods.convertToBytes(outputImage)
                );
        }

        public (ProcessingImage processedAccumulator, List<Vector2> ThetaRPairs) peakFinding(ProcessingImage accumulatorArray, byte t_peak,ImageRegionFinder regionFinder = null)
        {
            if (regionFinder == null) regionFinder = new FloodFill();

            bool[,] structElem = {
                { false, true, false},
                { true, false, true},
                { false, true, false}
            };
            ProcessingImage processingImage = accumulatorArray.halfThresholdImage(t_peak).binaryCloseImage(structElem);

            return (processingImage, getThetaRPairs(processingImage.toRegionalImage(regionFinder)));
        }

        private List<Vector2> getThetaRPairs(RegionalProcessingImage processedAcc)
        {
            return processedAcc.regionCenters()
                .Select(p =>
                    HelperFunctions.coordinateToThetaRPair(p.Value,
                        thetaDetail, rDetail))
                .ToList();
        }

        public Bitmap houghLineSegments(List<Vector2> peaks, byte minIntensity, ushort minSegLength, ushort maxGap)
        {

            List<((int X, int Y) startPoint, (int X, int Y) endPoint)> lineSegments = new List<((int X, int Y) startPoint, (int X, int Y) endPoint)>();

            List<LineSegment> longSegments = new List<LineSegment>();
            Bitmap outputImage = this.convertToImage();


            // Find Line 
            foreach (Vector2 ThetaR in peaks)
            {
                float theta = ThetaR.X, r = ThetaR.Y * (float)Rmax;

                LineSegment currentSegment = new LineSegment(theta, r, maxGap, minSegLength);


                for (int x = 0; x < inputImage.GetLength(0); x++)
                {
                    int xTransform = x - (width / 2);
                    double yTransform = (double)(xTransform * Math.Cos(theta) - r) / (float)(-Math.Sin(theta));
                    double y = (height / 2) - yTransform;

                    int roundY = (int)Math.Round(y);

                    if (roundY >= 0 && roundY < inputImage.GetLength(1) && inputImage[x, roundY] >= minIntensity)
                        currentSegment.addPoint(x, roundY, width, height);
                }
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    int yTransform = (width / 2) - y;
                    double xTransform = (double)(yTransform * Math.Sin(theta) - r) / (float)(-Math.Cos(theta));
                    double x = xTransform + (height / 2);

                    int roundX = (int)Math.Round(x);

                    if (roundX >= 0 && roundX < inputImage.GetLength(0) && inputImage[roundX, y] >= minIntensity)
                        currentSegment.addPoint(roundX, y, width, height);
                }
                if (currentSegment.LongEnough)
                {
                    currentSegment.close();
                    longSegments.Add(currentSegment);
                }
            }

            List<LineSegment> shortSegments = new List<LineSegment>();

            foreach (LineSegment seg in longSegments)
                foreach (LineSegment subseg in seg.getSegments(width, height))
                    shortSegments.Add(subseg);

            foreach (LineSegment seg in shortSegments)
                seg.drawToImage(outputImage, Color.Red, width, height, 2);

            #region Debugging Shenanigans
            bool drawShortSegPoints = false;
            bool drawLongSegPoints = false;

            if (drawLongSegPoints)
                foreach (LineSegment seg in longSegments)
                    seg.drawPointsToImage(outputImage, Color.MidnightBlue, 2);
            if (drawShortSegPoints)
                foreach (LineSegment seg in shortSegments)
                    seg.drawPointsToImage(outputImage, Color.GreenYellow, 1);
            #endregion
            return outputImage;
        }

        public List<(int X, int Y)> getHoughLineIntersections(List<Vector2> peaks)
        {
            List<(int X, int Y)> intersectionPoints = new List<(int X, int Y)>();
            foreach(Vector2 a in peaks)
            {
                foreach (Vector2 b in peaks)
                {
                    if (a.X == b.X && a.Y == b.Y ) continue;

                    float thetaA = a.X, thetaB = b.X;
                    float rA = a.Y * (float)Rmax, rB = b.Y * (float)Rmax;

                    if (Math.Sin(thetaB - thetaA) == 0) continue;

                    float factor = 1 / (float)Math.Sin(thetaB - thetaA);

                    float x0= factor * (float)(rA * Math.Sin(thetaB) - rB * Math.Sin(thetaA));
                    float y0 = factor * (float)(rB * Math.Cos(thetaA) - rA * Math.Cos(thetaB));

                    int x = (int)Math.Round(x0);
                    int y = (int)Math.Round(y0);

                    intersectionPoints.Add((x + (width / 2), (height / 2) - y));
                }
            }

            return intersectionPoints;
        }

        public Bitmap drawPoints(List<(int X, int Y)> points, Color color)
        {
            Bitmap outputImage = this.convertToImage();
            int offset = 3;

            foreach ((int X, int Y) in points)
            {
                Debug.WriteLine($"Drawing point: ({X}, {Y})");
                for (int xOffset = -offset; xOffset <= offset; xOffset++)
                    for (int yOffset = -offset; yOffset <= offset; yOffset++)
                        if (X + xOffset >= 0 && X + xOffset < width && Y + yOffset >= 0 && Y + yOffset < height)
                            outputImage.SetPixel(X + xOffset, Y + yOffset, color);
            }


            return outputImage;
        }
    }
}
