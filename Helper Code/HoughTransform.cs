using INFOIBV.Core;
using System;
using System.Collections.Generic;
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
        int minIntensity;
        ImageRegionFinder regionFinder;
        public int thetaDetail { get; private set; }
        public int rDetail { get; private set; }
        #endregion

        #region Getters for Calculations
        int imageWidth { get { return inputImage.GetLength(0); } }
        int imageHeight { get { return inputImage.GetLength(1); } }
        public float Rmax { get { return 0.5f * (float)Math.Sqrt((imageWidth * imageWidth) + (imageHeight * imageHeight)); } }

        public float deltaTheta { get { return (float)(Math.PI) / thetaDetail; } }

        public float deltaR { get { return (float)(2 * Rmax) / rDetail; } }

        (int X, int Y) centerPoint { get { return (imageWidth / 2, imageHeight / 2); } }
        #endregion

        Bitmap RGBImage; //Image that will be used to overlay lines on in houghLineSegments

        public HoughTransform(byte[,] inputImage, ImageRegionFinder regionFinder, int minIntensity, int thetaDetail, int rDetail) : base(inputImage)
        {
            this.minIntensity = minIntensity;
            this.thetaDetail = thetaDetail;
            this.rDetail = rDetail;

            // create RGBImage
            this.RGBImage = convertToImage(inputImage);
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
                        double theta = d * Math.PI / thetaDetail;
                        double Rreal = wave(theta);
                        double Rstep = (double)(2 * Rmax) / rDetail;
                        int RRounded = (int)Math.Round(Rreal / Rstep);
                        int Rindex = RRounded + (rDetail / 2);
                        outputImage[d, Rindex] = Math.Min(255, outputImage[d, Rindex] + inputImage[i, j] / 255f);
                    }
                }

            return new ProcessingImage(
                    ConverterMethods.convertToBytes(outputImage)
                );
        }

        public List<Vector2> peakFinding(ProcessingImage accumulatorArray, byte t_peak)
        {
            bool[,] structElem = {
                { false, true, false},
                { true, false, true},
                { false, true, false}
            };
            ProcessingImage processingImage = accumulatorArray.halfThresholdImage(t_peak).binaryCloseImage(structElem);

            return getThetaRPairs(processingImage.toRegionalImage(regionFinder));
        }

        private List<Vector2> getThetaRPairs(RegionalProcessingImage processedAcc)
        {
            return processedAcc.regionCenters()
                .Select(p =>
                    HelperFunctions.coordinateToThetaRPair(p.Value,
                        width, height))
                .ToList();
        }

        public static void houghLineSegments(byte[,] edgeMap, List<Vector2> peaks, byte minIntensity, ushort minSegLength, ushort maxGap)
        {
            int width = edgeMap.GetLength(0), height = edgeMap.GetLength(1);
            float maxR = 0.5f * (float)Math.Sqrt((width * width) + (height * height));

            // Create initial RGB image
            Bitmap OutputImage = new Bitmap(edgeMap.GetLength(0), edgeMap.GetLength(1)); // create new output image
            for (int x = 0; x < edgeMap.GetLength(0); x++)          // loop over columns
                for (int y = 0; y < edgeMap.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(edgeMap[x, y], edgeMap[x, y], edgeMap[x, y]);

                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            List<((int X, int Y) startPoint, (int X, int Y) endPoint)> lineSegments = new List<((int X, int Y) startPoint, (int X, int Y) endPoint)>();

            List<LineSegment> longSegments = new List<LineSegment>();

            // Find Line 
            foreach (Vector2 ThetaR in peaks)
            {
                float theta = ThetaR.X, r = ThetaR.Y * maxR;

                LineSegment currentSegment = new LineSegment(theta, r, maxGap, minSegLength);


                for (int x = 0; x < edgeMap.GetLength(0); x++)
                {
                    int xTransform = x - (width / 2);
                    float yTransform = (float)(xTransform * Math.Cos(theta) - r) / (float)(-Math.Sin(theta));
                    float y = (height / 2) - yTransform;

                    int roundY = (int)Math.Round(y);

                    if (roundY >= 0 && roundY < edgeMap.GetLength(1) && edgeMap[x, roundY] >= minIntensity)
                        currentSegment.addPoint(x, roundY, width, height);
                }
                for (int y = 0; y < edgeMap.GetLength(1); y++)
                {
                    int yTransform = (height / 2) - y;
                    float xTransform = (float)(yTransform * Math.Sin(theta) - r) / (float)(-Math.Cos(theta));
                    float x = xTransform + (width / 2);

                    int roundX = (int)Math.Round(x);

                    if (roundX >= 0 && roundX < edgeMap.GetLength(0) && edgeMap[roundX, y] >= minIntensity)
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
                seg.drawToImage(OutputImage, Color.Red, width, height, 2);

            bool drawShortSegPoints = false;
            bool drawLongSegPoints = false;

            if (drawLongSegPoints)
                foreach (LineSegment seg in longSegments)
                    seg.drawPointsToImage(OutputImage, Color.MidnightBlue, 2);
            if (drawShortSegPoints)
                foreach (LineSegment seg in shortSegments)
                    seg.drawPointsToImage(OutputImage, Color.GreenYellow, 1);

            return OutputImage;
        }
    }
}
