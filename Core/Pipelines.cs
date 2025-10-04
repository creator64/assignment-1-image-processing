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
        public static ProcessingImage peakFinding(ProcessingImage accumulatorArray, byte t_peak)
        {
            bool[,] structElem = {
                { false, true, false},
                { true, false, true},
                { false, true, false}
            };
            return accumulatorArray.halfThresholdImage(t_peak).binaryCloseImage(structElem);
        }
    }
}