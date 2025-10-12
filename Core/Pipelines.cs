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
        public static List<Vector2> peakFinding(ProcessingImage accumulatorArray, byte t_peak, ImageRegionFinder regionFinder = null)
        {
            if (regionFinder == null)
                regionFinder = new FloodFill();

            bool[,] structElem = {
                { false, true, false},
                { true, false, true},
                { false, true, false}
            };
            ProcessingImage processingImage = accumulatorArray.halfThresholdImage(t_peak).binaryCloseImage(structElem);

            return processingImage.toRegionalImage(regionFinder).getThetaRPairs();
        }

        public static ProcessingImage BinaryPipeLine(ProcessingImage processingImage, byte minIntensity, ushort minSegLength, ushort maxGap, byte t_mag, byte t_peak, ImageRegionFinder regionFinder)
        {

            ProcessingImage imageA = processingImage.adjustContrast();

            float[,] gaussianFilter = ProcessingImage.createGaussianFilter(5, 2.25f);
            float[,] horizontalKernel = {
                { -1, -2, -1},
                { 0, 0, 0},
                { 1, 2, 1}
            };

            float[,] verticalKernel = {
                { -1, 0, 1},
                { -2, 0, 2},
                { -1, 0, 1}
            };

            ProcessingImage binaryEdgeMap = imageA.convolveImage(gaussianFilter).edgeMagnitude(horizontalKernel, verticalKernel).thresholdImage(t_mag);

            HoughTransform htDrawLines = new HoughTransform(binaryEdgeMap.toArray(), binaryEdgeMap.width * 2, binaryEdgeMap.height * 2);
            ProcessingImage accumulatorArray = htDrawLines.houghTransform();
            List<Vector2> peaks = htDrawLines.peakFinding(accumulatorArray, t_peak, regionFinder).ThetaRPairs;
            Bitmap outputImage = htDrawLines.houghLineSegments(peaks, minIntensity, minSegLength, maxGap);
            return new RGBProcessingImage(binaryEdgeMap.toArray(), outputImage);;
        }
    }
}