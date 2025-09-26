namespace INFOIBV.Core
{
    public static class Pipelines
    {
        public static byte[,] GaussianFilterAndEdgeDetection(
                decimal sigma, decimal gaussianMatrixSize, 
                float[,] horizontalKernel, float[,] verticalKernel,
                byte[,] workingImage
            )
        {
            float[,] gaussianFilter = ProcessingFunctions.createGaussianFilter((byte)gaussianMatrixSize, (float)sigma);
            byte[,] convolvedImage = ProcessingFunctions.convolveImage(workingImage, gaussianFilter);
            byte[,] edgedImage = ProcessingFunctions.edgeMagnitude(convolvedImage, horizontalKernel, verticalKernel);
            return ProcessingFunctions.thresholdImage(edgedImage, 30);
        }
    }
}