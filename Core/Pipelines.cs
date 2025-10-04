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
    }
}