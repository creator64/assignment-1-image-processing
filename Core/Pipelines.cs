using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using INFOIBV.Core.Main;
using INFOIBV.Core.TemplateMatching;
using INFOIBV.Helper_Code;

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

        public static RGBImage BinaryPipeline(ProcessingImage processingImage, int thetaDetail, int rDetail, byte minIntensity, ushort minSegLength, ushort maxGap, byte t_mag, byte t_peak, ImageRegionFinder regionFinder)
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

            HoughTransform htDrawLines = binaryEdgeMap.toHoughTransform(thetaDetail, rDetail);
            ProcessingImage accumulatorArray = htDrawLines.houghTransform();
            List<Vector2> peaks = Pipelines.peakFinding(accumulatorArray, t_peak);
            return htDrawLines.houghLineSegments(peaks, minIntensity, minSegLength, maxGap);
        }

        public static RGBImage GrayscalePipeline(ProcessingImage processingImage, int thetaDetail, int rDetail, byte minIntensity, ushort minSegLength, ushort maxGap, byte t_peak, ImageRegionFinder regionFinder)
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

            ProcessingImage grayscaleEdgeMap = imageA.convolveImage(gaussianFilter).edgeMagnitude(horizontalKernel, verticalKernel);

            HoughTransform htDrawLines = grayscaleEdgeMap.toHoughTransform(thetaDetail, rDetail);
            ProcessingImage accumulatorArray = htDrawLines.houghTransform();
            List<Vector2> peaks = Pipelines.peakFinding(accumulatorArray, t_peak);
            return htDrawLines.houghLineSegments(peaks, minIntensity, minSegLength, maxGap);
        }

        public static RGBImage simpleCuneiDetection(ProcessingImage processingImage)
        {
            //--------------------------------------------------------
            //  Step 1 & 2: Binarisation (thresholding) & Pre-processing (despeckling via bilateralSmoothing)
            //--------------------------------------------------------
            ProcessingImage binaryEdgeMap = processingImage
                .adjustContrast()
                .bilateralSmoothing(2, 50)
                .otsuThreshold()
                .invertImage();

            //--------------------------------------------------------
            //  Step 3: Segmentation (Detect regions of text)
            //--------------------------------------------------------
            String baseDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            TemplateMatchingImage templateImage = ProcessingImage.fromBitmap(
                new Bitmap(
                    Path.Combine(baseDirectory, "images", "cunei_template_type_1.bmp")
                )
            ).toTemplateMatchingImage();
            
            // Segmentator segmentator = new SimpleConnectionSegmentator(binaryEdgeMap, (templateImage.width, templateImage.height));
            // Segmentator segmentator = new TestSegmentator(binaryEdgeMap);
            Segmentator segmentator = new ProjectionSegmentator(binaryEdgeMap);
            List<SubImage> segments = segmentator.segments;

            //--------------------------------------------------------
            //  Step 4: Feature Extraction (Template Matching)
            //--------------------------------------------------------

            return binaryEdgeMap.toTemplateMatchingImage().visualiseMatchesBinary(templateImage,
                threshold: 0.45,
                pointsToCheck: segments
            );
        }
    }
}