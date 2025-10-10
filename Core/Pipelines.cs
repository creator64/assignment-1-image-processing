using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using INFOIBV;
using INFOIBV.Helper_Code;
using static System.Net.Mime.MediaTypeNames;
namespace INFOIBV.Core
{
    public static class Pipelines
    {
        public static ProcessingImage GaussianFilterAndEdgeDetection(
                decimal sigma, decimal gaussianMatrixSize, 
                float[,] horizontalKernel, float[,] verticalKernel,
                ProcessingImage processingImage
            )
        {
            float[,] gaussianFilter = ProcessingImage.createGaussianFilter((byte)gaussianMatrixSize, (float)sigma);
            return processingImage
                .convolveImage(gaussianFilter)
                .edgeMagnitude(horizontalKernel, verticalKernel)
                .thresholdImage(30);
        }

        /// <param name="accumulatorArray">the input image</param>
        /// <param name="t_peak">the intensity value of the peak threshold</param>
        public static List<Vector2> peakFinding(ProcessingImage accumulatorArray, byte t_peak, ImageRegionFinder regionFinder = null)
        {
            if (regionFinder == null)
                regionFinder = new FloodFill();

            bool[,] structElem = {
                { false, true, false},
                { true, false, true},
                { false, true, false}
            };
            ProcessingImage processingImage =  accumulatorArray.halfThresholdImage(t_peak).binaryCloseImage(structElem);

            return processingImage.toRegionalImage(regionFinder).getThetaRPairs();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgeMap">A grayscale edge-magnitude image or a binary edge map</param>
        /// <param name="peaks">List of peaks in (theta, r) format</param>
        /// <param name="minIntensity">A minimum intensity threshold (for grayscale images)</param>
        /// <param name="minSegLength">A minimum segment length</param>
        /// <param name="maxGap">A maximum gap parameter</param>
        /// <returns>An RGB image with the detected line segments overlaid on top the input image (the edge map) in a red colour</returns>
        public static Bitmap houghLineSegments(byte[,] edgeMap, List<Vector2> peaks, byte minIntensity, ushort minSegLength, ushort maxGap)
        {
            // List<Vector2> highestPeaks = peaks.Take(25).ToList();

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

                longSegments.Add(currentSegment);
            }

            Debug.WriteLine($"longSegments length: {longSegments.Count}");


            List<LineSegment> shortSegments = new List<LineSegment>();

            foreach (LineSegment seg in longSegments)
                foreach (LineSegment subseg in seg.getSegments(width, height))
                    shortSegments.Add(subseg);

            Debug.WriteLine($"shortSegments length: {shortSegments.Count}");

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