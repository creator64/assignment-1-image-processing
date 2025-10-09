using System.Collections.Generic;
using System.Numerics;
using INFOIBV;
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
    }
}