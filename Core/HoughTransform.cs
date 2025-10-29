using INFOIBV.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using INFOIBV.Helper_Code;
using System.Text;
using System.Threading.Tasks;
using INFOIBV.Core.Main;

namespace INFOIBV.Core
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

        public RGBImage houghLineSegments(List<Vector2> peaks, byte minIntensity, ushort minSegLength, ushort maxGap)
        {

            List<((int X, int Y) startPoint, (int X, int Y) endPoint)> lineSegments = new List<((int X, int Y) startPoint, (int X, int Y) endPoint)>();

            Bitmap outputImage = this.getImage();

            List<LineSegment> segments = getLineSegments(peaks, minIntensity, maxGap, minSegLength);

            foreach (LineSegment seg in segments)
                seg.drawToImage(outputImage, Color.Red, width, height, 2);

            return new RGBImage(outputImage);
        }

        public List<LineSegment> getLineSegments(List<Vector2> peaks, byte minIntensity, ushort maxGap, ushort minSegLength) 
        {
            List<LineSegment> longSegments = new List<LineSegment>();

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
                    int yTransform = (height / 2) - y;
                    double xTransform = (double)(yTransform * Math.Sin(theta) - r) / (float)(-Math.Cos(theta));
                    double x = xTransform + (width / 2);

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

            return shortSegments;
        }

        public List<(int X, int Y)> getHoughLineIntersections(List<Vector2> peaks, byte minIntensity, ushort maxGap, ushort minSegLength)
        {
            List<(int X, int Y)> intersectionPoints = new List<(int X, int Y)>();
            List<LineSegment> lineSegments = getLineSegments(peaks, minIntensity, maxGap, minSegLength);

            foreach(LineSegment a in lineSegments)
            {
                foreach (LineSegment b in lineSegments)
                {
                    
                    if (a.theta == b.theta && a.r == b.r ) continue;

                    float thetaA = a.theta, thetaB = b.theta;
                    float rA = a.r, rB = b.r;

                    if (Math.Sin(thetaB - thetaA) == 0) continue;

                    float factor = 1 / (float)Math.Sin(thetaB - thetaA);

                    float x0= factor * (float)(rA * Math.Sin(thetaB) - rB * Math.Sin(thetaA));
                    float y0 = factor * (float)(rB * Math.Cos(thetaA) - rA * Math.Cos(thetaB));

                    int x = (int)Math.Round(x0);
                    int y = (int)Math.Round(y0);


                    bool aValid = a.validPoint(x, y);
                    bool bValid = b.validPoint(x, y);

                    if (a.validPoint(x + (width / 2), (height / 2) - y) && b.validPoint(x + (width / 2), (height / 2) - y))
                        intersectionPoints.Add((x + (width / 2), (height / 2) - y));
                }
            }

            return intersectionPoints;
        }

        public RGBImage drawPoints(List<(int X, int Y)> points, Color color)
        {
            Bitmap outputImage = this.getImage();
            int offset = 3;

            foreach ((int X, int Y) in points)
            {
                for (int xOffset = -offset; xOffset <= offset; xOffset++)
                    for (int yOffset = -offset; yOffset <= offset; yOffset++)
                        if (X + xOffset >= 0 && X + xOffset < width && Y + yOffset >= 0 && Y + yOffset < height)
                            outputImage.SetPixel(X + xOffset, Y + yOffset, color);
            }


            return new RGBImage(outputImage);
        }
    }
}
